Insurance case-handling sample (.NET 10): a Case API, a policy MCP server, and an AI agent that helps handle claims.

## Layout

- `src/CaseHandling.Domain` – `Case` model, `CaseValidator`, `CaseStatusTransitions`. Pure logic, no I/O.
- `src/CaseHandling.CaseApi` – Minimal API over `data/cases/cases.json` (in-memory `CaseStore`). `/admin` endpoints are for exercises.
- `src/CaseHandling.PolicyMcp` – MCP server exposing `data/policies/*.md`.
- `src/CaseHandling.FoundrySmokeTest` – Direct model call to verify Foundry access.
- `src/CaseHandling.Agent` – The application agent (Microsoft Agent Framework).
- `src/CaseHandling.Tests` – xUnit v3 tests for all projects.
- `data/` – Test data and policy documents. Treat as fixtures; do not edit unless asked.
- `data/policies/*.md` – The business rules. Read `general-claims-handling.md` (POL-GEN-001) and the category policy before changing validation or case handling.
- `briefs/` – Task briefs with acceptance criteria, one per task. Read the brief before you plan.

## Build and test

```powershell
dotnet build CaseHandling.slnx
dotnet test --solution CaseHandling.slnx
```

## C# conventions

- File-scoped namespaces, nullable enabled, implicit usings (`Directory.Build.props`).
- `sealed record` for models and DTOs; `required` / `init` properties; collection expressions (`[]`).
- Static classes for pure domain logic; private fields `_camelCase`.
- Always use braces; prefer pattern matching (`is { } x`, `is not { } x`, `is <= 0`).
- Use `nameof(...)` for field names in validation issues.
- API: Minimal API with `MapGroup`; request DTOs are `sealed record XxxRequest` at the bottom of `Program.cs`.

## Error responses

All Case API errors use ProblemDetails: `Results.Problem(title:, detail:, statusCode:)`, or `Results.NotFound()` for unknown ids. Do not return plain strings (e.g. `Results.BadRequest("...")`). Use 400 for invalid input, 404 for unknown case, 409 for disallowed status changes.

## Tests

- Method names describe behavior in snake case: `Missing_description_is_reported`, `Get_unknown_case_returns_404`.
- Arrange / act / assert separated by blank lines; use `TestContext.Current.CancellationToken`.
- Domain rules (validation, transitions) → unit tests against `CaseHandling.Domain` (e.g. `CaseValidatorTests`, start from `ValidCase() with { ... }`).
- HTTP behavior (status codes, response shape, routing) → `CaseApiTests` using `WebApplicationFactory<Program>`.

## Approval rule

Approving or rejecting a case is a business decision. Code and agents may *propose* a decision but must never set `Approved` or `Rejected` without explicit human approval.

## Packages and configuration

- Package versions live only in `Directory.Packages.props` (central package management). No `Version` attributes in `.csproj` files.
- Settings go in each project's `appsettings.json`. There are no keys or secrets: Azure access uses `AzureCliCredential` (`az login`). Never add API keys.

## Definition of done

- Every acceptance criterion is covered by at least one test.
- `dotnet build` passes with no new warnings, and `dotnet test` passes.
- Change is minimal and follows the conventions above; no unrelated edits.
- No changes to `data/` or existing tests unless the task asks for it.
- Summary lists changed files, tests added, and any open questions.
