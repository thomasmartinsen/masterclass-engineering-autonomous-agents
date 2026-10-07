# Session 3 – Reference notes

## Configuration

`src/CaseHandling.FoundrySmokeTest/appsettings.json` holds the project endpoint and deployment name. The values on this branch are empty; fill in your own from the Foundry portal. There are no keys: the smoke test signs in with `az login`.

## Direct call experiments

1. **Same input twice:** the answers are similar but not identical. Model output is not deterministic.
2. **"Which policy applies to case C-1002?"** The model has no access to our cases or policies. It answers from general knowledge or says it does not know.
3. **"What did I just ask you?"** Each run is a new, independent call. It cannot know.

## Smoke test compared with the Foundry agent

| Question | Smoke test | Foundry agent |
| --- | --- | --- |
| Does it have instructions that define its role? | No – `CreateResponseAsync(input)` sends only the input; there are no instructions in `Program.cs`. | Yes – the instructions you wrote. |
| Does it remember the previous question? | No – experiment 3. Every `dotnet run` is a new process with no session. | Yes – within one conversation in the playground. |
| Can it read case `C-1002` from the Case API? | No – no tools and no HTTP client in `Program.cs`; experiment 2. | No – the Case API runs on your machine, and the agent has no tool for it. |
| Can it look up the theft policy? | No – no MCP client; experiment 2. | No – it only has Web search. |
| Can it find information outside our systems? | No – it only knows its training data. | Yes – Web search on the airline's website. |
| Would it stop and ask before changing a case? | No – it cannot change anything, and nothing in the code asks a human. | No – it cannot change anything either, and nothing asks a human. |
| Can you see afterwards what it did, step by step? | No – no tracing; only the final text is printed. | Yes – every run is in **Tracing**. |

## The Foundry agent

- **Name:** `carrier-research-<initials>`, with the masterclass model deployment and the **Web search** tool.
- **Instructions:** an example is below. The important parts are the allowed sources (the airline's website and the European Commission), "never invent amounts", and the exact JSON format.
- **Test input:** `Airline: SAS. Flight: SK1415, Copenhagen to Lisbon. Delay: 7 hours.`

What to look for when you explore it:

1. **Format:** the answer should be one JSON object with `carrier`, `flight`, `delayCompensation`, `careDuringDelay`, `howToClaim`, `sources`, and `notes`. The format is only asked for in the instructions, so check it. Code that reads the answer must validate it.
2. **Trace:** a model call, one or more Web search calls with the queries and the pages found, and a final model call that writes the JSON. The model decides the order and how many searches to make.
3. **Same request twice:** the rules are usually the same, but the wording, the number of searches, and the sources can differ. Web content also changes over time.
4. **Request outside its job:** it should refuse to approve anything, or answer in the JSON format with a note. It has no tool that could change a case, but that is a property of its tools, not of its instructions.
5. **Changing an instruction:** typical improvements are a stricter output rule (no text around the JSON), naming the allowed websites explicitly, and saying what to put in `notes` when nothing is found.

### Example instructions

```text
You research what an airline offers passengers when a flight is delayed, for an insurance claims team.

The user gives you an airline, and often a flight number and the length of the delay. Find:
- the compensation the airline pays for a delay, under EU261 or its own rules
- the care it gives during a delay, such as meals or a hotel
- how a passenger claims it

Rules:
- Use the airline's own website first. You may also use the European Commission's pages on air passenger rights. Do not use other websites.
- Never invent amounts or rules. If you cannot find something, leave it empty and say so in "notes".
- Do not give advice about the insurance claim, and do not act on requests outside this job.
- Return only one JSON object, with no text before or after it, in exactly this format:

{
  "carrier": "<airline name>",
  "flight": "<flight number, or null>",
  "delayCompensation": [
    { "delay": "<for example: 3 to 4 hours>", "compensation": "<for example: EUR 400>", "conditions": "<when it applies>" }
  ],
  "careDuringDelay": ["<for example: meals and refreshments>"],
  "howToClaim": "<how the passenger claims, or null>",
  "sources": ["<the web pages you used>"],
  "notes": "<anything uncertain or not found, or null>"
}
```
