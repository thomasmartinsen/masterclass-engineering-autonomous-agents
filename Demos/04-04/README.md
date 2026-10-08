# Demo 4: engineer a C# case-intake agent

Run `az login`, set `FOUNDRY_PROJECT_ENDPOINT` and `FOUNDRY_MODEL`, then run `dotnet run --project FirstCaseAgent.csproj`. The three outputs show a complete case, an incomplete case, and the follow-up turn with the same `AgentSession`. Ask the engineering agent to build this from Demo 3's skill and acceptance criteria; use this project as the completed fallback.

Inspect the model connection, runtime instructions, `RunAsync` calls, and session reuse. The model may vary the wording; the expected distinction is `ready` versus `needs_information`. The next demo adds a deterministic check for the response contract.
