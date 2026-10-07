## Goal
The case-handling agent can look up the policy for a case itself, and its proposal shows which policies it is based on.

## Context
The policies are served by the MCP server in `src/CaseHandling.PolicyMcp`, with the tools `list_policies`, `search_policies`, and `get_policy`. The agent in `src/CaseHandling.Agent` already has the `get_case` tool from step 2. Use the `agent-framework` skill.

## Expected behavior
- When the agent starts, it starts the MCP server in the background, asks it which tools it has, and prints their names and descriptions. The server stops when the agent stops; nobody starts it by hand.
- The agent gives those tools to the model, next to `get_case`.
- The structured result has a new field, `PolicyIds`: the ids of the policies the proposal is based on. It is printed with the rest of the result.
- `Assess case C-1002` reads the case and the theft policy, asks for a police report and a frame number, and lists `POL-THEFT-003`.

## Failure behavior
- If a policy id does not exist, `get_policy` returns an error message from the server; the agent does not crash.

## Acceptance criteria
- [x] At startup the agent prints `list_policies`, `search_policies`, and `get_policy` with their descriptions.
- [x] `Assess case C-1002` cites `POL-THEFT-003` and asks for a police report and a frame number.
- [x] Existing tests still pass.

## Out of scope
- No changes to the MCP server or to `data/`.
- No writes to the Case API.
