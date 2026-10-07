namespace CaseHandling.PolicyMcp;

public sealed record PolicyDocument(string Id, string Title, string Category, string Content);

/// <summary>Loads policy documents with a simple front matter block (id, title, category).</summary>
public sealed class PolicyRepository
{
    private readonly IReadOnlyList<PolicyDocument> _policies;

    public PolicyRepository(string directory)
    {
        _policies = Directory.EnumerateFiles(directory, "*.md")
            .Select(Parse)
            .OrderBy(p => p.Id)
            .ToList();
    }

    public IReadOnlyList<PolicyDocument> All => _policies;

    public PolicyDocument? Get(string id) =>
        _policies.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<PolicyDocument> Search(string query)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return _policies
            .Select(p => (Policy: p, Hits: terms.Count(t =>
                p.Content.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(t, StringComparison.OrdinalIgnoreCase))))
            .Where(x => x.Hits > 0)
            .OrderByDescending(x => x.Hits)
            .Select(x => x.Policy)
            .ToList();
    }

    private static PolicyDocument Parse(string path)
    {
        var lines = File.ReadAllLines(path);
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var bodyStart = 0;

        if (lines.Length > 0 && lines[0].Trim() == "---")
        {
            var end = Array.FindIndex(lines, 1, l => l.Trim() == "---");
            foreach (var line in lines[1..end])
            {
                var separator = line.IndexOf(':');
                if (separator > 0)
                {
                    metadata[line[..separator].Trim()] = line[(separator + 1)..].Trim();
                }
            }

            bodyStart = end + 1;
        }

        var name = Path.GetFileNameWithoutExtension(path);
        return new PolicyDocument(
            metadata.GetValueOrDefault("id", name),
            metadata.GetValueOrDefault("title", name),
            metadata.GetValueOrDefault("category", "General"),
            string.Join('\n', lines[bodyStart..]).Trim());
    }
}
