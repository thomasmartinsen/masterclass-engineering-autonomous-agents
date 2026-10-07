# Session 4 – Agent development fundamentals and first agent

**Time available:** ~45 min

**Goal:** Write a skill that teaches GitHub Copilot how to build Agent Framework agents in this repository. Then use it to build and run your first case-handling agent.

There are two agents in play again. Keep them apart:

- The **skill** you write guides *GitHub Copilot*.
- The **case-handling agent** is the C# program GitHub Copilot writes for you.

## The building blocks

| Concept | What it is | In this repository |
| --- | --- | --- |
| Model client | The connection to a model deployment | `AIProjectClient` with your Foundry project endpoint |
| Agent | A model plus instructions, a name, and later tools | `AIAgent` |
| Instructions | The agent's role and rules, sent with every run | A text file in the agent project |
| Run | One request to the agent and its response | `agent.RunAsync(...)` |
| Session | The conversation a run belongs to, so the agent remembers earlier messages | `AgentSession` |
| Structured output | The response as a typed C# object instead of free text | `agent.RunAsync<T>(...)` |

## 0. Starting point

Continue on the branch you used in Session 3, normally `main`. If you did not finish Session 3, commit or stash your work and continue from the reference solution:

```powershell
git switch session-3
```

Your case-handling agent needs the same two settings as the smoke test, `Foundry:ProjectEndpoint` and `Foundry:ModelDeployment`. It reads them from its own `src/CaseHandling.Agent/appsettings.json`; use the values from Session 3.

## 1. Write the Agent Framework skill

Agent Framework changes quickly, and GitHub Copilot often writes code against APIs that no longer exist. A skill gives it the facts for *this* repository, so it doesn't have to guess.

Create `.github/skills/agent-framework/SKILL.md` with a header:

```markdown
---
name: agent-framework
description: <What the skill does and when to use it.>
---
```

Below the header, cover:

- **When to use it:** creating or changing the case-handling agent.
- **Packages:** which packages are approved, and where versions are managed. Tell it to check the API of the pinned version and the [current documentation](https://learn.microsoft.com/agent-framework/get-started/your-first-agent) instead of guessing.
- **Model connection:** which configuration keys to use and which credential. Look at how the smoke test does it.
- **Conventions:** where the agent code lives, where the instructions are stored, and how runs and sessions are used.
- **Verify:** the build command, the run command, and the two cases to try.
- **Report:** what the summary must contain.

Don't repeat what is already in `copilot-instructions.md` or the C# implementation skill.

## 2. Write the task brief

Write a brief, as in Session 1, in `briefs/session-04-case-handling-agent.md`. Use the same template. The goal is:

> A case-handling agent that takes a case description, asks for missing information when needed, and returns a structured proposed next step.

The agent must work like this, because the next steps and Session 5 build on it:

- It is a console program in `src/CaseHandling.Agent`, started with `dotnet run --project src/CaseHandling.Agent`.
- It reads messages in a loop. A message is sent when you press Enter; text pasted with several lines, such as a case description, is sent as one message. `exit` quits.
- All messages in one run belong to the same conversation, so a follow-up is assessed together with everything said before.
- After each message it prints the agent's structured result in a readable form.

Think about:

- What should the result contain? A proposed status, what is missing, a question for the customer, the reasoning?
- Which statuses may the agent propose, and which must it never propose?
- Which case do you use to check a complete case, and which to check an incomplete one?
- What is out of scope? There are no tools yet.

## 3. Let GitHub Copilot build it

Open a new chat in **Plan** mode and give it your brief. Check that the plan uses your skill: it should mention the packages and the model connection you wrote down.

Then switch to **Agent** in the same chat and let it implement the plan. Don't edit the code yourself.

## 4. Run your agent

```powershell
dotnet run --project src/CaseHandling.Agent
```

Try:

1. **A complete case.** Paste the description of `C-1001` from [data/cases/cases.json](../data/cases/cases.json).
2. **An incomplete case.** Restart, and type `My bike was stolen.`
3. **A follow-up.** In the same run, add the date and the amount. Does the agent reassess the case with everything you have told it?

## 5. Compare with Session 3

Go back to your table from Session 3, step 5. Which rows are now **yes**?

Then look closely at the bike case. The agent probably accepted it as complete once it had a date and an amount. Is that what the theft policy in [data/policies/theft.md](../data/policies/theft.md) says? What would the agent need to know that? That is Session 5.

## Output of this session

- An Agent Framework skill in `.github/skills/agent-framework/`
- A task brief in `briefs/`
- A running case-handling agent that proposes a next step, with structured output and a session
