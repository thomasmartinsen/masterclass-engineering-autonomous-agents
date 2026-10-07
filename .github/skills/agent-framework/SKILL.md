---
name: agent-framework
description: Repository facts for building the case-handling agent with Microsoft Agent Framework (packages, Foundry connection, instructions, runs, sessions, structured output). Use when creating or changing the agent in src/CaseHandling.Agent.
---

# Agent Framework

## When to use it

Creating or changing the case-handling agent in `src/CaseHandling.Agent`. Not for the Case API, the MCP server, or the smoke test.

## Packages

- Approved packages are in the `Foundry and Agent Framework` and `Hosting and configuration` groups of `Directory.Packages.props`: `Microsoft.Agents.AI.Foundry`, `Azure.AI.Projects`, `Azure.Identity`, `Microsoft.Extensions.Hosting` (`Microsoft.Agents.AI.Workflows` only for workflows).
- Do not add other AI packages (e.g. `Azure.AI.OpenAI`, Semantic Kernel, preview `Microsoft.Agents.AI.*`) without asking.
- Agent Framework changes quickly and many online samples are outdated. Do not guess an API. Before writing code:
  - Check the API of the **pinned** version (the restored package in `~/.nuget/packages`, or let the compiler tell you).
  - Read the current docs: https://learn.microsoft.com/agent-framework/get-started/your-first-agent
  - If they disagree, the pinned version wins. Report the difference.

## Model connection

Do it the same way as `src/CaseHandling.FoundrySmokeTest/Program.cs`:

- Configuration via `Host.CreateApplicationBuilder` with `ContentRootPath = AppContext.BaseDirectory`, which reads `appsettings.json`.
- Keys: `Foundry:ProjectEndpoint` and `Foundry:ModelDeployment`. Fail with a clear message if either is missing.
- Client: `new AIProjectClient(new Uri(endpoint), new AzureCliCredential())`. No API keys, no `DefaultAzureCredential`.
- Copy `appsettings.json` to the output directory in the `.csproj` (see the smoke test project).

## Conventions

- All agent code lives in `src/CaseHandling.Agent`. Reuse `CaseHandling.Domain` types (e.g. `CaseStatus`) instead of redefining them.
- Instructions are stored in a text file in the agent project (e.g. `instructions.md`), copied to output and loaded at startup. No long prompts inline in C#.
- Create the agent once (`AIAgent`, with a name and the instructions), then:
  - one `AgentSession` per conversation, created with `agent.CreateSessionAsync()`, and pass it to every run so follow-ups keep context;
  - use `agent.RunAsync<T>(input, session)` for structured output, with `T` a `sealed record` in the agent project.
- The agent proposes a next step; it never sets `Approved` or `Rejected` (see the approval rule).
- Tools:
  - Function tools are methods on a class in the agent project, registered with `AIFunctionFactory.Create(instance.Method, name: "snake_case")`. Every tool and parameter has a `[Description]`.
  - Tools the model can call only **read**. They validate their arguments and return an `Error: ...` message instead of throwing.
  - The policy MCP server is started over stdio by `PolicyServer`; its tools are passed to the agent as they are.
  - Wrap every tool in `LoggingAIFunction`, so each call is printed with its arguments and result.
- Writes happen only in the `CaseWorkflow` write step, after a human approves through the request port. Never give the agent a write tool.

## Verify

From the repository root, requires `az login`:

```powershell
dotnet build CaseHandling.slnx
dotnet run --project src/CaseHandling.Agent
```

Try both, each in a fresh run, with the Case API running (`dotnet run --project src/CaseHandling.CaseApi`):

1. **Complete case:** `Assess case C-1001`.
2. **Incomplete case:** `Assess case C-1002`. It should cite `POL-THEFT-003` and ask for a police report and a frame number.

For each, reject once and approve once, and check `GET /admin/changes`: no change after a rejection, exactly one after an approval. Reset with `POST /admin/reset`.

If you cannot run the agent (no login or endpoint), say so; do not claim it works.

## Report

In addition to the C# implementation report:

- **Packages and API** – packages referenced, and which API calls you verified against the pinned version or the docs.
- **Run results** – the structured output for both cases, including the follow-up.
- **Lessons** – anything that did not match the docs or your first guess, so it can be added to this skill.

## Lessons learned

- `AsAIAgent` is an extension method on `AIProjectClient` from `Microsoft.Agents.AI.Foundry` (namespace `Azure.AI.Projects`). Its parameters are `model`, `instructions`, `name`, `description`, `tools`, and `clientFactory`. There is no separate chat client to create.
- The program must keep reading messages in a loop with the same session. A single-run program loses the context for follow-ups.
- Enums in a structured result need `[JsonConverter(typeof(JsonStringEnumConverter<T>))]`, so the schema and the model use names instead of numbers. Use a separate enum for the statuses the agent may propose, so `Approved` and `Rejected` are not in the schema.
- MCP client (`ModelContextProtocol` 2.x): `McpClient.CreateAsync(new StdioClientTransport(...))`, then `ListToolsAsync()`. The returned `McpClientTool`s are `AIFunction`s and can be passed straight to `tools:`.
- Workflows (`Microsoft.Agents.AI.Workflows`): an executor may only send or yield the types it declares. A lambda bound with `BindAsExecutor` cannot call `YieldOutputAsync`; use `new FunctionExecutor<T>(id, handler, outputTypes: [typeof(TOut)])`, or return a value so it is sent along the edges. Use conditional `AddEdge<T>(from, to, condition)` for branches.
- Human approval is a `RequestPort.Create<TRequest, TResponse>`. The host watches `run.WatchStreamAsync()` for `RequestInfoEvent` and answers with `run.SendResponseAsync(request.CreateResponse(...))`.
