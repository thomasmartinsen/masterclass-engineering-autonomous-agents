using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL.");

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(model: model, name: "CaseChat", instructions: """
        You help investigate workshop cases. Answer using only facts supplied in the conversation.
        If a company policy is requested but has not been supplied, say you do not have that policy.
        Never imply that you have read a case system or policy repository.
        """);

AgentSession session = await agent.CreateSessionAsync();
Console.WriteLine("Question 1: general case discussion");
AgentResponse first = await agent.RunAsync(
    "A customer says their laptop will not start and they need a replacement tomorrow. What should I ask next?",
    session);
Console.WriteLine(first);
TokenUsageReporter.Write(first);
Console.WriteLine("\nQuestion 2: information the chat cannot know yet");
AgentResponse second = await agent.RunAsync(
    "What does our internal replacement policy require for this customer?", session);
Console.WriteLine(second);
TokenUsageReporter.Write(second);
