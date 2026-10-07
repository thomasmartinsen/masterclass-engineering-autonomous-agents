using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;

namespace CaseHandling.Agent;

/// <summary>Read-only function tool over the Case API. It never throws; failures become messages the model can act on.</summary>
public sealed partial class CaseTools(HttpClient http)
{
    public const string UnavailableMessage =
        "Error: the Case API is unavailable right now. Do not guess any case details. Tell the case handler that the case could not be read and to try again later.";

    [Description("Reads one insurance case from the Case API by its id, for example C-1001. Returns the case as JSON: description, category, incident date, reported date, claimed amount, currency, status, and notes. Returns a message starting with 'Error:' if the id is invalid, the case does not exist, or the Case API is unavailable.")]
    public async Task<string> GetCase(
        [Description("The case id, in the form C-1234.")] string caseId,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidCaseId(caseId))
        {
            return $"Error: '{caseId}' is not a valid case id. Case ids look like C-1001.";
        }

        try
        {
            using var response = await http.GetAsync($"cases/{caseId}", cancellationToken);
            return response.StatusCode switch
            {
                HttpStatusCode.OK => await response.Content.ReadAsStringAsync(cancellationToken),
                HttpStatusCode.NotFound => $"Error: no case with id '{caseId}' exists. Ask the case handler to check the id.",
                HttpStatusCode.ServiceUnavailable => UnavailableMessage,
                _ => $"Error: the Case API returned status {(int)response.StatusCode}. Do not guess any case details.",
            };
        }
        catch (HttpRequestException)
        {
            return UnavailableMessage;
        }
    }

    public static bool IsValidCaseId(string? caseId) => caseId is not null && CaseIdPattern().IsMatch(caseId);

    [GeneratedRegex(@"^C-\d{4,8}$")]
    private static partial Regex CaseIdPattern();
}
