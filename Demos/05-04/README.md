# Demo 4 — Structured output

Run `dotnet run --project StructuredOutput.csproj` with the two Foundry variables set.

The agent keeps Demo 1's application-owned `GetCase` tool and returns a typed `CaseTriage` object via `RunAsync<CaseTriage>`. Inspect the tool call, schema, printed JSON, and application validation. Ask the engineering agent to add one field, then review whether the model supplies it reliably.
