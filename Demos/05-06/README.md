# Optional exercise: controlled update behind human approval

With the Foundry variables set, run `dotnet run --project ApprovalWorkflow.csproj`. The agent drafts a proposal. A Microsoft Agent Framework workflow then pauses at a `RequestPort` and receives a human decision. The demo runs both decisions with test data: one approval should create exactly one in-memory write; rejection should create none. The code checks that no write exists when approval is requested.

During the class, replace the scripted `approved: true` / `approved: false` values with participant input. The workflow is intentionally a prepared skeleton; participants focus on the approval boundary and the code that writes only after a positive decision.
