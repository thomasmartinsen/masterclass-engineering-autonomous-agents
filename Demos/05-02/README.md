# Demo 2 — Tool calling through an external MCP server

This demo connects the C# agent to a **separate MCP server over HTTP**. The agent project contains no `SearchPolicy` implementation; it discovers that tool from the server at runtime.

For a self-contained workshop run, start the supplied policy server in one terminal:

```sh
dotnet run --project PolicyMcpServer/PolicyMcpServer.csproj -- --urls http://127.0.0.1:5055
```

After `az login` and setting `FOUNDRY_PROJECT_ENDPOINT` and `FOUNDRY_MODEL`, run the agent in a second terminal:

```sh
dotnet run --project McpCaseAgent/McpCaseAgent.csproj
```

The client defaults to `http://127.0.0.1:5055/mcp`. To use a separately hosted policy MCP server, set `POLICY_MCP_ENDPOINT` to its full Streamable HTTP MCP URL. If that endpoint uses bearer authentication, set `POLICY_MCP_BEARER_TOKEN` locally; do not commit it. The server must expose a `SearchPolicy` tool accepting a `query` string and returning policy evidence. Confirm network access before the workshop.

Inspect the discovered tool list in the agent terminal, the `SearchPolicy` call logged by the supplied server, and the policy ID in the response. Compare this with [Demo 1's application-owned tool](../05-01/README.md): the C# agent uses a tool in both runs, but this demo gets the tool from a separate server.
