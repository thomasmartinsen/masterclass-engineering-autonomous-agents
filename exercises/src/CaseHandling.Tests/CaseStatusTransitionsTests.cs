using CaseHandling.Domain;

namespace CaseHandling.Tests;

public sealed class CaseStatusTransitionsTests
{
    [Theory]
    [InlineData(CaseStatus.Received, CaseStatus.UnderReview)]
    [InlineData(CaseStatus.UnderReview, CaseStatus.Approved)]
    [InlineData(CaseStatus.AwaitingInformation, CaseStatus.UnderReview)]
    public void Allowed_transitions(CaseStatus from, CaseStatus to)
    {
        Assert.True(CaseStatusTransitions.IsAllowed(from, to));
    }

    [Theory]
    [InlineData(CaseStatus.Received, CaseStatus.Approved)]
    [InlineData(CaseStatus.Approved, CaseStatus.Rejected)]
    [InlineData(CaseStatus.Rejected, CaseStatus.UnderReview)]
    public void Disallowed_transitions(CaseStatus from, CaseStatus to)
    {
        Assert.False(CaseStatusTransitions.IsAllowed(from, to));
    }
}
