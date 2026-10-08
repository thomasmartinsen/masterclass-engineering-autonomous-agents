# Case handling – masterclass starter

Starter solution for the masterclass *Design and development of autonomous AI agents*. You will use GitHub Copilot to change this repository, and build a case-handling application agent with Microsoft Agent Framework and Microsoft Foundry.

## Before the masterclass

```powershell
dotnet --version          # 10.0.x
git --version
az login                  # account provided for the masterclass

git clone <repository url>
cd <repository folder>
dotnet build CaseHandling.slnx
dotnet test --solution CaseHandling.slnx
code .
```

Build and tests must pass. Sessions 1 and 2 need no Azure access.

## Exercises

1. [Agentic engineering](exercises/session-01-agentic-engineering.md)
2. [Adapting knowledge, skills, and agents](exercises/session-02-engineering-agent-customization.md)
3. [The platform for building and operating agents](exercises/session-03-platform-foundations.md)
4. [Agent development fundamentals and first agent](exercises/session-04-agent-development-fundamentals.md)
5. [Advanced agent design and orchestration](exercises/session-05-advanced-agent-design.md)
6. [Putting the pieces together for continued development](exercises/session-06-continued-development.md)

Finished early? [Bonus exercises](exercises/session-bonus.md) for every session, done on a branch of their own.

## What is in the repository

| Path | Purpose |
| --- | --- |
| `src/CaseHandling.Domain` | Case model, validation, allowed status transitions |
| `src/CaseHandling.CaseApi` | Case API over test data (`http://localhost:5080`) |
| `src/CaseHandling.PolicyMcp` | MCP server exposing the claims policies |
| `src/CaseHandling.FoundrySmokeTest` | Direct model call to verify Foundry access |
| `src/CaseHandling.Agent` | The application agent, built from Session 4 |
| `src/CaseHandling.Tests` | Tests |
| `data/cases`, `data/policies` | Test data and policy documents |
| `briefs/` | Task briefs you write for GitHub Copilot |
| `AGENTS.md` | Masterclass rules for GitHub Copilot: no peeking at the `session-*` branches or the exercises |

## Run the pieces

```powershell
# Case API – try the requests in src/CaseHandling.CaseApi/CaseHandling.CaseApi.http
dotnet run --project src/CaseHandling.CaseApi

# Policy MCP server (stdio)
dotnet run --project src/CaseHandling.PolicyMcp

# Foundry smoke test – fill in src/CaseHandling.FoundrySmokeTest/appsettings.json first
dotnet run --project src/CaseHandling.FoundrySmokeTest
```

The Foundry settings go in the project's `appsettings.json`. There are no keys or secrets: Azure access uses your `az login` identity.

```json
{
  "Foundry": {
    "ProjectEndpoint": "https://<resource>.services.ai.azure.com/api/projects/<project>",
    "ModelDeployment": "<deployment>"
  }
}
```

Case API admin endpoints for exercises: `GET /admin/changes` shows every write, `POST /admin/reset` restores the test data, and `POST /admin/faults` with `{ "unavailable": true }` simulates an outage.

## Falling behind

Work on `main` through all sessions; each session builds on the previous one.

Each session also has a reference branch with the completed state, and each branch builds on the one before it:

| Branch | Contains |
| --- | --- |
| `main` | The starting point |
| `session-1` | Session 1 completed |
| `session-2` | Sessions 1–2 completed |
| ... | ... |
| `session-6` | All sessions completed |

If you did not finish a session, commit or stash your work and continue from its reference branch, for example `git switch session-1` before you start Session 2. Use `git diff session-1 session-2` to see what a session adds.
