# Demo 1 — Chat

Run `dotnet run --project Chat.csproj` after `az login` and setting `FOUNDRY_PROJECT_ENDPOINT` and `FOUNDRY_MODEL`.

Show general Q&A with a session that carries context between turns. The second question exposes the boundary: the agent has no access to internal policy. Ask the engineering agent to review the instructions and verify that the response does not invent a policy.

Next, run [Demo 2: interactive chat](../04-02/README.md) and keep chatting until Esc is pressed.
