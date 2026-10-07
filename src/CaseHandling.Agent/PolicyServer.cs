using ModelContextProtocol.Client;

namespace CaseHandling.Agent;

/// <summary>Starts the policy MCP server as a child process over stdio.</summary>
public static class PolicyServer
{
    public static Task<McpClient> StartAsync(CancellationToken cancellationToken = default)
    {
        // The server is a project reference, so its build output sits next to the agent.
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "policies",
            Command = "dotnet",
            Arguments = [Path.Combine(AppContext.BaseDirectory, "CaseHandling.PolicyMcp.dll")],
            WorkingDirectory = AppContext.BaseDirectory,
        });

        return McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
    }
}
