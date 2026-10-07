using CaseHandling.PolicyMcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// stdout carries the MCP protocol, so all logs must go to stderr.
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

var policyDirectory = builder.Configuration["Policies:Path"]
    ?? Path.Combine(AppContext.BaseDirectory, "data", "policies");

builder.Services.AddSingleton(new PolicyRepository(policyDirectory));
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<PolicyTools>();

await builder.Build().RunAsync();
