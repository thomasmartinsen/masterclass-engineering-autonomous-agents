# Demo 1: customize an engineering agent

Open this folder in VS Code. It contains a [custom agent](.github/agents/case-engineer.agent.md) and a [skill](.github/skills/case-intake/SKILL.md). Run `dotnet run --project Customization.csproj`, then ask the custom agent to replace the placeholder output with a typed C# intake decision and to verify it against two inputs. Discuss which information belongs in repository knowledge, a reusable skill, or a specialist agent. The skill here supports **software engineering**; the case agent built later has separate runtime instructions.
