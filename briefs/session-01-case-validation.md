## Goal
A case that is missing information or has inconsistent dates is reported by validation, so a case handler knows it cannot be worked on yet.

## Context
Validation lives in `src/CaseHandling.Domain/CaseValidator.cs` and returns a list of `ValidationIssue` instead of throwing. Tests are in `src/CaseHandling.Tests/CaseValidatorTests.cs` (domain) and `CaseApiTests.cs` (HTTP). The required information comes from the general claims-handling policy in `data/policies/general-claims-handling.md` (POL-GEN-001), section *Required information*. The date rule comes from the case handlers: an incident cannot happen after it was reported.

## Expected behavior
- A complete case, such as `C-1001`, returns no issues.
- `C-1002` returns one issue on `IncidentDate` and one on `ClaimedAmount`.
- `C-1007` (incident after reported) returns an issue on `IncidentDate`.

## Failure behavior
- A missing `IncidentDate` adds an issue on `IncidentDate`.
- A missing `ClaimedAmount` adds an issue on `ClaimedAmount`.
- An `IncidentDate` after the `ReportedDate` adds an issue on `IncidentDate`.
- Edge case: an incident date equal to the reported date is valid.
- Problems are reported as issues, never thrown, like the existing checks.

## Acceptance criteria
- [x] Each failure behavior and the edge case has its own unit test in `CaseValidatorTests`.
- [x] A valid case still returns no issues.
- [x] `GET /cases/{id}/validation` reports the expected issues for `C-1002` and `C-1007`, and nothing for `C-1001`.
- [x] Tests use fixed dates, not `DateTime.Now`.
- [x] `dotnet test --solution CaseHandling.slnx` passes.

## Out of scope
- No changes to `data/` or to existing tests.
- No changes to the Case API endpoints.
- Decisions about a claim, such as the 12-month reporting rule. Whether a late claim is accepted is for the case handler to decide, not for validation.
