using System.Text.Json;
using CaseHandling.Agent;
using CaseHandling.Domain;

namespace CaseHandling.Tests;

public sealed class CaseAssessmentTests
{
    [Fact]
    public void Proposed_status_cannot_be_a_decision()
    {
        var statuses = Enum.GetValues<ProposedStatus>();

        Assert.All(statuses, s => Assert.False(CaseStatusTransitions.IsDecision(s.ToCaseStatus())));
    }

    [Theory]
    [InlineData(ProposedStatus.UnderReview, CaseStatus.UnderReview)]
    [InlineData(ProposedStatus.AwaitingInformation, CaseStatus.AwaitingInformation)]
    public void Proposed_status_maps_to_case_status(ProposedStatus proposed, CaseStatus expected)
    {
        Assert.Equal(expected, proposed.ToCaseStatus());
    }

    [Theory]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    public void Decision_status_cannot_be_deserialized(string status)
    {
        var json = $$"""{ "ProposedStatus": "{{status}}", "Reasoning": "r" }""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CaseAssessment>(json));
    }

    [Fact]
    public void Proposed_status_serializes_as_name()
    {
        var assessment = new CaseAssessment { ProposedStatus = ProposedStatus.AwaitingInformation, Reasoning = "r" };

        var json = JsonSerializer.Serialize(assessment);

        Assert.Contains("\"AwaitingInformation\"", json);
    }
}
