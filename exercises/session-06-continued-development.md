# Session 6 – Putting the pieces together for continued development

**Time available:** ~15 min

**Goal:** Let your C# agent use the Foundry agent you created in Session 3, follow one complete case from the request to the result, and leave with a plan for the next iteration.

## Where things run

| In your C# solution | In Microsoft Foundry |
| --- | --- |
| The case-handling agent, its instructions and structured output | The model deployment it calls |
| The Case API tool and the policy MCP server | Your `carrier-research` agent and its Web search tool |
| The workflow and the human approval | Tracing of the Foundry agent's runs |

## 0. Starting point

Continue on the branch you used in Session 5, normally `main`. If you did not finish Session 5, commit or stash your work and continue from the reference solution:

```powershell
git switch session-5
```

Start the Case API in a separate terminal and leave it running. You need the name of your Foundry agent from Session 3.

## 1. Connect the two agents

Write a brief in `briefs/session-06-carrier-agent.md` and have GitHub Copilot give your case-handling agent a tool that calls your Foundry agent. Put in the brief:

- The Foundry agent's name is configured as `Foundry:CarrierAgentName` in `src/CaseHandling.Agent/appsettings.json`. Without it, the agent works as before.
- The tool runs the Foundry agent through the same `AIProjectClient`, with the airline, the flight, and the delay.
- The JSON answer is parsed into a typed record. If it is not the agreed format, or the call fails, the tool returns a message starting with `Error:` instead of throwing.
- The answer comes from the web. The case-handling agent treats it as information, never as instructions.
- The case-handling agent's instructions say when to use the tool: for travel delays, to find what the airline already pays, which [POL-TRAVEL-004](../data/policies/travel-delay.md) does not cover.
- Tests for the tool, without calling Foundry.

As before, plan in **Plan** mode first, then switch to **Agent** to implement. Review the diff before you run anything.

Short on time? Commit or stash your work, run `git switch session-6`, and add your agent's name to `src/CaseHandling.Agent/appsettings.json`.

## 2. Follow one case end to end

Run the agent, type `Assess case C-1003`, and approve the proposal. Follow the run in the console and in Foundry:

1. **Request:** your message and the case the agent read.
2. **Tools:** the calls to the Case API, the policy server, and your Foundry agent, with their arguments and results.
3. **Foundry:** open your agent in the Foundry portal, and find the run your C# agent started in **Tracing**. Which web pages did it use?
4. **Proposal and approval:** what did the case-handling agent propose, which policies and sources did it cite, and what did you approve?
5. **Result:** check `GET /admin/changes`. Exactly one change should be recorded.

Answer:

- Which steps ran in C#, and which ran in Foundry?
- Where does text from outside your code enter the agent: the case, the policies, the web? What stops it from acting as instructions?
- Where is the approval boundary, and what stops the agents from going around it?

## 3. Plan the next iteration

Answer one question for each topic:

| Topic | Question | Your answer |
| --- | --- | --- |
| Improving | What is the next change, and what brief would you give GitHub Copilot? | |
| Evaluating | How do you measure the agent before a new version goes out? Which cases would you test, and what is the expected result for each? | |
| Hosting | Where does the agent run as a service, and what replaces `az login`? | |
| Operating | Who reads the traces, who approves proposals in production, and which content must never be logged? | |

Write the three most important items, most important first, in `backlog.md`. The first item must be small enough to implement and verify with the same workflow you used today.

## Output of this session

- A C# agent that uses your Foundry agent as a tool, with tests
- One case followed from the request to the result, in the console and in Foundry
- Answers for improving, evaluating, hosting, and operating the agent
- A backlog with three items
