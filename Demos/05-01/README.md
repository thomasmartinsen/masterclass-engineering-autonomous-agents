# Demo 1 — Tools defined in the project

With `az login` and the Foundry variables set, run:

```sh
dotnet run --project CaseAgent/CaseAgent.csproj
```

The agent receives a typed, read-only C# `GetCase` function tool defined in this project. In **Debug** builds, function-invocation middleware prints each tool name and its arguments before forwarding the call. The existing debug token-usage output follows the agent response. In **Release** builds, the tool-call middleware and its console output are omitted.

Run `dotnet run -c Debug --project CaseAgent/CaseAgent.csproj` to inspect the tool call. Ask the engineering agent to implement another case ID and the unknown-ID error path, then review its changes. This demo uses only application-owned tools; [Demo 2](../05-02/README.md) introduces tools exposed by a separate MCP server.

The middleware follows Agent Framework's [function-calling middleware pattern](https://learn.microsoft.com/en-us/agent-framework/agents/middleware/).
