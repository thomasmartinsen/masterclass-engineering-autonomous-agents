# Demo 1: invoke an agent running in Foundry

This demo uses Microsoft Agent Framework in C# to invoke a **versioned Prompt Agent already created in Microsoft Foundry**. The agent's model and instructions live in Foundry; the C# application selects its name and version, sends two messages, and reuses one Agent Framework session. It does not define a new local agent.

## Instructor setup in Foundry

1. In the prepared Foundry project, create a Prompt Agent named `WorkshopCaseAgent` using the class model deployment. Give it instructions such as: "You help triage customer device incidents. Ask for missing facts, distinguish observations from assumptions, recommend a next diagnostic step, and never claim that a case has been updated."
2. Publish or save a version, then test the agent in Foundry. Record its exact name and version from the project. If you attach hosted tools or knowledge, configure them on this Foundry agent definition and test access before class.
3. Grant participants permission to invoke the agent and share the project endpoint, agent name, and version. Have a successful portal run ready to compare with the C# run.

## Run from C#

After `az login`, set the values for the prepared agent:

```sh
export FOUNDRY_PROJECT_ENDPOINT="https://<account>.services.ai.azure.com/api/projects/<project>"
export FOUNDRY_AGENT_NAME="WorkshopCaseAgent"
export FOUNDRY_AGENT_VERSION="<published-version>"
dotnet run -c Debug --project EndToEnd.csproj
```

Watch the two calls to the **same Foundry agent version**. The second turn builds on the first through a reused `AgentSession`. In Debug mode, token usage and an estimated cost (if rates are configured) print after each response. Show the corresponding agent definition and runs in Foundry, then discuss version pinning, evaluation, traces, credentials, and how the team will keep developing the agent.

The other demos remain the examples for locally defined C# tools, MCP, structured output, and RAG. This final demo shows the different integration boundary: the agent definition is managed in Foundry and Agent Framework invokes it from application code.

Reference: [Microsoft Foundry Agent Service integration for Agent Framework](https://learn.microsoft.com/en-us/agent-framework/integrations/by-component/agent-services/foundry).
