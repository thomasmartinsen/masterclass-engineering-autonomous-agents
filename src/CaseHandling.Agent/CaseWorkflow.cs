using Microsoft.Agents.AI.Workflows;

namespace CaseHandling.Agent;

public sealed record ApprovalRequest(CaseAssessment Assessment);

public sealed record ApprovalDecision(CaseAssessment Assessment, bool Approved);

public sealed record CaseOutcome(CaseAssessment Assessment, bool Approved, WriteResult? Write);

public interface IApprover
{
    Task<bool> ApproveAsync(CaseAssessment assessment, CancellationToken cancellationToken);
}

/// <summary>
/// Investigate and propose (model) -> approve (human) -> write (code).
/// The agent has no write tool; only the write step can change a case, and only after approval.
/// </summary>
public static class CaseWorkflow
{
    public static Workflow Build(
        Func<string, CancellationToken, Task<CaseAssessment>> investigate,
        CaseStatusWriter writer)
    {
        var investigateStep = ((Func<string, CancellationToken, ValueTask<ApprovalRequest>>)(async (input, cancellationToken) =>
            new ApprovalRequest(await investigate(input, cancellationToken)))).BindAsExecutor("Investigate");

        var nothingToApproveStep = new FunctionExecutor<ApprovalRequest>(
            "NothingToApprove",
            (request, context, cancellationToken) =>
                context.YieldOutputAsync(new CaseOutcome(request.Assessment, Approved: false, Write: null), cancellationToken),
            outputTypes: [typeof(CaseOutcome)]);

        var approvalStep = RequestPort.Create<ApprovalRequest, ApprovalDecision>("Approve");

        var writeStep = new FunctionExecutor<ApprovalDecision>(
            "Write",
            async (decision, context, cancellationToken) =>
            {
                WriteResult? write = null;
                if (decision.Approved && decision.Assessment is { CaseId: { } caseId, ProposedStatus: { } status })
                {
                    using var activity = Tracing.Source.StartActivity("write_status");
                    activity?.SetTag("case.id", caseId);
                    activity?.SetTag("case.status", status.ToString());

                    write = await writer.SetStatusAsync(
                        caseId,
                        status.ToCaseStatus(),
                        $"Approved by case handler. {decision.Assessment.Reasoning}",
                        cancellationToken);

                    activity?.SetTag("write.succeeded", write.Succeeded);
                }

                await context.YieldOutputAsync(new CaseOutcome(decision.Assessment, decision.Approved, write), cancellationToken);
            },
            outputTypes: [typeof(CaseOutcome)]);

        // Without a case id or a proposed status there is nothing to approve or write.
        return new WorkflowBuilder(investigateStep)
            .AddEdge<ApprovalRequest>(investigateStep, approvalStep, r => r!.Assessment is { CaseId: not null, ProposedStatus: not null })
            .AddEdge<ApprovalRequest>(investigateStep, nothingToApproveStep, r => r!.Assessment is not { CaseId: not null, ProposedStatus: not null })
            .AddEdge(approvalStep, writeStep)
            .WithOutputFrom(nothingToApproveStep, writeStep)
            .Build();
    }

    public static async Task<CaseOutcome> RunAsync(Workflow workflow, string input, IApprover approver, CancellationToken cancellationToken = default)
    {
        CaseOutcome? outcome = null;
        using var workflowActivity = Tracing.Source.StartActivity("case_workflow");

        await using var run = await InProcessExecution.RunStreamingAsync(workflow, input, cancellationToken: cancellationToken);
        await foreach (var evt in run.WatchStreamAsync(cancellationToken))
        {
            switch (evt)
            {
                case RequestInfoEvent { Request: var request } when request.TryGetDataAs<ApprovalRequest>(out var approval):
                    bool approved;
                    using (var approvalActivity = Tracing.Source.StartActivity("approve"))
                    {
                        approvalActivity?.SetTag("case.id", approval.Assessment.CaseId);
                        approvalActivity?.SetTag("case.proposed_status", approval.Assessment.ProposedStatus?.ToString());
                        approved = await approver.ApproveAsync(approval.Assessment, cancellationToken);
                        approvalActivity?.SetTag("approval.approved", approved);
                    }

                    await run.SendResponseAsync(request.CreateResponse(new ApprovalDecision(approval.Assessment, approved)));
                    break;
                case WorkflowOutputEvent output when output.Is<CaseOutcome>(out var result):
                    outcome = result;
                    break;
                case WorkflowErrorEvent { Exception: var exception }:
                    throw new InvalidOperationException("The case workflow failed.", exception);
                case ExecutorFailedEvent failed:
                    throw new InvalidOperationException($"Workflow step '{failed.ExecutorId}' failed.", failed.Data as Exception);
            }

            if (outcome is not null)
            {
                break;
            }
        }

        if (outcome is null)
        {
            throw new InvalidOperationException("The case workflow ended without an outcome.");
        }

        workflowActivity?.SetTag("case.id", outcome.Assessment.CaseId);
        workflowActivity?.SetTag("approval.approved", outcome.Approved);
        workflowActivity?.SetTag("write.succeeded", outcome.Write?.Succeeded ?? false);
        return outcome;
    }
}
