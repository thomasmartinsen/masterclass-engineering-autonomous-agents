# Demo 1: an engineering-agent development loop

Run `dotnet run --project DevelopmentLoop.csproj -- C-100`. The starter has a small, inspectable C# case fixture and no cloud dependency.

Give the engineering agent this task: “Add a `NeedsClarification` property to the output. It must be true when contact details are missing. Handle an unknown case ID without a stack trace. Show the changed files and run the project for C-100, C-101, and C-999.” Review the proposed change, build it, run those three cases, and ask the agent to correct any failure. The point is the **goal → plan → code → verification → review** loop used throughout the day.
