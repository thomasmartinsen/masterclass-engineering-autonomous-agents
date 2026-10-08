# Masterclass demos

Demo folders are named `SS-DD`: session number, then demo number. The numbered session sketches remain in [sessions](../sessions/README.md). Each demo has its own entry point and run instructions.
There is no Session 6 in the current agenda.

| Session | Demo folders |
| --- | --- |
| 1 | [01-01 Development loop](01-01/README.md) |
| 2 | [02-01 Custom engineering agent](02-01/README.md) |
| 3 | [03-01 Foundry connection](03-01/README.md) |
| 4 | [04-01 Chat](04-01/README.md), [04-02 Interactive chat](04-02/README.md), [04-03 Engineering skill](04-03/README.md), [04-04 Case-intake agent](04-04/README.md), [04-05 Review and tests](04-05/README.md) |
| 5 | [05-01 Project tools](05-01/README.md), [05-02 External MCP server](05-02/README.md), [05-03 Chained tool calls](05-03/README.md), [05-04 Structured output](05-04/README.md), [05-05 RAG](05-05/README.md) |
| 5, optional | [05-06 Approval workflow](05-06/README.md), [05-07 Failure cases](05-07/README.md) |
| 7 | [07-01 Invoke a Foundry agent](07-01/README.md) |

## Prerequisites

- .NET 10 SDK. All projects inherit settings from [Directory.Build.props](Directory.Build.props) and pinned NuGet versions from [Directory.Packages.props](Directory.Packages.props).
- For Sessions 3–5 and 7: `az login` and access to a prepared Microsoft Foundry project. Sessions 3–5 need a model deployment; Session 7 needs a versioned Foundry Prompt Agent.
- Set `FOUNDRY_PROJECT_ENDPOINT` to the project endpoint. For Sessions 3–5 set `FOUNDRY_MODEL` to the deployed model name; for Session 7 set `FOUNDRY_AGENT_NAME` and `FOUNDRY_AGENT_VERSION` to the prepared agent. Do not commit credentials.
- For Session 5's MCP demo, run the supplied policy server in a separate terminal or use an instructor-provided `POLICY_MCP_ENDPOINT`.

Sessions 3–5 use Microsoft Agent Framework's code-owned agent pattern (`AIProjectClient.AsAIAgent` with a model). Session 4 covers scripted and interactive chat; Session 5 adds project tools → external MCP tools → chained tool calls → structured output → RAG. Session 7 invokes an existing Foundry agent through Agent Framework. Shared source used by the demos is in [_shared](_shared/TokenUsageReporter.cs). See each demo's README for commands and expected observations.

**Verification status:** These C# projects were prepared against current Microsoft documentation and package listings. The authoring machine does not have the .NET SDK, so restore, compilation, and a live Foundry run still need to be checked on a machine with the prerequisites above.

## Debug token usage and estimated cost

All demos that call a model print input, output, and total token counts after **every** agent response when built in Debug mode (`dotnet run -c Debug --project <project.csproj>`). If the provider does not return usage, the console says it is unavailable. Release builds omit these debug lines.

[Demo 05-01](05-01/README.md) also uses function-invocation middleware to print each project tool call and its arguments in Debug builds only.

For an estimated USD cost, set both `FOUNDRY_INPUT_USD_PER_1M_TOKENS` and `FOUNDRY_OUTPUT_USD_PER_1M_TOKENS` to the rates for the deployed model. Without both rates, the console prints that the cost estimate is unavailable. This simple estimate uses reported input and output totals and does not account for special pricing such as cached or audio tokens.

## Sources

- [Microsoft Foundry model provider](https://learn.microsoft.com/en-us/agent-framework/integrations/by-component/model-providers/microsoft-foundry)
- [Microsoft Foundry Agent Service integration](https://learn.microsoft.com/en-us/agent-framework/integrations/by-component/agent-services/foundry)
- [Agent Framework function tools](https://learn.microsoft.com/en-us/agent-framework/agents/tools/function-tools)
- [Agent Framework structured outputs](https://learn.microsoft.com/en-us/agent-framework/agents/structured-outputs)
- [Agent Framework RAG](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-rag)
- [Agent Framework MCP tools](https://learn.microsoft.com/en-us/agent-framework/agents/tools/local-mcp-tools)
- [Agent Framework human-in-the-loop workflows](https://learn.microsoft.com/en-us/agent-framework/workflows/human-in-the-loop)
- [MCP C# SDK getting started](https://github.com/modelcontextprotocol/csharp-sdk/blob/main/docs/concepts/getting-started.md)
