using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL.");

const string instructions = """
You are a case-intake agent. Do not invent case facts.
Reply with JSON only, without Markdown, using exactly these properties:
caseId, status, question, proposedNextStep.
Use status "needs_information" and a specific question if a fact needed for a next step is missing.
Use status "ready" and a concrete proposedNextStep when the facts are sufficient.
For a missing value use null. Never update a case; propose only.
""";

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(model: model, name: "CaseIntake", instructions: instructions);

Console.WriteLine("Complete case:");
AgentResponse complete = await agent.RunAsync(
    "C-100: Customer's laptop will not start. The customer can be reached at 555-0100 and needs a replacement tomorrow.");
Console.WriteLine(complete);
TokenUsageReporter.Write(complete);

AgentSession session = await agent.CreateSessionAsync();
Console.WriteLine("\nIncomplete case, first turn:");
AgentResponse first = await agent.RunAsync(
    "C-101: Employee cannot access the portal. We do not know which account is affected.", session);
Console.WriteLine(first);
TokenUsageReporter.Write(first);

Console.WriteLine("\nSame case, second turn in the same session:");
AgentResponse second = await agent.RunAsync(
    "The affected account is alex@example.test and the error is 'account disabled'.", session);
Console.WriteLine(second);
TokenUsageReporter.Write(second);
