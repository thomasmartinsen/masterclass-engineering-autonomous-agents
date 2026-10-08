using System.Text.Json;

public sealed record CaseDecision(string? CaseId, string? Status, string? Question, string? ProposedNextStep)
{
    public static CaseDecision Parse(string text, string expectedCaseId)
    {
        var decision = JsonSerializer.Deserialize<CaseDecision>(text, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new InvalidDataException("The agent returned no decision.");

        if (!string.Equals(decision.CaseId, expectedCaseId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The agent returned a decision for a different case.");
        if (decision.Status == "needs_information" && string.IsNullOrWhiteSpace(decision.Question))
            throw new InvalidDataException("Missing-information decisions need a question.");
        if (decision.Status == "ready" && string.IsNullOrWhiteSpace(decision.ProposedNextStep))
            throw new InvalidDataException("Ready decisions need a proposed next step.");
        if (decision.Status is not ("ready" or "needs_information"))
            throw new InvalidDataException("Unexpected decision status.");
        return decision;
    }
}
