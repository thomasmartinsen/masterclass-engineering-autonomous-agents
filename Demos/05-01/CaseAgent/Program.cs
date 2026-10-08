using System.ComponentModel;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL.");
var cases = new CaseLookup();
AIAgent baseAgent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(
        model: model,
        name: "CaseWithFunctionTool",
        instructions: """
            You investigate workshop cases but never update them. Call GetCase for the requested case ID.
            Use only the returned facts. If the tool fails, explain the failure without inventing a case.
            """,
        tools: [AIFunctionFactory.Create(cases.GetCase)]);

#if DEBUG
AIAgent agent = baseAgent.AsBuilder().Use(LogToolCallAsync).Build();
#else
AIAgent agent = baseAgent;
#endif

Console.WriteLine("Internal C# function tool:");
AgentResponse caseResponse = await agent.RunAsync("Read case C-100 and summarize the next question to ask.");
Console.WriteLine(caseResponse);
TokenUsageReporter.Write(caseResponse);

#if DEBUG
static async ValueTask<object?> LogToolCallAsync(
    AIAgent _,
    FunctionInvocationContext context,
    Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
    CancellationToken cancellationToken)
{
    string arguments = context.Arguments.Count == 0
        ? ""
        : " (Args: " + string.Join(", ", context.Arguments.Select(argument =>
            $"{argument.Key} = {argument.Value}")) + ")";
    Console.WriteLine($"[debug] Tool call: '{context.Function.Name}'{arguments}");
    return await next(context, cancellationToken);
}
#endif

public sealed class CaseLookup
{
    [Description("Read one workshop case by its case ID. This tool never updates the case.")]
    public string GetCase([Description("Case ID, for example C-100.")] string caseId)
    {
        return caseId switch
        {
            "C-100" => "C-100: Customer laptop will not start. Contact number 555-0100. Replacement is needed tomorrow.",
            "C-101" => "C-101: Employee cannot access portal. Affected account is unknown.",
            _ => throw new ArgumentException("Unknown case ID", nameof(caseId)),
        };
    }
}
