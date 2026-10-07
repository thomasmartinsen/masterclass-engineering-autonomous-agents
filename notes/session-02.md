# Session 2 – Reference notes

## Session 1 notes sorted into layers

| Note from Session 1 | Layer | Where it went |
| --- | --- | --- |
| The business rules are in `data/policies/` | Instructions | `copilot-instructions.md` → Layout |
| Build and test commands, `--solution` flag | Instructions | `copilot-instructions.md` → Build and test |
| Test naming and where tests belong | Instructions | `copilot-instructions.md` → Tests |
| `data/` is test data, do not edit | Instructions | `copilot-instructions.md` → Layout, Definition of done |
| Report validation issues, do not throw | Instructions | `copilot-instructions.md` → C# conventions |
| Fixed dates in tests | Skill | `csharp-implementation` → Implement |
| Ask for acceptance criteria before coding | Skill | `csharp-implementation` → Ask |
| Run build and tests before reporting | Skill | `csharp-implementation` → Verify |
| Review the diff against the criteria | Agent | `reviewer.agent.md` |
| Definition of done | Instructions | `copilot-instructions.md` → Definition of done |

## Decisions

- **Error responses:** the Case API mixed `Results.BadRequest("...")` (notes endpoint) and `Results.Problem(...)` (status endpoint). The guidance picks ProblemDetails for all errors. Step 6 fixes the notes endpoint.
- **Reviewer tools:** `read`, `search`, and `execute`. `execute` is needed for `git show` and `git diff`. The instructions limit it to read-only git commands and forbid editing files.

## New task with customizations

Request, with no brief: *"The notes endpoint in the Case API returns a plain string when the author or the text is missing. Fix it."*

What to look for, compared with Session 1:

- The `csharp-implementation` skill appears in the references of the response.
- The agent asks for acceptance criteria before it changes code, and proposes some. The agreed criteria are in [briefs/session-02-notes-problem-details.md](../briefs/session-02-notes-problem-details.md).
- The fix uses `Results.Problem(title:, statusCode:)` with 400, as the error-response rule says, without being told.
- The test is an API test in `CaseApiTests` with a snake-case name: `Note_without_author_or_text_returns_400_problem_details`.
- The summary lists changed files, criteria → tests, and the test result.
