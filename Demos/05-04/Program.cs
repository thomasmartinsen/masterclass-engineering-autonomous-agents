using System.ComponentModel;
using System.Text.Json;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL.");

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(model: model, name: "CaseClassifier", instructions: """
        Call GetCase for the requested case ID, then return a structured triage result from those facts.
        Do not invent a policy or claim that you have updated a case.
        Use NeedsMoreInformation when facts are insufficient for a recommendation.
        """, tools: [AIFunctionFactory.Create(new CaseLookup().GetCase)]);

AgentResponse<CaseTriage> response = await agent.RunAsync<CaseTriage>(
    "Read case C-100 and return structured triage.");
TokenUsageReporter.Write(response);
CaseTriage result = response.Result;

if (result.CaseId != "C-100" || string.IsNullOrWhiteSpace(result.Summary))
    throw new InvalidDataException("The structured output lacks the expected case ID or summary.");

Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

public sealed class CaseLookup
{
    [Description("Read one workshop case by its ID; never updates the case.")]
    public string GetCase([Description("Case ID such as C-100.")] string caseId)
    {
        Console.WriteLine($"GetCase({caseId})");
        return caseId == "C-100"
            ? "C-100: Customer laptop will not start. Replacement needed tomorrow. Contact 555-0100."
            : throw new ArgumentException("Unknown case ID", nameof(caseId));
    }
}

public sealed class CaseTriage
{
    public string CaseId { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Priority { get; set; } = "";
    public bool NeedsMoreInformation { get; set; }
    public string NextQuestion { get; set; } = "";
}
