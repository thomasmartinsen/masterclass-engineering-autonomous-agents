# Session 5 – Advanced agent design and orchestration

**Time available:** ~45 min

**Goal:** Let your agent fetch case data and policies itself, and put the one consequential action, changing a case, behind human approval.

## Three ways to extend an agent

| | Function tool | MCP server | Workflow |
| --- | --- | --- | --- |
| What it is | A C# method the agent can call | A separate process that offers tools over a standard protocol | Code that decides the order of steps |
| Who decides when it runs | The model | The model | Your code |
| Use it for | Your own logic and APIs, such as the Case API | Shared capabilities that several agents or tools can use, such as policies | Steps that must always happen in a fixed order, such as approval before a write |

## 0. Starting point

Continue on the branch you used in Session 4, normally `main`. If you did not finish Session 4, commit or stash your work and continue from the reference solution:

```powershell
git switch session-4
```

Start the Case API in a separate terminal and leave it running:

```powershell
dotnet run --project src/CaseHandling.CaseApi
```

## 1. Map the case journey

A case goes through these steps. For each, decide: does it need the **model's judgment**, or must your **code control** it?

| Step | Model judgment or code control? | Why |
| --- | --- | --- |
| Read the case | | |
| Find the relevant policy | | |
| Propose the next step | | |
| Ask a human to approve | | |
| Write the new status to the case | | |

## 2. Add a function tool for the Case API

The Case API already returns a case with `GET /cases/{id}`. Now your case-handling agent should be able to read a case itself.

Write a brief in `briefs/session-05-case-tool.md` and ask GitHub Copilot to add a **function tool** to your case-handling agent in `src/CaseHandling.Agent`: a C# method that calls this endpoint, and that the model can call when it needs a case. You don't change the Case API. Things to put in the brief:

- The tool only **reads**. It calls `GET /cases/{id}`.
- Where the Case API's address is configured. It must not be hard-coded.
- What the tool returns when the case does not exist (`404`) or the API is down (`503`). The agent must get a clear message it can act on, not an exception.
- A test for the tool, including the error cases.

Remember to tell GitHub Copilot to use your Agent Framework skill. Review the diff before you run anything:

- Is the tool's description clear enough for the model to know when to use it?
- Is the case id validated before it is sent to the API?

Make sure the Case API is still running (step 0). Then run your case-handling agent and type `Assess case C-1002`. It should now fetch the case itself.

## 3. Connect the policy MCP server

The policies are already served by an MCP server in `src/CaseHandling.PolicyMcp`, with the tools `list_policies`, `search_policies`, and `get_policy`. Now your case-handling agent should be able to look up the policy for a case itself.

Write a brief in `briefs/session-05-policy-tools.md` and ask GitHub Copilot to connect your case-handling agent in `src/CaseHandling.Agent` to this server. You don't change the MCP server, and you don't start it yourself: the agent starts it in the background and stops it again when the agent stops. Things to put in the brief:

- When the agent starts, it starts the MCP server, asks it which tools it has, and prints their names and descriptions.
- The agent gives those tools to the model, next to `get_case`.
- The structured result gets one more field: the ids of the policies the proposal is based on, for example `POL-THEFT-003`.

Remember to tell GitHub Copilot to use your Agent Framework skill. Review the diff before you run anything:

- Can you find the code that starts the server, and the line that adds its tools?
- Is the new field part of the printed result?

Make sure the Case API is still running (step 0). Then run your case-handling agent. At startup it prints the three policy tools; read their descriptions, because they are all the model knows about when to use each tool. Then type `Assess case C-1002`. Does the agent now ask for a police report and a frame number, as the theft policy requires? Does the result list `POL-THEFT-003`?

## 4. Put the write behind approval

Your agent can already investigate a case and propose a next step (steps 2 and 3). So far, nothing happens with the proposal. Now it can be carried out, but only after a human says yes.

That order must not depend on the model. The agent decides *how* to investigate, but your code decides *what happens next*: first the proposal, then a human's approval, then the write. An Agent Framework **workflow** is code that does exactly that, with the agent as one of its steps.

Write a brief in `briefs/session-05-approval-workflow.md` and ask GitHub Copilot to build the workflow in your case-handling agent. Things to put in the brief:

- **Investigate and propose:** the first step runs your agent as it works today and gets its structured proposal.
- **Approve:** the workflow pauses and asks the case handler in the console to approve or reject the proposal.
- **Write:** only if approved, the workflow calls `PUT /cases/{id}/status` with the proposed status and a reason.
- The agent itself gets **no tool that can write**. Only the workflow can write, and only after approval. The agent must never propose `Approved` or `Rejected`.
- A proposal without a case id, for example for a pasted description, is not sent for approval.
- Tests for the workflow that don't call the model: approve writes once, reject writes nothing.

Remember to tell GitHub Copilot to use your Agent Framework skill. Review the diff before you run anything:

- Can you find the steps of the workflow and the order they run in?
- Is there any way to reach the write without passing the approval?

Then run your case-handling agent and try three cases:

1. `Assess case C-1001` and approve. The case gets the proposed status, and one change is recorded.
2. `Assess case C-1002` and reject. Nothing changes.
3. Paste a description without a case id, such as `My bike was stolen.` The agent proposes a next step, but you are not asked to approve anything, and nothing changes.

You can check the cases, see which changes were made, and reset the case data to how it started, all through the Case API with the requests in [CaseHandling.CaseApi.http](../src/CaseHandling.CaseApi/CaseHandling.CaseApi.http).

## Output of this session

- A function tool that reads a case, with tests
- The policy MCP server connected, and policy ids in the proposal
- A workflow where no case is changed without human approval, tested for both approve and reject
