namespace CaseHandling.Agent;

public sealed record CaseAssessment
{
    public string? CaseId { get; init; }

    public required ProposedStatus ProposedStatus { get; init; }

    public IReadOnlyList<string> MissingInformation { get; init; } = [];

    public string? CustomerQuestion { get; init; }

    public IReadOnlyList<string> PolicyIds { get; init; } = [];

    public required string Reasoning { get; init; }
}
