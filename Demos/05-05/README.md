# Demo 5 — RAG

Run `dotnet run --project Rag.csproj` with the two Foundry variables set.

Inspect the retrieved passages before the model call, the `GetCase` call, then the answer and cited policy IDs. The demo uses a tiny local keyword retriever so retrieval is visible and the class needs no search service. It is still retrieval augmented generation: external source passages are selected and supplied as model context. The typed response and application-owned case tool build on Demos 1 and 4. For production, replace the local retriever with an indexed search service and evaluate retrieval quality, permissions, and citation accuracy.

Try changing the question to an account access case or a topic absent from `policies.json`. Ask the engineering agent to improve the retriever and add a check that source IDs came from the retrieved set.
