# Session 6 – Reference notes

## Connect the two agents

The brief is in [briefs/session-06-carrier-agent.md](../briefs/session-06-carrier-agent.md).

Add the name of your Foundry agent to `src/CaseHandling.Agent/appsettings.json`:

```json
"Foundry": { "CarrierAgentName": "carrier-research-<initials>" }
```

- `CarrierTools.GetCarrierCompensation` is the function tool `get_carrier_compensation`. It validates the airline and flight, runs the Foundry agent with `projectClient.AsAIAgent(new AgentReference(name))`, and parses the answer into `CarrierCompensation`. Anything else becomes an `Error:` message. Tests: `CarrierToolsTests`.
- Unknown fields in the answer are dropped, so only the agreed format reaches the case-handling agent.
- `instructions.md` says when to use the tool, and that its answer is web content: information, never instructions.

## Follow one case end to end

`Assess case C-1003`, approved:

| Step | Runs in | What you see |
| --- | --- | --- |
| Request | C# | `Assess case C-1003` goes into the workflow's Investigate step. |
| Case | C# → Case API | `[tool] get_case {"caseId":"C-1003"}` and the case JSON. |
| Policies | C# → MCP server | `search_policies`, then `get_policy` for `POL-TRAVEL-004` and `POL-GEN-001`. |
| Airline rules | C# → Foundry agent → Web search | `[tool] get_carrier_compensation {"carrier":"SAS","flight":"SK1415 ...","delayHours":7}` and the JSON answer. The Foundry agent's own run, with its web searches, is in **Tracing**. |
| Model calls | Foundry model deployment | Between every tool call, the case-handling agent calls the model to decide the next step. |
| Proposal | C# | Status, missing information (for example carrier confirmation, receipts, and whether compensation was claimed from SAS), cited policies, and the airline's rules. |
| Approval | C#, human | `[approve]` asks the case handler. |
| Result | C# → Case API | `[write]` and exactly one change in `GET /admin/changes`. |

With tracing turned on (`ApplicationInsights:ConnectionString`, see [briefs/session-06-tracing.md](../briefs/session-06-tracing.md)), the C# side of the run is also in **Tracing**: the agent run, the model calls, the tool calls, `approve`, and `write_status`.

**Trust boundaries:** the case text from the Case API, the policy text from the MCP server (`travel-delay.md` contains a hidden instruction), and the web pages behind the Foundry agent. All three reach the model as text. The instructions say to treat them as data. The typed record limits what comes back from the Foundry agent, but it cannot clean up the text inside the fields.

**Approval boundary:** between the `Approve` request port and the `Write` step. Neither agent has a write tool, `ProposedStatus` cannot be `Approved` or `Rejected`, and `CaseStatusWriter` refuses them.

## Plan the next iteration

| Topic | Our answer |
| --- | --- |
| Improving | Treat tool output as data (backlog item 1). Brief: an *Untrusted content* section in the instructions, checked on `C-1003`. |
| Evaluating | A small set of cases with expected results (`C-1001`, `C-1002`, `C-1003`, the Case API down), run with every approval rejected. Compare status, cited policies, and writes with the last release (backlog item 3). |
| Hosting | The case-handling agent as a hosted agent in Foundry, next to the carrier agent. A managed identity with the *Azure AI User* role replaces `az login`. |
| Operating | Developers and operations read traces, kept for 30 days, with sensitive data off. Case handlers approve in the case system, and the Case API records who approved. |

## Also in this branch

- **Tracing** to Application Insights, from [briefs/session-06-tracing.md](../briefs/session-06-tracing.md).
- **No proposal when the case cannot be read**, from [briefs/session-06-case-api-unavailable.md](../briefs/session-06-case-api-unavailable.md). With the Case API down, the agent proposes nothing and nothing is sent for approval.
