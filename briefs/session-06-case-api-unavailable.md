## Goal
When the agent cannot read the case, it proposes nothing, and nothing is sent for approval.

## Context
Today `ProposedStatus` is required. With the Case API unavailable (`POST /admin/faults` with `{ "unavailable": true }`), `Assess case C-1001` still returns `AwaitingInformation` and asks the customer for information the case already contains. A case handler could approve that.

## Expected behavior
- `ProposedStatus` may be null. The instructions say to use null when a tool returned an error and the case could not be read.
- Then `MissingInformation` is empty, `CustomerQuestion` is null, and `Reasoning` says what failed.
- The workflow sends a proposal without a status straight to the outcome. It is never sent for approval and never written.

## Failure behavior
- If the agent still proposes a status without having read the case, that is a model failure, not a code failure. The code guarantee is that nothing without a status reaches approval.

## Acceptance criteria
- [x] A workflow test shows that a proposal without a status skips approval and writes nothing.
- [x] Existing tests still pass.
- [x] With the fault enabled, `Assess case C-1001` gives no proposal and says the Case API is unavailable.

## Out of scope
- Retrying the Case API.
- Changes to the Case API.
