using System.Collections.Concurrent;
using System.Diagnostics;
using CaseHandling.Agent;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CaseHandling.Tests;

public sealed class TracingTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public TracingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    public async ValueTask InitializeAsync() => await _client.PostAsync("/admin/reset", null);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public void Tracing_is_off_without_connection_string()
    {
        var configuration = new ConfigurationBuilder().Build();

        using var provider = Tracing.Start(configuration);

        Assert.Null(provider);
    }

    [Fact]
    public async Task Approval_and_write_are_traced()
    {
        var activities = new ConcurrentBag<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == Tracing.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activities.Add,
        };
        ActivitySource.AddActivityListener(listener);
        var proposal = new CaseAssessment { CaseId = "C-1001", ProposedStatus = ProposedStatus.UnderReview, Reasoning = "Complete." };
        var workflow = CaseWorkflow.Build((_, _) => Task.FromResult(proposal), new CaseStatusWriter(_client));

        await CaseWorkflow.RunAsync(workflow, "Assess case C-1001", new ApproveAll(), TestContext.Current.CancellationToken);

        var mine = activities.Where(a => Equals(a.GetTagItem("case.id"), "C-1001")).ToList();
        Assert.Contains(mine, a => a.OperationName == "approve" && Equals(a.GetTagItem("approval.approved"), true));
        Assert.Contains(mine, a => a.OperationName == "write_status" && Equals(a.GetTagItem("write.succeeded"), true));
        Assert.Contains(mine, a => a.OperationName == "case_workflow");
    }

    private sealed class ApproveAll : IApprover
    {
        public Task<bool> ApproveAsync(CaseAssessment assessment, CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
