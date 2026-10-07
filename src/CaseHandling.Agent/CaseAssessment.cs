namespace CaseHandling.Agent;

public sealed record CaseAssessment
{
    public string? CaseId { get; init; }

    /// <summary>Null when there is not enough evidence to propose anything, for example when the case cannot be read.</summary>
    public required ProposedStatus? ProposedStatus { get; init; }

    public IReadOnlyList<string> MissingInformation { get; init; } = [];

    public string? CustomerQuestion { get; init; }

    public IReadOnlyList<string> PolicyIds { get; init; } = [];

    public required string Reasoning { get; init; }
}
