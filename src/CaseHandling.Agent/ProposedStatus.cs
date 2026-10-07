using System.Text.Json.Serialization;
using CaseHandling.Domain;

namespace CaseHandling.Agent;

/// <summary>The only statuses the agent may propose. Approved and Rejected are human decisions.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProposedStatus>))]
public enum ProposedStatus
{
    UnderReview,
    AwaitingInformation,
}

public static class ProposedStatusExtensions
{
    public static CaseStatus ToCaseStatus(this ProposedStatus status) => status switch
    {
        ProposedStatus.UnderReview => CaseStatus.UnderReview,
        ProposedStatus.AwaitingInformation => CaseStatus.AwaitingInformation,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Not a status the agent may propose."),
    };
}
