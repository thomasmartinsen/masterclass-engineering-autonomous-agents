using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CaseHandling.Agent;

public static class Tracing
{
    public const string SourceName = "CaseHandling.Agent";
    public const string ConnectionStringKey = "ApplicationInsights:ConnectionString";
    public const string IncludeSensitiveDataKey = "Tracing:IncludeSensitiveData";

    public static readonly ActivitySource Source = new(SourceName);

    /// <summary>Exports agent runs, model calls, tool calls, approval, and writes. Returns null, and traces nothing, without a connection string.</summary>
    public static TracerProvider? Start(IConfiguration configuration)
    {
        if (configuration[ConnectionStringKey] is not { Length: > 0 } connectionString)
        {
            return null;
        }

        return Sdk.CreateTracerProviderBuilder()
            .ConfigureResource(r => r.AddService(SourceName))
            .AddSource(SourceName)
            .AddAzureMonitorTraceExporter(o => o.ConnectionString = connectionString)
            .Build();
    }
}
