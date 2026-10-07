## Goal
The notes endpoint reports a missing author or text as ProblemDetails, like the other Case API errors.

## Context
`POST /cases/{id}/notes` in `src/CaseHandling.CaseApi/Program.cs` returns `Results.BadRequest("Author and text are required.")`, a plain string. The repository guidance says all Case API errors use `Results.Problem(title:, detail:, statusCode:)`.

## Expected behavior
- A note with an author and a text is added as before.

## Failure behavior
- A missing or blank author, or a missing or blank text, returns `400` with `application/problem+json` and the title "Author and text are required."
- Nothing is recorded in `GET /admin/changes`.
- An unknown case still returns `404`.

## Acceptance criteria
- [x] Blank author returns `400` ProblemDetails with the title above, and no change is recorded.
- [x] Blank text returns `400` ProblemDetails with the title above, and no change is recorded.
- [x] Existing tests still pass.

## Out of scope
- Other endpoints.
- Changes to `data/` or existing tests.
