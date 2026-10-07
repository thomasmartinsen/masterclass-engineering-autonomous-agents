# Session 5 – Reference notes

## The case journey

| Step | Model judgment or code control? | Why |
| --- | --- | --- |
| Read the case | Model decides to call `get_case`; code validates the id and handles errors | The model must know which case it is talking about, but the tool decides what is a valid request. |
| Find the relevant policy | Model judgment (MCP tools) | Choosing and reading the right policy is a judgment over text. |
| Propose the next step | Model judgment, constrained by code | The `ProposedStatus` enum cannot represent `Approved` or `Rejected`. |
| Ask a human to approve | Code control | The workflow always stops at the `Approve` request port. The model cannot skip it. |
| Write the new status to the case | Code control | Only the workflow's `Write` step can write, and only after a yes. The agent has no write tool. |

## Brief: function tool for the Case API

The brief is in [briefs/session-05-case-tool.md](../briefs/session-05-case-tool.md). The tests are in `CaseToolsTests`.

## Policy MCP server tools

Listed when the agent starts:

- `get_policy` – Returns the full text of a policy by its id, for example POL-WATER-002.
- `search_policies` – Finds policies that mention any of the given search terms or match a case category.
- `list_policies` – Lists all claims-handling policies with their id, title, and category.

## Workflow

```text
input ──▶ Investigate (agent + read tools) ──▶ CaseId? ──no──▶ NoCase (output, nothing written)
                                                 │
                                                yes
                                                 ▼
                                         Approve (request port, human)
                                                 │
                                                 ▼
                                    Write (PUT /cases/{id}/status only if approved)
```

## Results

`CaseWorkflowTests` checks the workflow without a model: approve writes exactly once, reject writes nothing, approval is asked before anything is written, a proposal without a case id is never sent for approval, and `Approved` or `Rejected` is never written.

A recorded run, with the Case API reset before it:

- `Assess case C-1002`: the agent called `get_case`, `search_policies`, and `get_policy` for `POL-THEFT-003` and `POL-GEN-001`. It proposed `AwaitingInformation` and asked for the incident date, the claimed amount, the police report number, proof of ownership, and the frame number. **Rejected** → `GET /admin/changes` showed no change.
- `Assess case C-1001`: the agent read the case and `POL-WATER-002`. **Approved** → `GET /admin/changes` showed exactly one status change.

The agent proposed `AwaitingInformation` for `C-1001`, because photos and the plumber invoice are not in the case. That follows `POL-GEN-001`, which requires category documentation before `UnderReview`. Whether case handlers want that for a clear water damage case is a question for the policy owner, not a code fix.
