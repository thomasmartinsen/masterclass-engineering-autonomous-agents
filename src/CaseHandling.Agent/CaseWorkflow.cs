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

        var noCaseStep = new FunctionExecutor<ApprovalRequest>(
            "NoCase",
            (request, context, cancellationToken) =>
                context.YieldOutputAsync(new CaseOutcome(request.Assessment, Approved: false, Write: null), cancellationToken),
            outputTypes: [typeof(CaseOutcome)]);

        var approvalStep = RequestPort.Create<ApprovalRequest, ApprovalDecision>("Approve");

        var writeStep = new FunctionExecutor<ApprovalDecision>(
            "Write",
            async (decision, context, cancellationToken) =>
            {
                WriteResult? write = null;
                if (decision.Approved && decision.Assessment.CaseId is { } caseId)
                {
                    write = await writer.SetStatusAsync(
                        caseId,
                        decision.Assessment.ProposedStatus.ToCaseStatus(),
                        $"Approved by case handler. {decision.Assessment.Reasoning}",
                        cancellationToken);
                }

                await context.YieldOutputAsync(new CaseOutcome(decision.Assessment, decision.Approved, write), cancellationToken);
            },
            outputTypes: [typeof(CaseOutcome)]);

        // Without a case id there is nothing to approve or write.
        return new WorkflowBuilder(investigateStep)
            .AddEdge<ApprovalRequest>(investigateStep, approvalStep, r => r!.Assessment.CaseId is not null)
            .AddEdge<ApprovalRequest>(investigateStep, noCaseStep, r => r!.Assessment.CaseId is null)
            .AddEdge(approvalStep, writeStep)
            .WithOutputFrom(noCaseStep, writeStep)
            .Build();
    }

    public static async Task<CaseOutcome> RunAsync(Workflow workflow, string input, IApprover approver, CancellationToken cancellationToken = default)
    {
        CaseOutcome? outcome = null;

        await using var run = await InProcessExecution.RunStreamingAsync(workflow, input, cancellationToken: cancellationToken);
        await foreach (var evt in run.WatchStreamAsync(cancellationToken))
        {
            switch (evt)
            {
                case RequestInfoEvent { Request: var request } when request.TryGetDataAs<ApprovalRequest>(out var approval):
                    var approved = await approver.ApproveAsync(approval.Assessment, cancellationToken);
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

        return outcome ?? throw new InvalidOperationException("The case workflow ended without an outcome.");
    }
}
