using CaseHandling.Agent;
using Microsoft.Extensions.Configuration;

namespace CaseHandling.Tests;

public sealed class AgentSettingsTests
{
    [Fact]
    public void Complete_configuration_is_read()
    {
        var configuration = Configuration(endpoint: "https://example.test", deployment: "model");

        var settings = AgentSettings.FromConfiguration(configuration);

        Assert.Equal(new AgentSettings("https://example.test", "model"), settings);
    }

    [Fact]
    public void Missing_endpoint_is_reported()
    {
        var configuration = Configuration(endpoint: "", deployment: "model");

        var ex = Assert.Throws<InvalidOperationException>(() => AgentSettings.FromConfiguration(configuration));

        Assert.Contains(AgentSettings.ProjectEndpointKey, ex.Message);
    }

    [Fact]
    public void Missing_deployment_is_reported()
    {
        var configuration = Configuration(endpoint: "https://example.test", deployment: null);

        var ex = Assert.Throws<InvalidOperationException>(() => AgentSettings.FromConfiguration(configuration));

        Assert.Contains(AgentSettings.ModelDeploymentKey, ex.Message);
    }

    private static IConfiguration Configuration(string? endpoint, string? deployment) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [AgentSettings.ProjectEndpointKey] = endpoint,
                [AgentSettings.ModelDeploymentKey] = deployment,
            })
            .Build();
}
