# Bonus exercises

For when you finish a session early, or want to go further after the masterclass. No session depends on them, and there is no reference solution.

## Keep them apart from your session work

Do each bonus exercise on a branch of its own. Start it from the reference branch of the session it belongs to:

```powershell
git switch -c bonus-2-openspec session-2
```

Your work on `main` is not touched. When you are done, commit on the bonus branch, then go back with `git switch main`.

## Session 1 – Agentic engineering

### 1.1 The rule that was out of scope

[POL-GEN-001](../data/policies/general-claims-handling.md) says a claim must be reported no later than 12 months after the incident, "unless the customer can document why reporting was not possible". Write a brief that adds this rule to the case validation.

The brief has to decide what validation does with the exception, because validation cannot see the customer's documentation. Plan it in **Plan** mode first.

- Which questions did GitHub Copilot ask, and what did it assume without asking?
- Did the decision belong in the brief, or in the code?

### 1.2 Tests first

In a new chat, give GitHub Copilot your brief from 1.1 and ask for the tests only, one per acceptance criterion. Run them and check that they fail. Then, in another new chat, ask it to make the tests pass without changing them.

- Did the tests keep the implementation inside the brief?
- Which acceptance criteria were hard to turn into a test?

### 1.3 Same brief, another model

Run your brief from 1.1 in **Plan** mode with two different models from the model picker. Compare the plans side by side; you do not need to implement them.

- Which files, tests, and edge cases does each plan cover?
- Which plan would you accept, and why?

## Session 2 – Adapting knowledge, skills, and agents

### 2.1 Spec-driven development with OpenSpec

