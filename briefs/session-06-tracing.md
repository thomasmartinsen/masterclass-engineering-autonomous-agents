## Goal
Every case run can be followed step by step in Tracing in the Foundry portal.

## Context
The agent is in `src/CaseHandling.Agent` (see the `agent-framework` skill). The Foundry project has an Application Insights resource connected. The workflow (`CaseWorkflow`) investigates, asks for approval, and writes.

## Expected behavior
- The connection string is read from `ApplicationInsights:ConnectionString` in `src/CaseHandling.Agent/appsettings.json`.
- Agent runs, model calls, and tool calls are traced, with tool arguments and results.
- The approval step and the write are traced, with the case id, the proposed status, the decision, and whether the write succeeded.
- New packages are added to `Directory.Packages.props`.

## Failure behavior
- Without a connection string the agent runs as before and says that tracing is off.

## Acceptance criteria
- [x] A unit test shows that tracing is off without a connection string.
- [x] A unit test shows that an approved run produces `approve` and `write_status` spans with the expected tags.
- [ ] `C-1001`, approved, appears in Tracing with model calls, tool calls, approval, and write. Check this with your own connection string.
- [x] No connection string or endpoint is committed.

## Out of scope
- Metrics and logs.
- Changes to the Case API or the policy MCP server.
