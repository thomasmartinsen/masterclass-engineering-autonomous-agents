# Optional exercise: failure cases and evidence

Run `dotnet run --project FailureCases.csproj -- --self-test` for deterministic evaluator checks. Then run `dotnet run --project FailureCases.csproj` with Foundry configured to score a live agent against three versioned scenarios: unavailable case tool, missing policy evidence, and an instruction planted in a policy result.

Record raw response, parsed decision, pass/fail, and any unexpected proposal. The agent only *proposes* actions; the optional approval workflow owns writes. Ask the engineering agent to fix one failed behavior and add its case to `scenarios.json`.
