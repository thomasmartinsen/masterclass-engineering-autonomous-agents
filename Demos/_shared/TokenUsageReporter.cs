using System.Globalization;
using Microsoft.Agents.AI;

internal static class TokenUsageReporter
{
    public static void Write(AgentResponse response)
    {
#if DEBUG
        var usage = response.Usage;
        if (usage is null)
        {
            Console.WriteLine("[debug] Token usage: unavailable from this model response.");
            return;
        }

        var input = usage.InputTokenCount;
        var output = usage.OutputTokenCount;
        var total = usage.TotalTokenCount ?? (input.HasValue && output.HasValue ? input + output : null);
        Console.WriteLine($"[debug] Token usage: In = {Format(input)} | Out = {Format(output)} | Total = {Format(total)}");

        if (input.HasValue && output.HasValue
            && ReadRate("FOUNDRY_INPUT_USD_PER_1M_TOKENS") is { } inputRate
            && ReadRate("FOUNDRY_OUTPUT_USD_PER_1M_TOKENS") is { } outputRate)
        {
            decimal estimate = (input.Value * inputRate + output.Value * outputRate) / 1_000_000m;
            Console.WriteLine($"[debug] Estimated model cost: ${estimate.ToString("G29", CultureInfo.InvariantCulture)} USD");
        }
        else
        {
            Console.WriteLine("[debug] Estimated model cost: unavailable; configure input and output USD rates per 1M tokens.");
        }
#endif
    }

#if DEBUG
    private static string Format(long? count) => count?.ToString(CultureInfo.InvariantCulture) ?? "unavailable";

    private static decimal? ReadRate(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal rate)
            && rate >= 0 ? rate : null;
    }
#endif
}
