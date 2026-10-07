# Session 2 – Adapting knowledge, skills, and agents

**Time available:** ~30 min

**Goal:** Turn what you learned in Session 1 into customizations, so GitHub Copilot knows this repository without being told every time. Then show that its behavior changes.

## The three layers

| Layer | File | Loaded | Use it for |
| --- | --- | --- | --- |
| Repository knowledge and instructions | `.github/copilot-instructions.md` | Always, in every chat | Facts and rules that apply to every change: layout, commands, conventions, rules |
| Skill | `.github/skills/<name>/SKILL.md` | On demand, when the task matches its description or you type `/<name>` | A repeatable procedure, such as how to implement a change |
| Custom agent | `.github/agents/<name>.agent.md` | When you select it in the agent picker | A role with its own instructions and a limited set of tools, such as a reviewer |

Rule of thumb: if it is true for *every* task, it goes in the instructions. If it is *how to do* a kind of task, it is a skill. If it needs a *different role or different tools*, it is an agent.

## 0. Starting point

Continue on `main` with your Session 1 work. If you did not finish Session 1, commit or stash your work and continue from the reference solution:

```powershell
git switch session-1
```

## 1. Sort your Session 1 notes

Take your notes from Session 1: how the agent works, where its context came from, what the brief and the plan changed, and your definition of done. Next to each note, write which layer it belongs to: instructions, skill, or agent. For example, "the business rules are in `data/policies/`" is a fact for every task, so it belongs in the instructions. "Write a brief, plan, then implement" is a way of working, so it may belong in a skill.

## 2. Create the structure

Create these files in the repository root. Leave them empty for now:

```text
.github/
  copilot-instructions.md
  skills/
    csharp-implementation/
      SKILL.md
  agents/
    reviewer.agent.md
```

The locations matter. VS Code only finds customizations in these folders, and a skill's folder name must match the `name` in its `SKILL.md`.

## 3. Write the repository guidance

Write [.github/copilot-instructions.md](../.github/copilot-instructions.md) as plain Markdown. It needs no header. Use your notes, and cover at least:

- **What and where.** One sentence about the repository, what each project in `src/` contains, and that task briefs live in `briefs/`.
- **Build and test.** The exact commands.
- **C# conventions.** Look at the existing code and write down what you see.
- **Error responses.** Look at how the Case API endpoints report errors. Are they consistent? Decide on one way.
- **Tests.** Naming, and when a test belongs in a domain unit test or in an API test.
- **Approval rule.** Approving or rejecting a case is a business decision. Code and agents may *propose* a decision but must never set `Approved` or `Rejected` without explicit human approval.
- **Packages.** Packages and versions are managed in `Directory.Packages.props`.
- **Configuration.** Settings go in each project's `appsettings.json`. There are no keys or secrets: Azure access uses your `az login` identity.
- **Definition of done.** Use the one you drafted in Session 1.

Keep it short. Everything in this file goes into every chat, so every line costs context.

## 4. Write the C# implementation skill

A skill starts with a header that tells the agent *when* to use it:

```markdown
---
name: csharp-implementation
description: <What the skill does and when to use it. The agent reads only this until it decides to load the skill.>
---
```

Below the header, write the procedure in [.github/skills/csharp-implementation/SKILL.md](../.github/skills/csharp-implementation/SKILL.md). It should make the agent:

1. **Ask** for acceptance criteria if the task has none. Do not guess.
2. **Inspect** the existing code and tests before changing anything, and follow their style.
3. **Implement** the smallest change that meets the criteria. Do not touch `data/` or existing tests unless the task says so.
4. **Verify** by running the build and the tests.
5. **Report** a summary a reviewer can act on: changed files, one test per criterion, the command output, and open issues.

Don't repeat what is already in `copilot-instructions.md`.

## 5. Write the reviewer agent and use it

A custom agent also starts with a header. It decides which tools the agent may use:

```markdown
---
description: <Shown in the chat input when the agent is selected.>
tools: ['read', 'search']
---
```

Below the header, write the instructions in [.github/agents/reviewer.agent.md](../.github/agents/reviewer.agent.md): what the reviewer inspects (correctness against the acceptance criteria, missing tests, unsafe actions, consistency with the repository guidance) and how it reports findings. It never edits files.

Think about the `tools` list. With only `read` and `search`, the reviewer cannot see a committed diff. Adding `execute` lets it run `git diff`, but it could then also change files from the terminal. Which do you choose, and how do you limit it?

Check that VS Code found all three files. In the Chat view, select **Configure Chat** (the gear icon) and look under **Instructions**, **Skills**, and **Agents**. If something is missing, right-click in the Chat view and select **Diagnostics**.

Then select **reviewer** in the agent picker, open a new chat, and ask:

> Review the Session 1 change against its brief, #file:briefs/session-01-case-validation.md: the validation in `CaseValidator` and its tests in `CaseValidatorTests` and `CaseApiTests`.

Did it find problems you missed? Did it report anything that is not really a problem?

## 6. Try your customizations on a new task

Commit your customizations first:

```powershell
git add .github; git commit -m "Copilot customizations"
```

Then open a **new chat** in Agent mode and give the agent this request. Give it exactly this, with no brief and no acceptance criteria:

> The notes endpoint in the Case API returns a plain string when the author or the text is missing. Fix it.

Compare it with your Session 1 run:

- Was the skill used? Check the references and tool calls in the response.
- Did the agent ask for acceptance criteria before it changed any code?
- Does the fix follow the error-response rule from your instructions, and do the tests follow your conventions, without you saying so?
- Did it run the build and the tests, and report the way your definition of done says?

Review the change and commit it if you accept it.

## Output of this session

- Repository guidance, a C# implementation skill, and a reviewer agent that you wrote, committed to your branch
- A fix to the notes endpoint, made with the customizations
- One observed difference in the agent's behavior compared with Session 1
