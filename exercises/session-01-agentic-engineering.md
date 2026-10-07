# Session 1 – Agentic engineering

**Time available:** ~40 min

**Goal:** Get a feel for how GitHub Copilot works as an engineering agent. Give it the same task twice, first as a one-line request and then with a brief and a plan, and compare what it does. Verify the result before you accept it.

You are not building the case-handling agent yet. Today you meet two kinds of agents:

| | Engineering agent | Application agent |
| --- | --- | --- |
| What it is | GitHub Copilot, Claude Code, or Codex | The C# agent you build from Session 4 |
| What it works on | This repository: code, tests, docs | Customer cases |
| Who it works for | You, the developer | Case handlers |

In this session you work with **GitHub Copilot**, the engineering agent, only. From now on, the exercises call it GitHub Copilot.

## 0. Get ready

You cloned this repository before the masterclass (see the [README](../README.md)). Open it in VS Code, and from the repository root make sure you are on `main` with the latest version, and that the build and tests pass:

```powershell
git switch main
git pull
dotnet build CaseHandling.slnx
dotnet test --solution CaseHandling.slnx
```

Work on `main` all day. Each session builds on the previous one, so commit your work at the end of every session.

If you don't finish a session, you can continue from its reference solution. Each session has a branch with the completed exercise: `session-1`, `session-2`, and so on. Commit or stash your work and switch to it, for example `git switch session-1` before you start Session 2.

Open the Chat view in VS Code and pick a model. You will use two modes from the agent picker: **Plan**, which researches and plans but cannot change files, and **Agent**, which changes files and runs commands.

## 1. Understand the problem

Cases are claims from customers. Before a case handler can work on a case, it must pass validation in [CaseValidator.cs](../src/CaseHandling.Domain/CaseValidator.cs). The case handlers report two cases in [data/cases/cases.json](../data/cases/cases.json) that pass validation but should not:

| Case | What the case handlers say |
| --- | --- |
| `C-1002` | *"My bike was stolen."* We don't know when it happened (`incidentDate` is `null`) or how much the customer claims (`claimedAmount` is `null`). |
| `C-1007` | The incident date is two days *after* the date the claim was reported. That cannot be right. |

See for yourself. From the repository root, start the Case API:

```powershell
dotnet run --project src/CaseHandling.CaseApi
```

When it shows `Now listening on: http://localhost:5080`, open [CaseHandling.CaseApi.http](../src/CaseHandling.CaseApi/CaseHandling.CaseApi.http) and select **Send Request** above `### Validate all cases`. This needs the REST Client extension, which VS Code suggests when you open the repository. Without it, open <http://localhost:5080/cases/validation> in a browser.

You get every case with its validation issues. All `issues` lists are empty. Stop the API with `Ctrl+C` before you continue: while it runs, it locks files the agent needs to build.

## 2. Start with a one-line request

Start the way many people do. Open a **new chat**, select **Agent**, and send exactly this:

> Fix the validation on cases.

Let it finish. Then start the Case API again, send `### Validate all cases`, and stop the API again. Which of the two cases have issues now? Do complete cases, such as `C-1001`, still have none?

Now look back through the chat and notice what the agent did. Expand the tool calls and the list of references in its response to see the details:

- **Search:** what did it search for, and which files did it find?
- **Tools:** which tools did it use: search, read file, edit, terminal? In what order?
- **Context:** which files did it read before it changed anything: code, tests, data, documents? Which files did you have open in the editor? Open files are part of the agent's context, too.
- **Decisions:** how did it decide what "fix the validation" means?
- **Commands:** which commands did it run, and what did it do when one failed?
- **Edits:** which files did it change, and did it add tests?
- **Verification:** how did it check its own work before it said it was done?

Then undo the change, so the next run starts from the same code: in **Source Control**, select **Discard All Changes**. Your chat with the agent stays, so you can look back at what it did.

## 3. Write a task brief

Now write down exactly what you want, so the agent doesn't have to work it out.

Create `briefs/session-01-case-validation.md` and copy this template into it. All task briefs go in the [briefs](../briefs) folder, so you and the agent can find them again:

```markdown
## Goal
<One sentence: what should be true when the task is done?>

## Context
<Where does the relevant code live? What does the agent need to know?>

## Expected behavior
<Inputs and the results you expect from them.>

## Failure behavior
<What should happen when the input is incomplete or wrong?>

## Acceptance criteria
- [ ] <Observable and checkable. One test per criterion.>
- [ ] ...

## Out of scope
<What the agent must not change.>
```

Questions to consider:

- Which rules does validation need for the two cases in step 1? Read the **Required information** section of the [general claims-handling policy](../data/policies/general-claims-handling.md).
- Which cases must still pass? A complete case, such as `C-1001`, must not get issues.
- What about the edge case: an incident date on the same day it was reported?
- Should a problem be reported as a validation issue or throw an exception? Look at how the existing checks work.
- How will you *see* that each criterion is met?

## 4. Ask for a plan first

Open a **new chat**, select **Plan** in the agent picker, and give it your brief:

> Here is my task brief: #file:briefs/session-01-case-validation.md. Plan the change.

Plan mode can read and search the repository, but it cannot edit files, so nothing changes yet. Read the plan. Is it the change you had in mind? Does it touch only the files you expected? Correct it in chat before you continue.

## 5. Let the agent implement

Switch to **Agent** in the same chat, so it keeps the plan, and ask it to carry out the plan and to run the build and the tests. **Don't edit the code yourself.** If something is wrong, tell the agent what to change.

Watch what it does, as in step 2. What is different this time?

## 6. Review before you accept

Open the changed files in Source Control and review the diff as if a colleague had written it.

- [ ] Every acceptance criterion in your brief is met.
- [ ] `### Validate all cases` shows issues for `C-1002` and `C-1007`. Complete cases, such as `C-1001`, still have none. Start the Case API to check, and stop it again afterwards.
- [ ] Only the expected files were changed. In particular, `data/` and existing tests were not edited to make the problem disappear.
- [ ] There is at least one test for each acceptance criterion, and each test fails without the change.
- [ ] The tests are deterministic. They don't depend on today's date or on the order they run in.
- [ ] The new checks follow the existing style in `CaseValidator`.
- [ ] `dotnet test --solution CaseHandling.slnx` passes when **you** run it.
- [ ] You can explain the change in your own words.

Accept the change, or tell the agent what to revise and review it again.

## 7. Capture what you learned

Write down four short notes. You will use them in Session 2.

1. **How the agent works:** What did it do, step by step: read, search, edit, run, check? What surprised you?
2. **Where its context came from:** What did it use to decide what to do: the code, the data, the documents, your open files, your brief?
3. **What the brief and the plan changed:** Compare the two runs. What was different in what the agent did, in the result, and in how sure you are that it is right?
4. **Definition of done:** Your checklist for accepting a change from the agent, for any task in this repository, not just this one. Start from what you checked in step 6 and keep what you would check every time. For example: the build has no new warnings, all tests pass, every acceptance criterion has a test, and `data/` was not changed. In Session 2 you give this list to the agent, so it checks it itself.

## Output of this session

- A task brief with acceptance criteria in `briefs/`
- A reviewed and accepted change with passing tests
- Notes on how the agent works, where its context came from, what the brief changed, and a draft definition of done
