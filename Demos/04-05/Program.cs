using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

if (args.Contains("--self-test"))
{
    Check("{\"caseId\":\"C-101\",\"status\":\"needs_information\",\"question\":\"Which account is affected?\",\"proposedNextStep\":null}", "C-101");
    Check("{\"caseId\":\"C-100\",\"status\":\"ready\",\"question\":null,\"proposedNextStep\":\"Arrange a replacement\"}", "C-100");
    MustReject("{\"caseId\":\"C-999\",\"status\":\"ready\",\"proposedNextStep\":\"Close case\"}", "C-100");
    MustReject("{\"caseId\":\"C-101\",\"status\":\"needs_information\"}", "C-101");
    Console.WriteLine("Decision-contract checks passed.");
    return;
}

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL.");

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(model: model, name: "ReviewedCaseIntake", instructions: """
        You are a case-intake agent. Do not invent facts or update a case.
        Return JSON only with caseId, status, question, proposedNextStep.
        Status is "ready" only when the next step has enough facts; otherwise use "needs_information" and ask one question.
        Use null for an inapplicable field. Keep the case ID from the user.
        """);

AgentSession session = await agent.CreateSessionAsync();
AgentResponse firstResponse = await agent.RunAsync("C-101: Employee cannot access the portal. The affected account is unknown.", session);
var first = firstResponse.ToString();
Console.WriteLine($"First turn: {first}");
TokenUsageReporter.Write(firstResponse);
var firstDecision = CaseDecision.Parse(first, "C-101");
if (firstDecision.Status != "needs_information")
    throw new InvalidDataException("Expected a clarifying question for C-101.");

AgentResponse secondResponse = await agent.RunAsync("The account is alex@example.test and the error is 'account disabled'.", session);
var second = secondResponse.ToString();
Console.WriteLine($"Second turn: {second}");
TokenUsageReporter.Write(secondResponse);
CaseDecision.Parse(second, "C-101");

static void Check(string text, string expectedId) => CaseDecision.Parse(text, expectedId);
static void MustReject(string text, string expectedId)
{
    try { CaseDecision.Parse(text, expectedId); }
    catch (InvalidDataException) { return; }
    throw new Exception("Invalid decision was accepted.");
}
