using System.ComponentModel;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
var app = builder.Build();
app.MapMcp("/mcp");
app.Run();

[McpServerToolType]
public static class PolicyTools
{
    [McpServerTool, Description("Search the workshop's case-handling policy. Returns a policy ID and excerpt.")]
    public static string SearchPolicy([Description("The case topic, such as laptop replacement or account access.")] string query)
    {
        Console.WriteLine($"MCP SearchPolicy called with: {query}");
        if (query.Contains("laptop", StringComparison.OrdinalIgnoreCase)
            || query.Contains("replacement", StringComparison.OrdinalIgnoreCase))
            return "POL-04: For a failed laptop, confirm contact details and urgency before proposing a replacement. A human approves any case update.";

        return "POL-07: For portal access, confirm the affected account and error before proposing a reset or escalation. A human approves any case update.";
    }
}
