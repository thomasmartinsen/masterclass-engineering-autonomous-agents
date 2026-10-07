namespace CaseHandling.Agent;

public static class AgentInstructions
{
    public const string FileName = "instructions.md";

    public static string Load(string directory)
    {
        var path = Path.Combine(directory, FileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Agent instructions file '{FileName}' was not found in '{directory}'.", path);
        }

        var text = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException($"Agent instructions file '{path}' is empty.");
        }

        return text;
    }
}
