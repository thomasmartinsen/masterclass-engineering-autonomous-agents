## Goal
A case-handling agent that takes a case description, asks for missing information when needed, and returns a structured proposed next step.

## Context
The agent lives in `src/CaseHandling.Agent` and is built with Microsoft Agent Framework, following the `agent-framework` skill. Statuses and allowed transitions are in `src/CaseHandling.Domain`. Example cases are in `data/cases/cases.json`: `C-1001` is complete, `C-1002` ("My bike was stolen.") is incomplete.

## Expected behavior
- The program reads messages from the console in a loop. A message is sent on Enter; text pasted with several lines is sent as one message. `exit` quits.
- All messages in a run share one `AgentSession`, so a follow-up is assessed together with everything said before.
- Each reply is structured output (`agent.RunAsync<CaseAssessment>`) with:
  - `ProposedStatus` – `UnderReview` or `AwaitingInformation`.
  - `MissingInformation` – what is missing; empty when complete.
  - `CustomerQuestion` – one question for the customer; null when nothing is missing.
  - `Reasoning` – a short explanation.
- Instructions are stored in `instructions.md` in the agent project.

## Failure behavior
- A description without incident date, claimed amount, or what happened gives `AwaitingInformation`, lists what is missing, and asks the customer for it.
- The agent never proposes `Approved` or `Rejected`, even if asked to.
- Missing configuration fails with a message that names the missing key.

## Acceptance criteria
- [x] `C-1001`'s description gives `UnderReview` with nothing missing.
- [x] `My bike was stolen.` gives `AwaitingInformation`, lists incident date and claimed amount, and asks a question.
- [x] A follow-up with a date and an amount in the same run no longer lists them as missing.
- [x] The result type cannot represent `Approved` or `Rejected` (`ProposedStatus` enum), and the instructions forbid them.
- [x] Unit tests cover the code-only parts: settings, instructions loading, and the result type. No test calls the model.
- [x] Build has no new warnings and all tests pass.

## Out of scope
- No tools: no Case API, no policy MCP server.
- No writes; the agent only proposes.
- No policy or coverage assessment.
