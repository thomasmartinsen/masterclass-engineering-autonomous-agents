using System.Collections.Concurrent;
using System.Text.Json;
using CaseHandling.Domain;

namespace CaseHandling.CaseApi;

public sealed record CaseChange(DateTimeOffset At, string CaseId, string Kind, string Detail);

/// <summary>In-memory case store seeded from JSON test data. Reset restores the seed.</summary>
public sealed class CaseStore
{
    private readonly string _seedPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConcurrentDictionary<string, Case> _cases = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<CaseChange> _changes = new();

    public CaseStore(string seedPath, JsonSerializerOptions jsonOptions)
    {
        _seedPath = seedPath;
        _jsonOptions = jsonOptions;
        Reset();
    }

    public IReadOnlyList<Case> GetAll() => _cases.Values.OrderBy(c => c.Id).ToList();

    public Case? Get(string id) => _cases.GetValueOrDefault(id);

    public IReadOnlyList<CaseChange> Changes => _changes.ToList();

    public Case AddNote(string id, string author, string text)
    {
        var updated = Update(id, c => c with
        {
            Notes = [.. c.Notes, new CaseNote(DateTimeOffset.UtcNow, author, text)],
        });
        _changes.Enqueue(new(DateTimeOffset.UtcNow, id, "note", $"{author}: {text}"));
        return updated;
    }

    public Case SetStatus(string id, CaseStatus status, string reason)
    {
        var previous = Get(id)?.Status;
        var updated = Update(id, c => c with { Status = status });
        _changes.Enqueue(new(DateTimeOffset.UtcNow, id, "status", $"{previous} -> {status}: {reason}"));
        return updated;
    }

    public void Reset()
    {
        var seed = JsonSerializer.Deserialize<List<Case>>(File.ReadAllText(_seedPath), _jsonOptions)
            ?? throw new InvalidOperationException($"No cases found in '{_seedPath}'.");

        _cases.Clear();
        foreach (var c in seed)
        {
            _cases[c.Id] = c;
        }

        _changes.Clear();
    }

    private Case Update(string id, Func<Case, Case> change) =>
        _cases.AddOrUpdate(id, _ => throw new KeyNotFoundException(id), (_, existing) => change(existing));
}