[OpenSpec](https://github.com/Fission-AI/OpenSpec) keeps a proposal, specs, a design, and a task list for each change in the repository. It needs Node.js 20.19 or newer.

```powershell
npm install -g @fission-ai/openspec@latest
openspec init
```

Choose GitHub Copilot when `openspec init` asks for your tools. Look at what it created in `openspec/` and in `.github/`. `openspec init` also prints the slash commands for your tool, for example `/opsx-propose`.

1. Run `/opsx-explore` and describe a change: listing cases reported between two dates, `GET /cases?reportedFrom=…&reportedTo=…`.
2. Run `/opsx-propose` and review `proposal.md`, `specs/`, `design.md`, and `tasks.md` before any code is written.
3. Run `/opsx-apply`, review the diff, and run the tests.
4. Run `/opsx-archive`.

- What does OpenSpec keep after the change that a brief and a plan in the chat do not?
- What overlaps with your `.github/copilot-instructions.md` and your skill? Which would you keep?

### 2.2 Give GitHub Copilot the policy MCP server

The policy MCP server is not only for your C# agent. Build it, then add it to VS Code in `.vscode/mcp.json`:

```powershell
dotnet build src/CaseHandling.PolicyMcp
```

```json
{
  "servers": {
    "policies": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["${workspaceFolder}/src/CaseHandling.PolicyMcp/bin/Debug/net10.0/CaseHandling.PolicyMcp.dll"]
    }
  }
}
```

Start the server from the file, then ask in chat: "Which policy applies to case C-1004, and what does it say about the deductible?"

- Did GitHub Copilot call the MCP tools, or read the Markdown files directly? Where do you see that?
- When is a tool better than a file in the repository?

### 2.3 Instructions for one part of the code, and a prompt file

- Move the test conventions out of `.github/copilot-instructions.md` into `.github/instructions/tests.instructions.md` with `applyTo: "tests/**"` in its front matter. Ask for a new test, and check in the chat's references that the file was used.
- Write `.github/prompts/brief.prompt.md` that interviews you one question at a time and then writes a brief in `briefs/` using the template from Session 1. Run it with `/brief`.

When does a rule belong in the instructions for every chat, and when only for some files?

## Session 3 – The platform for building and operating agents

### 3.1 Two models, one question

Deploy a second model in your Foundry project, or use another deployment that is already there. Run the smoke test with the same question against both, by changing `Foundry:ModelDeployment`.

- How do the answers differ in content, length, and time?
- Which would you use for the case-handling agent, and which for the carrier research?

### 3.2 Try to break your carrier agent

In the Foundry playground, give your `carrier-research` agent:

- a flight that does not exist
- an airline it cannot find anything about
- a message that asks it to ignore its instructions
- a question that has nothing to do with travel

Does it keep the agreed JSON format every time? Tighten its instructions and try again. Keep the list of messages: it is the start of an evaluation set.

### 3.3 Policies as knowledge in Foundry

Create another Foundry agent with the **File search** tool, and upload the files from `data/policies/`. Ask it: "What is the deductible for a fire claim?" and "How soon must a theft be reported to the police?"

- How do its citations compare with the policy MCP server you use in Session 5?
- What happens when a policy changes? Who updates it, and where?

## Session 4 – Agent development fundamentals and first agent

### 4.1 Stream the answer

Write a brief to show the agent's answer in the console while it is being generated, and still print the structured result at the end.

- What happens to the structured output while the text streams?
- Is streaming worth it for this agent?

### 4.2 Start a new conversation

Add a `/new` command that starts a new session without restarting the program. Ask about a case, ask a follow-up such as "What about the deductible?", then type `/new` and ask the same follow-up.

- What does the agent remember within a session, and where is it kept?
- When should a case handler start a new session?

### 4.3 Test the agent without Foundry

Write a test that runs your agent against a fake chat client that returns a fixed JSON answer, and checks the parsed result.

- What can you test without a model, and what can you only test with one?

## Session 5 – Advanced agent design and orchestration

### 5.1 When the Case API is down

Send `### Simulate the Case API being unavailable` from `CaseHandling.CaseApi.http`, then run the agent with `Assess case C-1001`.

- What does the agent propose? Could a case handler approve something that is wrong?

Write a brief so that the agent proposes nothing when it cannot read the case, and implement it. Reset with `### Reset data and faults` when you are done. The `session-6` branch has one solution in `briefs/session-06-case-api-unavailable.md`: compare it with yours afterwards.

### 5.2 Approval on a single tool call

Give the agent a tool that adds a note to the case (`POST /cases/{id}/notes`), and require a human to approve each call to that tool, instead of using the workflow.

- What does the human see in each case, and when?
- When is approval on a tool call enough, and when do you need a workflow?

### 5.3 The Case API as an MCP server

Move the `get_case` tool into an MCP server of its own, and connect your agent to it the way it connects to the policy server.

- What changes in testing, in deployment, and in who owns the tool?
- Which tools would you share between agents as MCP servers, and which would you keep as function tools?

## Session 6 – Putting the pieces together for continued development

### 6.1 From a tool to an agent workflow

Today the case-handling agent decides itself when to call your Foundry agent, because it is a tool. Rework it so that the carrier research is a step in the workflow instead:

```text
Investigate → (travel delay?) Carrier research → Assess → Approve → Write
```

Write the brief, plan it, and implement it. Keep the tests for the carrier answer.

- Who decides now that the carrier research runs: the model, or the workflow?
- What changes in the traces, in the tests, and in the number of model calls?
- When would you use agents as tools, and when agents as steps in a workflow?

### 6.2 Evaluate before you ship

Take your answer for **Evaluating** from Session 6. Put five or more cases in `eval/cases.json`, each with the expected proposed status and policy ids. Write a console program or test that runs the agent on each case and reports what matched.

- Run it twice. Do you get the same results?
- Which differences are failures, and which are acceptable?

### 6.3 Ship your first backlog item

Take the first item in `backlog.md` and implement it with everything from the masterclass: a brief, a plan, the implementation, the reviewer agent, and a run of the agent. If you did 6.2, run the evaluation before and after.
