using CaseHandling.Agent;

namespace CaseHandling.Tests;

public sealed class AgentInstructionsTests
{
    [Fact]
    public void Instructions_file_loads_from_output()
    {
        var instructions = AgentInstructions.Load(AppContext.BaseDirectory);

        Assert.False(string.IsNullOrWhiteSpace(instructions));
    }

    [Fact]
    public void Instructions_forbid_decision_statuses()
    {
        var instructions = AgentInstructions.Load(AppContext.BaseDirectory);

        Assert.Contains("Never propose `Approved` or `Rejected`", instructions);
    }

    [Fact]
    public void Missing_instructions_file_is_reported()
    {
        var directory = Directory.CreateTempSubdirectory().FullName;

        var ex = Assert.Throws<FileNotFoundException>(() => AgentInstructions.Load(directory));

        Assert.Contains(AgentInstructions.FileName, ex.Message);
    }
}
