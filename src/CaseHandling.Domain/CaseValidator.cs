namespace CaseHandling.Domain;

public sealed record ValidationIssue(string Field, string Message);

public static class CaseValidator
{
    private static readonly string[] _supportedCurrencies = ["DKK", "EUR", "SEK", "NOK"];

    public static IReadOnlyList<ValidationIssue> Validate(Case @case)
    {
        var issues = new List<ValidationIssue>();

        if (string.IsNullOrWhiteSpace(@case.CustomerId))
        {
            issues.Add(new(nameof(Case.CustomerId), "Customer id is required."));
        }

        if (string.IsNullOrWhiteSpace(@case.Description))
        {
            issues.Add(new(nameof(Case.Description), "A description of the incident is required."));
        }

        if (@case.ClaimedAmount is <= 0)
        {
            issues.Add(new(nameof(Case.ClaimedAmount), "Claimed amount must be greater than zero."));
        }

        if (!_supportedCurrencies.Contains(@case.Currency))
        {
            issues.Add(new(nameof(Case.Currency), $"Currency '{@case.Currency}' is not supported."));
        }

        return issues;
    }
}
