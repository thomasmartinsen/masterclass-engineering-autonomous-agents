namespace CaseHandling.Domain;

public static class CaseStatusTransitions
{
    private static readonly Dictionary<CaseStatus, CaseStatus[]> _allowed = new()
    {
        [CaseStatus.Received] = [CaseStatus.UnderReview, CaseStatus.AwaitingInformation],
        [CaseStatus.UnderReview] = [CaseStatus.AwaitingInformation, CaseStatus.Approved, CaseStatus.Rejected],
        [CaseStatus.AwaitingInformation] = [CaseStatus.UnderReview],
        [CaseStatus.Approved] = [],
        [CaseStatus.Rejected] = [],
    };

    public static bool IsAllowed(CaseStatus from, CaseStatus to) =>
        _allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    public static IReadOnlyList<CaseStatus> AllowedFrom(CaseStatus from) =>
        _allowed.TryGetValue(from, out var targets) ? targets : [];

    /// <summary>Approved and Rejected are decisions and require explicit human approval.</summary>
    public static bool IsDecision(CaseStatus status) =>
        status is CaseStatus.Approved or CaseStatus.Rejected;
}
