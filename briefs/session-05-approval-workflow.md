## Goal
A proposal from the case-handling agent can be carried out, but only after a case handler approves it.

## Context
The agent in `src/CaseHandling.Agent` investigates a case with `get_case` and the policy tools, and returns a structured proposal (`CaseAssessment`). The Case API changes a status with `PUT /cases/{id}/status`, and `GET /admin/changes` lists every write. Use the `agent-framework` skill; the workflow uses `Microsoft.Agents.AI.Workflows`.

## Expected behavior
- An Agent Framework workflow, `CaseWorkflow`, with these steps in a fixed order:
  1. **Investigate and propose:** runs the agent with the case handler's message and gets its proposal.
  2. **Approve:** a request port pauses the workflow; the console asks the case handler to approve or reject.
  3. **Write:** only if approved, calls `PUT /cases/{id}/status` with the proposed status and a reason.
- A proposal without a case id goes straight to the outcome, without approval or write.
- The console prints the proposal, the approval question, and what was written.

## Failure behavior
- The agent has no tool that can write. Only the write step can write.
- `Approved` and `Rejected` can never be written: the proposal type cannot represent them, and `CaseStatusWriter` refuses them.
- A rejected proposal writes nothing.

## Acceptance criteria
- [x] Approve writes exactly one change (`CaseWorkflowTests`).
- [x] Reject writes nothing.
- [x] Approval is asked before anything is written.
- [x] A proposal without a case id is never sent for approval.
- [x] `Approved` and `Rejected` are never written.
- [x] The tests run without calling the model.

## Out of scope
- No changes to the Case API or to `data/`.
- Approval anywhere other than the console.
