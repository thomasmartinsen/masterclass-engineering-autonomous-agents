using System.Text.Json;

// Small deterministic baseline for the engineering-agent development loop.
var input = args.Length > 0 ? args[0] : "C-100";
var cases = new Dictionary<string, CaseRecord>(StringComparer.OrdinalIgnoreCase)
{
    ["C-100"] = new("C-100", "Laptop will not start", "Customer", true),
    ["C-101"] = new("C-101", "Access request", "Employee", false),
};

if (!cases.TryGetValue(input, out var record))
{
    Console.Error.WriteLine($"Unknown case: {input}");
    return 1;
}

Console.WriteLine(JsonSerializer.Serialize(new
{
    record.Id,
    record.Title,
    record.RequesterType,
    record.HasContactDetails,
}));
return 0;

internal sealed record CaseRecord(string Id, string Title, string RequesterType, bool HasContactDetails);
