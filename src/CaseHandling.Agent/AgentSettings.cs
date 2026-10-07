using Microsoft.Extensions.Configuration;

namespace CaseHandling.Agent;

public sealed record AgentSettings(string ProjectEndpoint, string ModelDeployment)
{
    public const string ProjectEndpointKey = "Foundry:ProjectEndpoint";
    public const string ModelDeploymentKey = "Foundry:ModelDeployment";

    public static AgentSettings FromConfiguration(IConfiguration configuration)
    {
        return new AgentSettings(Required(configuration, ProjectEndpointKey), Required(configuration, ModelDeploymentKey));
    }

    private static string Required(IConfiguration configuration, string key)
    {
        if (configuration[key] is not { } value || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing configuration '{key}'. Set it in src/CaseHandling.Agent/appsettings.json.");
        }

        return value;
    }
}
