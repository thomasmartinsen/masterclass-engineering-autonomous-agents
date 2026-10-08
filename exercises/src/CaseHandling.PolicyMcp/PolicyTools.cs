using System.ComponentModel;
using ModelContextProtocol.Server;

namespace CaseHandling.PolicyMcp;

[McpServerToolType]
public sealed class PolicyTools(PolicyRepository repository)
{
    [McpServerTool(Name = "list_policies", ReadOnly = true)]
    [Description("Lists all claims-handling policies with their id, title, and category.")]
    public IEnumerable<object> ListPolicies() =>
        repository.All.Select(p => new { p.Id, p.Title, p.Category });

    [McpServerTool(Name = "get_policy", ReadOnly = true)]
    [Description("Returns the full text of a policy by its id, for example POL-WATER-002.")]
    public object GetPolicy([Description("The policy id.")] string policyId) =>
        repository.Get(policyId) is { } policy
            ? policy
            : new { Error = $"No policy with id '{policyId}'. Use list_policies to see valid ids." };

    [McpServerTool(Name = "search_policies", ReadOnly = true)]
    [Description("Finds policies that mention any of the given search terms or match a case category such as WaterDamage, Theft, TravelDelay, or Fire.")]
    public IEnumerable<object> SearchPolicies([Description("Space-separated search terms.")] string query) =>
        repository.Search(query).Select(p => new { p.Id, p.Title, p.Category });
}
