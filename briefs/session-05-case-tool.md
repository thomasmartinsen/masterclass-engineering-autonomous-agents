## Goal
The case-handling agent can read a case from the Case API by its id.

## Context
The agent is in `src/CaseHandling.Agent` (see the `agent-framework` skill). The Case API is `src/CaseHandling.CaseApi`; `GET /cases/{id}` returns a case, `404` for an unknown id, and `503` when `POST /admin/faults` has made it unavailable. Tests can run the Case API in memory with `WebApplicationFactory<Program>`, like `CaseApiTests`.

## Expected behavior
- A function tool `get_case` takes a case id and calls `GET /cases/{id}`. It returns the case JSON.
- The Case API's base address comes from `CaseApi:BaseUrl` in `appsettings.json`. It is not hard-coded.
- The tool and its parameter have descriptions that tell the model when to use it.

## Failure behavior
- The tool never throws. Every failure is a message that starts with `Error:`.
- An invalid id (not `C-` followed by 4–8 digits) is rejected before any HTTP call.
- `404` → the case does not exist; ask the case handler to check the id.
- `503` or no connection → the Case API is unavailable; do not guess case details.

## Acceptance criteria
- [x] `C-1002` returns the case JSON.
- [x] `C-9999` returns a not-found message.
- [x] With the fault enabled, the tool returns the unavailable message.
- [x] With no server, the tool returns the unavailable message.
- [x] Invalid ids, including path tricks such as `C-1001/../../admin/reset`, are rejected without an HTTP call.
- [x] The tool only reads. The agent has no tool that writes.

## Out of scope
- No changes to the Case API or to `data/`.
- No write operations.
