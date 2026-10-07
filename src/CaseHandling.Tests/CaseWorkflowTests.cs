using System.Net.Http.Json;
using System.Text.Json;
using CaseHandling.Agent;
using CaseHandling.Domain;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CaseHandling.Tests;

public sealed class CaseWorkflowTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public CaseWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    public async ValueTask InitializeAsync() => await _client.PostAsync("/admin/reset", null);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static CaseAssessment Proposal(string? caseId = "C-1001") => new()
    {
        CaseId = caseId,
        ProposedStatus = ProposedStatus.UnderReview,
        PolicyIds = ["POL-WATER-002"],
        Reasoning = "All required information is present.",
    };

    [Fact]
    public async Task Approved_proposal_is_written_once()
    {
        var ct = TestContext.Current.CancellationToken;
        var workflow = CaseWorkflow.Build((_, _) => Task.FromResult(Proposal()), new CaseStatusWriter(_client));

        var outcome = await CaseWorkflow.RunAsync(workflow, "Assess case C-1001", new FixedApprover(true), ct);

        Assert.True(outcome.Write?.Succeeded);
        var changes = await _client.GetFromJsonAsync<JsonElement[]>("/admin/changes", ct);
        var change = Assert.Single(changes!);
        Assert.Equal("status", change.GetProperty("kind").GetString());
    }

    [Fact]
    public async Task Rejected_proposal_writes_nothing()
    {
        var ct = TestContext.Current.CancellationToken;
        var workflow = CaseWorkflow.Build((_, _) => Task.FromResult(Proposal()), new CaseStatusWriter(_client));

        var outcome = await CaseWorkflow.RunAsync(workflow, "Assess case C-1001", new FixedApprover(false), ct);

        Assert.False(outcome.Approved);
        Assert.Null(outcome.Write);
        Assert.Empty((await _client.GetFromJsonAsync<JsonElement[]>("/admin/changes", ct))!);
    }

    [Fact]
    public async Task Approval_is_asked_before_anything_is_written()
    {
        var ct = TestContext.Current.CancellationToken;
        var approver = new FixedApprover(false, onAsk: async () =>
            Assert.Empty((await _client.GetFromJsonAsync<JsonElement[]>("/admin/changes", ct))!));
        var workflow = CaseWorkflow.Build((_, _) => Task.FromResult(Proposal()), new CaseStatusWriter(_client));

        await CaseWorkflow.RunAsync(workflow, "Assess case C-1001", approver, ct);

        Assert.Equal(1, approver.Calls);
    }

    [Fact]
    public async Task Proposal_without_case_id_skips_approval_and_writes_nothing()
    {
        var ct = TestContext.Current.CancellationToken;
        var approver = new FixedApprover(true);
        var workflow = CaseWorkflow.Build((_, _) => Task.FromResult(Proposal(caseId: null)), new CaseStatusWriter(_client));

        var outcome = await CaseWorkflow.RunAsync(workflow, "My bike was stolen.", approver, ct);

        Assert.Null(outcome.Write);
        Assert.Equal(0, approver.Calls);
        Assert.Empty((await _client.GetFromJsonAsync<JsonElement[]>("/admin/changes", ct))!);
    }

    [Fact]
    public async Task Proposal_without_status_skips_approval_and_writes_nothing()
    {
        var ct = TestContext.Current.CancellationToken;
        var approver = new FixedApprover(true);
        var workflow = CaseWorkflow.Build((_, _) => Task.FromResult(Proposal() with { ProposedStatus = null }), new CaseStatusWriter(_client));

        var outcome = await CaseWorkflow.RunAsync(workflow, "Assess case C-1001", approver, ct);

        Assert.Null(outcome.Write);
        Assert.Equal(0, approver.Calls);
        Assert.Empty((await _client.GetFromJsonAsync<JsonElement[]>("/admin/changes", ct))!);
    }

    [Theory]
    [InlineData(CaseStatus.Approved)]
    [InlineData(CaseStatus.Rejected)]
    public async Task Decision_status_is_never_written(CaseStatus status)
    {
        var ct = TestContext.Current.CancellationToken;
        var writer = new CaseStatusWriter(_client);

        var result = await writer.SetStatusAsync("C-1004", status, "Agent proposal", ct);

        Assert.False(result.Succeeded);
        Assert.Empty((await _client.GetFromJsonAsync<JsonElement[]>("/admin/changes", ct))!);
    }

    private sealed class FixedApprover(bool approve, Func<Task>? onAsk = null) : IApprover
    {
        public int Calls { get; private set; }

        public async Task<bool> ApproveAsync(CaseAssessment assessment, CancellationToken cancellationToken)
        {
            Calls++;
            if (onAsk is not null)
            {
                await onAsk();
            }

            return approve;
        }
    }
}
