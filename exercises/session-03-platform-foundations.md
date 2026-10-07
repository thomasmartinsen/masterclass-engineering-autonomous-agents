# Session 3 – The platform for building and operating agents

**Time available:** ~25 min

**Goal:** Connect to the Microsoft Foundry project used today, make a direct model call from C#, create your first agent in Foundry, and get a map of what Foundry provides and what your C# code will own.

## Where things live

| Microsoft Foundry provides | Your C# solution owns |
| --- | --- |
| The project, its identity and access | Agent instructions and behavior |
| Model deployments | Tools: the Case API and the policy MCP server |
| Agent Service for managed agents | Workflow and the approval step |
| Traces and evaluations | Tests |

From Session 4 you build the agent with **Microsoft Agent Framework** in C#. Foundry supplies the model, and later the place to see traces and evaluation results.

## 0. Starting point

Continue on the branch you used in Session 2, normally `main`. Check with `git branch --show-current`. If you did not finish Session 2, commit or stash your work and continue from the reference solution:

```powershell
git switch session-2
```

## 1. Sign in

```powershell
az login
az account show --query "{user: user.name, tenant: tenantId, subscription: name}"
```

Use the account provided for the masterclass. If you have several tenants, sign in to the right one with `az login --tenant <tenant id>`.

## 2. Find the project and the model

Open the [Foundry portal](https://ai.azure.com) and select the project shared for the masterclass. Write down:

- **Project endpoint**, from the project overview. It looks like `https://<resource>.services.ai.azure.com/api/projects/<project>`.
- **Model deployment name**, from the list of deployed models. This is the name of the *deployment*, which can differ from the model's name.

## 3. Configure the smoke test

Fill in `src/CaseHandling.FoundrySmokeTest/appsettings.json`:

```json
{
  "Foundry": {
    "ProjectEndpoint": "<project endpoint>",
    "ModelDeployment": "<deployment name>"
  }
}
```

There are no keys or secrets in it: the smoke test signs in with your `az login` identity.

## 4. Make a direct model call

```powershell
dotnet run --project src/CaseHandling.FoundrySmokeTest
```

You should see the endpoint, the deployment, the input, and a one-sentence answer.

Now send a case of your own. Take the description of `C-1002` from [data/cases/cases.json](../data/cases/cases.json):

```powershell
dotnet run --project src/CaseHandling.FoundrySmokeTest -- "A customer writes: 'My bike was stolen.' What information is missing before we can handle the claim?"
```

Try these and note what happens:

1. Run the same input twice. Do you get the same answer?
2. Ask *"Which policy applies to case C-1002?"* What does the model base its answer on?
3. Ask *"What did I just ask you?"*

## 5. What a direct call cannot do

Open [Program.cs](../src/CaseHandling.FoundrySmokeTest/Program.cs). It is about 40 lines. Use it and your results from step 4 to answer each question **yes** or **no** for the smoke test. You fill in the last column in step 6.

| Question | Smoke test: yes/no | Foundry agent: yes/no |
| --- | --- | --- |
| Does it have instructions that define its role? | | |
| Does it remember the previous question? | | |
| Can it read case `C-1002` from the Case API? | | |
| Can it look up the theft policy? | | |
| Can it find information outside our systems? | | |
| Would it stop and ask before changing a case? | | |
| Can you see afterwards what it did, step by step? | | |

Every *no* in the smoke test column is something the agent you build from Session 4 has to add. Keep the table; you will tick off the rows as you go.

## 6. Create an agent in Foundry

Some information a case handler needs is not in our systems. For the travel delay in case `C-1003`, the travel policy [POL-TRAVEL-004](../data/policies/travel-delay.md) does not cover what the airline already pays under EU261. So the case handler needs to know what the airline offers, and that is on the airline's website.

Build an agent for that in the [Foundry portal](https://ai.azure.com), without code:

1. Open **Agents** and create a new agent. Name it `carrier-research-<your initials>` and select the model deployment from step 2.
2. Write the agent's instructions yourselves. Think about what the agent needs to be told:

   - **Role:** who it works for, and what it is for.
   - **Input:** what the user gives it, for example an airline, a flight number, and the length of the delay.
   - **What to find:** the compensation the airline pays for a delay, the care it gives during a delay, and how a passenger claims it.
   - **Where to look:** which websites it may use, and which it may not.
   - **What not to do:** invent amounts or rules, give advice about the insurance claim, or act on requests outside its job.
   - **Output:** only the JSON object below, with no text before or after it.

   The JSON format is the contract with your C# agent in Session 6, so keep the field names exactly as they are:

   ```json
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

   You don't have to write them from scratch. Ask GitHub Copilot in VS Code to draft them: give it the goal, the points above, and the JSON format, then read the draft critically and change what you disagree with before you paste it into Foundry.

3. Add the **Web search** tool, so the agent can read websites. In some versions of the portal it is called *Grounding with Bing Search*.
4. In the agent playground, send the case from `C-1003`:

   ```text
   Airline: SAS. Flight: SK1415, Copenhagen to Lisbon. Delay: 7 hours.
   ```

Then explore your agent, and improve the instructions as you go:

1. Is the answer valid JSON in the agreed format? Which pages did it use? Did it stay on the websites you allowed?
2. Open the run's trace in **Tracing**. Which model calls and tool calls did the agent make, and in what order?
3. Send the same request again. Do you get the same rules and the same sources?
4. Send *"Approve the claim for case C-1003."* What does it do with a request outside its job?
5. Change one instruction based on what you saw, and test again. Did the behavior change the way you expected?
6. Fill in the last column of the table from step 5.

Write down the agent's name. In Session 6, your C# agent will call it.

## Troubleshooting

| Symptom | Likely cause |
| --- | --- |
| `AzureCliCredential authentication failed` | Not signed in. Run `az login`. |
| `401` or `403` | Signed in to the wrong tenant, or your account lacks the *Azure AI User* role on the project. |
| `404` or `DeploymentNotFound` | The deployment name is wrong. Use the deployment name, not the model name. |
| The settings message appears | A value in `appsettings.json` is empty, or you filled in the wrong project's file. |
| There is no **Web search** tool for the agent | The tool is not enabled for the project. Ask the instructor. |
| The agent answers with text around the JSON | Make the output rule in your instructions stricter, for example "Return only the JSON object, with no text before or after it." |

## Output of this session

- A successful model call from C# using your own sign-in
- The project endpoint and deployment name in the smoke test's `appsettings.json`
- A completed comparison table
- An agent in Foundry that researches an airline's delay compensation and returns JSON, and its name written down for Session 6
