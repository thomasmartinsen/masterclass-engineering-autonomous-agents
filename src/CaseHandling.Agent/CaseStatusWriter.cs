using System.Net.Http.Json;
using CaseHandling.Domain;

namespace CaseHandling.Agent;

public sealed record WriteResult(bool Succeeded, string Message);

/// <summary>The only code that changes a case. It is used by the workflow after approval and is never given to the agent.</summary>
public sealed class CaseStatusWriter(HttpClient http)
{
    public async Task<WriteResult> SetStatusAsync(string caseId, CaseStatus status, string reason, CancellationToken cancellationToken = default)
    {
        if (CaseStatusTransitions.IsDecision(status))
        {
            return new(false, $"Refused: {status} is a decision for a case handler and is never written by the workflow.");
        }

        if (!CaseTools.IsValidCaseId(caseId))
        {
            return new(false, $"Refused: '{caseId}' is not a valid case id.");
        }

        try
        {
            using var response = await http.PutAsJsonAsync(
                $"cases/{caseId}/status",
                new { status = status.ToString(), reason },
                cancellationToken);

            return response.IsSuccessStatusCode
                ? new(true, $"Case {caseId} set to {status}.")
                : new(false, $"Case API returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(cancellationToken)}");
        }
        catch (HttpRequestException ex)
        {
            return new(false, $"Case API is unavailable: {ex.Message}");
        }
    }
}
