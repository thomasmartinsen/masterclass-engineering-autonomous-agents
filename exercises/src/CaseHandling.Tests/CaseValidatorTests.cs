using CaseHandling.Domain;

namespace CaseHandling.Tests;

public sealed class CaseValidatorTests
{
    private static Case ValidCase() => new()
    {
        Id = "C-TEST",
        CustomerId = "CUST-1",
        Category = CaseCategory.WaterDamage,
        Description = "Burst pipe in the kitchen.",
        IncidentDate = new DateOnly(2026, 9, 1),
        ReportedDate = new DateOnly(2026, 9, 2),
        ClaimedAmount = 1000m,
    };

    [Fact]
    public void Valid_case_has_no_issues()
    {
        Assert.Empty(CaseValidator.Validate(ValidCase()));
    }

    [Fact]
    public void Missing_description_is_reported()
    {
        var issues = CaseValidator.Validate(ValidCase() with { Description = " " });

        Assert.Contains(issues, i => i.Field == nameof(Case.Description));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Non_positive_claimed_amount_is_reported(decimal amount)
    {
        var issues = CaseValidator.Validate(ValidCase() with { ClaimedAmount = amount });

        Assert.Contains(issues, i => i.Field == nameof(Case.ClaimedAmount));
    }
}
