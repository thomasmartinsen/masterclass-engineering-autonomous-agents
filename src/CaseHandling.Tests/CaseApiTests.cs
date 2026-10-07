using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CaseHandling.Domain;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CaseHandling.Tests;

public sealed class CaseApiTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    public CaseApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    public async ValueTask InitializeAsync() => await _client.PostAsync("/admin/reset", null);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Get_case_returns_seeded_case()
    {
        var found = await _client.GetFromJsonAsync<Case>("/cases/C-1001", _json, TestContext.Current.CancellationToken);

        Assert.NotNull(found);
        Assert.Equal(CaseCategory.WaterDamage, found.Category);
    }

    [Fact]
    public async Task Get_unknown_case_returns_404()
    {
        var response = await _client.GetAsync("/cases/C-9999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Validation_reports_missing_incident_date_and_claimed_amount()
    {
        var issues = await _client.GetFromJsonAsync<ValidationIssue[]>("/cases/C-1002/validation", _json, TestContext.Current.CancellationToken);

        Assert.NotNull(issues);
        Assert.Contains(issues, i => i.Field == nameof(Case.IncidentDate));
        Assert.Contains(issues, i => i.Field == nameof(Case.ClaimedAmount));
    }

    [Fact]
    public async Task Validation_reports_incident_date_after_reported_date()
    {
        var issues = await _client.GetFromJsonAsync<ValidationIssue[]>("/cases/C-1007/validation", _json, TestContext.Current.CancellationToken);

        Assert.NotNull(issues);
        Assert.Contains(issues, i => i.Field == nameof(Case.IncidentDate));
    }

    [Fact]
    public async Task Validation_of_complete_case_returns_no_issues()
    {
        var issues = await _client.GetFromJsonAsync<ValidationIssue[]>("/cases/C-1001/validation", _json, TestContext.Current.CancellationToken);

        Assert.NotNull(issues);
        Assert.Empty(issues);
    }

    [Fact]
    public async Task Disallowed_status_change_returns_409()
    {
        var response = await _client.PutAsJsonAsync(
            "/cases/C-1006/status",
            new { status = "UnderReview", reason = "Reopen" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Writes_are_recorded_and_cleared_by_reset()
    {
        var ct = TestContext.Current.CancellationToken;
        await _client.PostAsJsonAsync("/cases/C-1001/notes", new { author = "test", text = "hello" }, ct);

        var changes = await _client.GetFromJsonAsync<JsonElement[]>("/admin/changes", ct);
        Assert.Single(changes!);

        await _client.PostAsync("/admin/reset", null, ct);
        changes = await _client.GetFromJsonAsync<JsonElement[]>("/admin/changes", ct);
        Assert.Empty(changes!);
    }

    [Fact]
    public async Task Fault_mode_makes_cases_unavailable()
    {
        var ct = TestContext.Current.CancellationToken;
        await _client.PostAsJsonAsync("/admin/faults", new { unavailable = true }, ct);

        var response = await _client.GetAsync("/cases/C-1001", ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Validate_all_cases_returns_one_result_per_case()
    {
        var ct = TestContext.Current.CancellationToken;

        var results = await _client.GetFromJsonAsync<JsonElement[]>("/cases/validation", ct);
        var cases = await _client.GetFromJsonAsync<Case[]>("/cases", _json, ct);

        Assert.Equal(cases!.Select(c => c.Id), results!.Select(r => r.GetProperty("caseId").GetString()));
        Assert.All(results!, r => Assert.Equal(JsonValueKind.Array, r.GetProperty("issues").ValueKind));
    }
}
