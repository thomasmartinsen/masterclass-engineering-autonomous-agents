# Demo 5: review and test the generated agent

Run `dotnet run --project ReviewAndTests.csproj -- --self-test` before making a model call. It checks meaningful response-contract failures: wrong case ID and a missing clarification question. Then, with Foundry configured, run `dotnet run --project ReviewAndTests.csproj` to inspect a real two-turn case. Ask the engineering agent to propose one improvement, review its diff, rerun the checks, and update the engineering skill from Demo 3 with the lesson learned.

The local checks validate the *contract*, not model quality. A live run can still vary; inspect the raw output if it fails the contract.
