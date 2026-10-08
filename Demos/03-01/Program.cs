using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT to the prepared project endpoint.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL to the deployed model name.");

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(model: model, name: "ConnectionCheck", instructions: "Reply with one short sentence confirming that this is a model response.");
AgentResponse response = await agent.RunAsync("Confirm the workshop connection.");
Console.WriteLine(response);
TokenUsageReporter.Write(response);
