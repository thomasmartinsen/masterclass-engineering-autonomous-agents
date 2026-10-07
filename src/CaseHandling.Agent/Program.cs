using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using CaseHandling.Agent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// Args are the first message, not configuration, so they are not passed to the host.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = AppContext.BaseDirectory,
});

AgentSettings settings;
try
{
    settings = AgentSettings.FromConfiguration(builder.Configuration);
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

if (!Uri.TryCreate(builder.Configuration["CaseApi:BaseUrl"], UriKind.Absolute, out var caseApiBaseUrl))
{
    Console.Error.WriteLine("Missing or invalid configuration 'CaseApi:BaseUrl'. Set it in src/CaseHandling.Agent/appsettings.json.");
    return 1;
}

var instructions = AgentInstructions.Load(AppContext.BaseDirectory);
using var caseApi = new HttpClient { BaseAddress = caseApiBaseUrl };
var toolCalls = new List<ToolCall>();

using var tracerProvider = Tracing.Start(builder.Configuration);
var includeSensitiveData = builder.Configuration.GetValue(Tracing.IncludeSensitiveDataKey, false);
Console.WriteLine(tracerProvider is null
    ? $"Tracing is off. Set {Tracing.ConnectionStringKey} in src/CaseHandling.Agent/appsettings.json to turn it on."
    : $"Tracing to Application Insights (sensitive data: {includeSensitiveData}).");

await using var policyServer = await PolicyServer.StartAsync();
var policyTools = await policyServer.ListToolsAsync();
Console.WriteLine("Policy MCP server tools:");
foreach (var tool in policyTools)
{
    Console.WriteLine($"  {tool.Name}: {tool.Description}");
}

IList<AITool> tools =
[
    new LoggingAIFunction(AIFunctionFactory.Create(new CaseTools(caseApi).GetCase, name: "get_case"), Console.Out, toolCalls),
    .. policyTools.Select(t => new LoggingAIFunction(t, Console.Out, toolCalls)),
];

var projectClient = new AIProjectClient(new Uri(settings.ProjectEndpoint), new AzureCliCredential());

if (builder.Configuration[CarrierTools.AgentNameKey] is { Length: > 0 } carrierAgentName)
{
    AIAgent carrierAgent = projectClient.AsAIAgent(new AgentReference(carrierAgentName));
    var carrierTools = new CarrierTools(async (request, cancellationToken) =>
        (await carrierAgent.RunAsync(request, cancellationToken: cancellationToken)).Text);
    tools.Add(new LoggingAIFunction(AIFunctionFactory.Create(carrierTools.GetCarrierCompensation, name: "get_carrier_compensation"), Console.Out, toolCalls));
    Console.WriteLine($"Foundry agent '{carrierAgentName}' is available as get_carrier_compensation.");
}
else
{
    Console.WriteLine($"No Foundry carrier agent. Set {CarrierTools.AgentNameKey} in src/CaseHandling.Agent/appsettings.json to use it.");
}

AIAgent agent = projectClient
    .AsAIAgent(
        model: settings.ModelDeployment,
        instructions: instructions,
        name: "CaseHandlingAgent",
        tools: tools,
        clientFactory: chatClient => chatClient.AsBuilder()
            .UseOpenTelemetry(sourceName: Tracing.SourceName, configure: c => c.EnableSensitiveData = includeSensitiveData)
            .Build())
    .AsBuilder()
    .UseOpenTelemetry(Tracing.SourceName, a => a.EnableSensitiveData = includeSensitiveData)
    .Build();

// One session for the whole run, so follow-up messages are assessed together with the first one.
var session = await agent.CreateSessionAsync();

var workflow = CaseWorkflow.Build(
    async (input, cancellationToken) =>
    {
        var response = await agent.RunAsync<CaseAssessment>(input, session, cancellationToken: cancellationToken);
        Print(response.Result);
        return response.Result;
    },
    new CaseStatusWriter(caseApi));
var approver = new ConsoleApprover();

Console.WriteLine();
Console.WriteLine($"Case-handling agent ({settings.ModelDeployment}), Case API at {caseApiBaseUrl}.");
Console.WriteLine("Type 'Assess case C-1001' or paste a case description, and press Enter. Type 'exit' to quit.");

var input = args.Length > 0 ? string.Join(' ', args) : ReadMessage();
while (input is not null)
{
    var outcome = await CaseWorkflow.RunAsync(workflow, input, approver);
    Console.WriteLine(outcome switch
    {
        { Write: { } write } => $"[write] {write.Message}",
        { Assessment.CaseId: null } => "[write] No case id, so there is nothing to write.",
        { Assessment.ProposedStatus: null } => "[write] No proposal, so there is nothing to write.",
        _ => "[write] Nothing written.",
    });
    Console.WriteLine();

    input = ReadMessage();
}

return 0;

static string? ReadMessage()
{
    while (true)
    {
        Console.Write("> ");
        if (Console.ReadLine() is not { } first)
        {
            return null;
        }

        if (first.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var lines = new List<string> { first };

        // Pasted text arrives all at once, so lines that are already waiting belong to the same message.
        while (!Console.IsInputRedirected && Console.KeyAvailable && Console.ReadLine() is { } next)
        {
            lines.Add(next);
        }

        var message = string.Join(Environment.NewLine, lines).Trim();
        if (message.Length > 0)
        {
            return message;
        }
    }
}

static void Print(CaseAssessment assessment)
{
    Console.WriteLine();
    Console.WriteLine($"Case:                {assessment.CaseId ?? "none"}");
    Console.WriteLine($"Proposed status:     {assessment.ProposedStatus?.ToString() ?? "none"}");
    Console.WriteLine("Missing information:" + (assessment.MissingInformation.Count == 0 ? " none" : ""));
    foreach (var item in assessment.MissingInformation)
    {
        Console.WriteLine($"  - {item}");
    }

    Console.WriteLine($"Customer question:   {assessment.CustomerQuestion ?? "none"}");
    Console.WriteLine($"Policies:            {(assessment.PolicyIds.Count == 0 ? "none" : string.Join(", ", assessment.PolicyIds))}");
    Console.WriteLine($"Reasoning:           {assessment.Reasoning}");
    Console.WriteLine();
}
