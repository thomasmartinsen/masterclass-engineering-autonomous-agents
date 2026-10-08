using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenAI.Responses;

// A direct model call: no agent, no instructions, no tools, no session.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

var endpoint = builder.Configuration["Foundry:ProjectEndpoint"];
var model = builder.Configuration["Foundry:ModelDeployment"];

if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(model))
{
    Console.Error.WriteLine("""
        Set Foundry:ProjectEndpoint and Foundry:ModelDeployment in
        src/CaseHandling.FoundrySmokeTest/appsettings.json:

        { "Foundry": { "ProjectEndpoint": "https://<resource>.services.ai.azure.com/api/projects/<project>", "ModelDeployment": "<deployment>" } }
        """);
    return 1;
}

var input = args.Length > 0
    ? string.Join(' ', args)
    : "A customer reports that a water pipe under the kitchen sink burst and damaged the floor. Summarise the case in one sentence.";

var client = new AIProjectClient(new Uri(endpoint), new AzureCliCredential());
var responses = client.ProjectOpenAIClient.GetProjectResponsesClientForModel(model);

Console.WriteLine($"Endpoint:   {endpoint}");
Console.WriteLine($"Deployment: {model}");
Console.WriteLine($"Input:      {input}");
Console.WriteLine();

ResponseResult response = await responses.CreateResponseAsync(input);
Console.WriteLine(response.GetOutputText());
return 0;
