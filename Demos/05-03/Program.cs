using System.ComponentModel;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("Set FOUNDRY_PROJECT_ENDPOINT.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
    ?? throw new InvalidOperationException("Set FOUNDRY_MODEL.");

var cases = new WorkshopCases();
AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(
        model: model,
        name: "CaseToolChain",
        instructions: """
            For every request about a customer's case, first call FindCases to resolve the case ID.
            Then call GetCase with that ID to read the recommended priority.
            Call SetCasePriority only when the user explicitly asks for a change, using the
            case ID from FindCases and the recommended priority returned by GetCase.
            Never invent a case ID or priority. These tools use in-memory workshop data only.
            """,
        tools:
        [
            AIFunctionFactory.Create(cases.FindCases),
            AIFunctionFactory.Create(cases.GetCase),
            AIFunctionFactory.Create(cases.SetCasePriority),
        ]);

Console.WriteLine("Question 1: What is the recommended priority for Ben's open case?");
AgentSession readSession = await agent.CreateSessionAsync();
AgentResponse readResponse = await agent.RunAsync(
    "What is the recommended priority for Ben's open case?", readSession);
Console.WriteLine(readResponse);
TokenUsageReporter.Write(readResponse);
var readCalls = cases.TakeCalls();
RequireInOrder(readCalls, "FindCases", "GetCase");
if (readCalls.Contains("SetCasePriority"))
    throw new InvalidDataException("The read-only question unexpectedly changed a case.");

Console.WriteLine("\nQuestion 2: Set Ben's case priority to its recommended priority.");
AgentSession changeSession = await agent.CreateSessionAsync();
AgentResponse changeResponse = await agent.RunAsync(
    "Set Ben's open case priority to its recommended priority.", changeSession);
Console.WriteLine(changeResponse);
TokenUsageReporter.Write(changeResponse);
var changeCalls = cases.TakeCalls();
RequireInOrder(changeCalls, "FindCases", "GetCase", "SetCasePriority");
if (cases.CurrentPriority != "High")
    throw new InvalidDataException("The expected in-memory priority change did not occur.");
Console.WriteLine($"Verified final priority for C-100: {cases.CurrentPriority}");

static void RequireInOrder(IReadOnlyList<string> calls, params string[] expected)
{
    int next = 0;
    foreach (string name in calls)
        if (next < expected.Length && name == expected[next]) next++;
    if (next != expected.Length)
        throw new InvalidDataException($"Expected tool chain {string.Join(" -> ", expected)}; saw {string.Join(" -> ", calls)}.");
}

public sealed class WorkshopCases
{
    private readonly List<string> _calls = [];
    private string? _resolvedCaseId;
    private bool _caseRead;
    public string CurrentPriority { get; private set; } = "Normal";

    public string[] TakeCalls()
    {
        string[] calls = [.. _calls];
        _calls.Clear();
        _resolvedCaseId = null;
        _caseRead = false;
        return calls;
    }

    [Description("Find open workshop cases for a customer name. Call this before GetCase to obtain the case ID.")]
    public string FindCases([Description("Customer name, such as Ben.")] string customerName)
    {
        _calls.Add(nameof(FindCases));
        Console.WriteLine($"- Tool Call: '{nameof(FindCases)}' (Args: customerName = {customerName})");
        _resolvedCaseId = string.Equals(customerName, "Ben", StringComparison.OrdinalIgnoreCase)
            ? "C-100" : null;
        _caseRead = false;
        return _resolvedCaseId is null
            ? "No open cases found for that customer."
            : "Ben has one open case: C-100.";
    }

    [Description("Read a workshop case by the ID returned by FindCases. Includes the recommended priority.")]
    public string GetCase([Description("Case ID returned by FindCases, such as C-100.")] string caseId)
    {
        _calls.Add(nameof(GetCase));
        Console.WriteLine($"- Tool Call: '{nameof(GetCase)}' (Args: caseId = {caseId})");
        if (_resolvedCaseId is null || caseId != _resolvedCaseId)
            throw new InvalidOperationException("Resolve the case ID with FindCases first.");
        _caseRead = true;
        return $"C-100: Ben's laptop will not start. CurrentPriority={CurrentPriority}; RecommendedPriority=High.";
    }

    [Description("Change a workshop case's priority in memory. Only use when the user explicitly requests a change.")]
    public string SetCasePriority(
        [Description("Case ID returned by FindCases.")] string caseId,
        [Description("Recommended priority returned by GetCase.")] string priority)
    {
        _calls.Add(nameof(SetCasePriority));
        Console.WriteLine($"- Tool Call: '{nameof(SetCasePriority)}' (Args: caseId = {caseId}, priority = {priority})");
        if (_resolvedCaseId is null || caseId != _resolvedCaseId || !_caseRead)
            throw new InvalidOperationException("Call FindCases and GetCase before changing priority.");
        if (!string.Equals(priority, "High", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Priority does not match the case recommendation.", nameof(priority));
        CurrentPriority = "High";
        return $"Case {caseId} now has priority {CurrentPriority}.";
    }
}
