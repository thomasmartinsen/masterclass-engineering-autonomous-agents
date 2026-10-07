namespace CaseHandling.Agent;

/// <summary>Asks the case handler in the console to approve or reject a proposal.</summary>
public sealed class ConsoleApprover : IApprover
{
    public Task<bool> ApproveAsync(CaseAssessment assessment, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[approve] Set case {assessment.CaseId} to {assessment.ProposedStatus}? Type 'yes' to approve, anything else rejects.");
        Console.Write("[approve] > ");
        var answer = Console.ReadLine();
        var approved = string.Equals(answer?.Trim(), "yes", StringComparison.OrdinalIgnoreCase);
        Console.WriteLine(approved ? "[approve] Approved by case handler." : "[approve] Rejected by case handler. Nothing is written.");
        return Task.FromResult(approved);
    }
}
