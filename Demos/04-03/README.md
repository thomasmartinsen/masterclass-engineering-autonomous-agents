# Demo 3: give the engineering agent a framework skill

Open this folder in VS Code. Review the [skill](.github/skills/agent-framework-csharp/SKILL.md), then ask the engineering agent to inspect the starter and plan the first case agent. Ask it to name the package versions, configuration variables, build command, and acceptance checks before it writes code. Run `dotnet build SkillStarter.csproj` to establish the baseline.

The skill is for the **engineering agent** that writes code. It is not a prompt loaded into the running case agent. Demo 4 contains a completed case-agent snapshot so the class can continue if live code generation takes longer than planned.
