using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;

static string RequiredEnvironmentVariable(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Set {name} to the value of the prepared Foundry Prompt Agent.");

string endpoint = RequiredEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT");
string agentName = RequiredEnvironmentVariable("FOUNDRY_AGENT_NAME");
string agentVersion = RequiredEnvironmentVariable("FOUNDRY_AGENT_VERSION");

var projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());

// The model, instructions, and any hosted tools belong to this versioned agent in Foundry.
// Agent Framework provides the C# AIAgent interface used to invoke it.
FoundryAgent agent = projectClient.AsAIAgent(new AgentReference(agentName, agentVersion));
AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine($"Invoking Foundry agent {agentName} (version {agentVersion}) from C#.");

await RunTurnAsync("A customer's laptop will not start, and they need a replacement tomorrow. What should we check first?");
await RunTurnAsync("The power adapter works, but the laptop still does not start. What is the next step?");

async Task RunTurnAsync(string message)
{
    Console.WriteLine($"\nUser: {message}");
    AgentResponse response = await agent.RunAsync(message, session: session);
    Console.WriteLine($"Agent: {response}");
    TokenUsageReporter.Write(response);
}
