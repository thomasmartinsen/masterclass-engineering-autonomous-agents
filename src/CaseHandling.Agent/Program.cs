using Azure.AI.Projects;
using Azure.Identity;
using CaseHandling.Agent;
using Microsoft.Agents.AI;
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

var instructions = AgentInstructions.Load(AppContext.BaseDirectory);

AIAgent agent = new AIProjectClient(new Uri(settings.ProjectEndpoint), new AzureCliCredential())
    .AsAIAgent(model: settings.ModelDeployment, instructions: instructions, name: "CaseHandlingAgent");

// One session for the whole run, so follow-up messages are assessed together with the first one.
var session = await agent.CreateSessionAsync();

Console.WriteLine($"Case-handling agent ({settings.ModelDeployment}).");
Console.WriteLine("Type or paste a case description and press Enter. Add information in follow-up messages. Type 'exit' to quit.");

var input = args.Length > 0 ? string.Join(' ', args) : ReadMessage();
while (input is not null)
{
    var response = await agent.RunAsync<CaseAssessment>(input, session);
    Print(response.Result);

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
    Console.WriteLine($"Proposed status:     {assessment.ProposedStatus}");
    Console.WriteLine("Missing information:" + (assessment.MissingInformation.Count == 0 ? " none" : ""));
    foreach (var item in assessment.MissingInformation)
    {
        Console.WriteLine($"  - {item}");
    }

    Console.WriteLine($"Customer question:   {assessment.CustomerQuestion ?? "none"}");
    Console.WriteLine($"Reasoning:           {assessment.Reasoning}");
    Console.WriteLine();
}
