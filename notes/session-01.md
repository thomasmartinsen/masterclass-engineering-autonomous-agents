# Session 1 – Reference notes

The task brief is in [briefs/session-01-case-validation.md](../briefs/session-01-case-validation.md).

## The one-line request

*"Fix the validation on cases."* What an agent typically does:

1. **Explores:** lists the repository, opens `CaseValidator.cs`, the `Case` model, `cases.json`, and the existing tests. It may also open the policies, the README, or the exercises, if it decides they are relevant.
2. **Decides what "fix" means:** usually missing `IncidentDate` and `ClaimedAmount` (the only nullable fields, both `null` in `C-1002`). Whether it also adds the date-order check (`C-1007`) depends on what it happened to read and how it interprets the request. Some agents also add the policy's 12-month reporting rule, but that is a decision for the case handler, not a validation issue.
3. **Edits and verifies:** changes `CaseValidator`, often adds domain tests, runs the build and the tests. The first `dotnet test` may fail without `--solution`, and the build fails while the Case API is running.

The result can be good or incomplete. Either way, you only know by checking it against what the case handlers asked for.

## With a brief and a plan

- The agent goes straight to `CaseValidator`, its tests, and the policy, because the brief names them.
- The plan shows its intended changes before any file is touched, so you can correct it cheaply.
- The scope is yours, not the agent's: both cases, the edge case, an API test, and what is out of scope.

## What we learned

1. **How the agent works**
   - It works in a loop: read and search, decide, edit, run the build and the tests, read the output, and adjust.
   - It decides on its own where to look and when it is done.
2. **Where its context came from**
   - The code, the test data, and any documents it chose to open.
   - The files open in the editor.
   - The brief and the plan, when we gave them.
3. **What the brief and the plan changed**
   - Less exploring, a predictable scope, and a result we could check criterion by criterion.
   - We reviewed a plan before any code was written.
4. **Definition of done**
   - Every acceptance criterion has a test.
   - Build has no new warnings and all tests pass.
   - Minimal change that follows existing style; no edits to `data/` or existing tests unless asked.
   - Summary lists changed files, tests, and open questions.
