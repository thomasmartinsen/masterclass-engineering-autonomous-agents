namespace CaseHandling.Domain;

public sealed record Case
{
    public required string Id { get; init; }

    public required string CustomerId { get; init; }

    public CaseCategory Category { get; init; }

    public string Description { get; init; } = "";

    public DateOnly? IncidentDate { get; init; }

    public DateOnly ReportedDate { get; init; }

    public decimal? ClaimedAmount { get; init; }

    public string Currency { get; init; } = "DKK";

    public CaseStatus Status { get; init; } = CaseStatus.Received;

    public IReadOnlyList<CaseNote> Notes { get; init; } = [];
}

public enum CaseCategory
{
    Other,
    WaterDamage,
    Theft,
    TravelDelay,
    Fire,
}

public enum CaseStatus
{
    Received,
    UnderReview,
    AwaitingInformation,
    Approved,
    Rejected,
}

public sealed record CaseNote(DateTimeOffset CreatedAt, string Author, string Text);
