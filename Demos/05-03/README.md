# Demo 3 — Chained tool calls

After `az login` and setting `FOUNDRY_PROJECT_ENDPOINT` and `FOUNDRY_MODEL`, run:

```sh
dotnet run --project ToolChain.csproj
```

The first question asks for the recommended priority of Ben's case. The agent must call `FindCases` to discover the case ID, then `GetCase` to read its recommendation. The second request asks the agent to apply that recommendation, adding `SetCasePriority` as the third dependent call. Each method prints its name and arguments, and the program checks the observed call order and final state.

The change affects **in-memory workshop data only** and resets on the next run. Use the two prompts to show that tool results from one step provide arguments for the next. The two requests use separate agent sessions so the second chain starts without case details carried over from the first answer. In Debug mode, token usage is printed after each agent response.

Ask the engineering agent to add a second customer and case, then review whether the tool descriptions and validation still prevent a guessed ID or priority.
