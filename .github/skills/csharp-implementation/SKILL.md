---
name: csharp-implementation
description: Procedure for implementing a feature or bug fix in the C# code of this repository (domain, Case API, MCP server, agent). Use when asked to add, change, or fix C# behavior, including validation rules and API endpoints.
---

# C# implementation

Follow these steps in order. Do not skip a step.

## 1. Ask

If the task has no acceptance criteria, stop and ask for them. Propose a short list based on the task, but do not start coding until they are confirmed. Do not guess.

## 2. Inspect

Before changing anything:

- Read the code you will change and its neighbors (same file, same folder).
- Read the existing tests for that code and copy their structure and naming.
- Check `data/` for test data that exercises the case (read only).

## 3. Implement

- Make the smallest change that meets the criteria. No refactoring, renaming, or cleanup on the side.
- Add one test per acceptance criterion, in the right test class (domain vs. API).
- Do not touch `data/` or existing tests unless the task says so. If an existing test seems wrong, report it instead of changing it.

## 4. Verify

Run the build and the tests from the repository root. If something fails, fix it and run both again. Do not report success without a green run.

## 5. Report

End with a summary a reviewer can act on:

- **Changed files** – one line each, what and why.
- **Criteria → tests** – a table mapping each acceptance criterion to the test that proves it.
- **Verification** – the build and test commands and their result (pass/fail counts).
- **Open issues** – assumptions, things you noticed but did not change, follow-ups.
