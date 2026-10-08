# Demo 2 — Interactive chat

After `az login` and setting `FOUNDRY_PROJECT_ENDPOINT` and `FOUNDRY_MODEL`, run in a terminal:

```sh
dotnet run --project InteractiveChat.csproj
```

Type a message and press **Enter** to send it. Continue chatting in the same agent session. Press **Esc** at the `You:` prompt to exit immediately; an empty line simply starts a new prompt. The demo reads keys directly so Esc works without pressing Enter. It requires an interactive terminal rather than redirected input.

Try asking about a laptop case, then ask a follow-up that refers to the same case. Ask for an internal policy that has not been supplied and inspect whether the agent acknowledges the missing source. Debug builds print token usage and, when rates are configured, an estimated cost after every response.
