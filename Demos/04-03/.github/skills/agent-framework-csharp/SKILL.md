---
name: agent-framework-csharp
description: Use when adding or changing a C# Microsoft Agent Framework agent in this workshop repository.
---

# Implement a C# Agent Framework agent

1. Read the requested behavior and the current C# project before editing. Identify the deployed Foundry model and project endpoint from `FOUNDRY_MODEL` and `FOUNDRY_PROJECT_ENDPOINT`; never hard-code them or print credentials.
2. Use the versions pinned in the repository's `Directory.Packages.props`. Check current Microsoft Agent Framework documentation before using an unfamiliar API.
3. Keep the agent's runtime instructions in C# next to its creation. Distinguish those instructions from this engineering skill.
4. Implement the smallest code change that meets the acceptance criteria. Use `AIProjectClient.AsAIAgent` for a code-owned agent and reuse one `AgentSession` across turns of the same case.
5. Run `dotnet build` for the changed project and the demo's local checks. If Foundry access is available, run the complete and incomplete case examples.
6. Review the diff. Report files changed, commands run, observed results, and any behavior that remains unverified.
