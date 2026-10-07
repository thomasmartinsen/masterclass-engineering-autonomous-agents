using System.Text.Json;
using Microsoft.Extensions.AI;

namespace CaseHandling.Agent;

public sealed record ToolCall(string Name, string Arguments, string Result);

/// <summary>Wraps a tool so every call, with its arguments and result, is printed and recorded.</summary>
public sealed class LoggingAIFunction(AIFunction inner, TextWriter log, IList<ToolCall> calls) : DelegatingAIFunction(inner)
{
    protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var args = JsonSerializer.Serialize(arguments.ToDictionary(a => a.Key, a => a.Value));
        log.WriteLine($"[tool] {Name} {args}");

        var result = await base.InvokeCoreAsync(arguments, cancellationToken);
        var text = result is JsonElement json ? json.GetRawText() : result?.ToString() ?? "";
        log.WriteLine($"[tool] {Name} -> {Shorten(text)}");

        calls.Add(new(Name, args, text));
        return result;
    }

    private static string Shorten(string text) =>
        text.Length <= 200 ? text : text[..200] + $"... ({text.Length} characters)";
}
