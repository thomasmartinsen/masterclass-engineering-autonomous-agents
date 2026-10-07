using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CaseHandling.Agent;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CaseHandling.Tests;

public sealed class CaseToolsTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public CaseToolsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    public async ValueTask InitializeAsync() => await _client.PostAsync("/admin/reset", null);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Get_case_returns_case_json()
    {
        var tools = new CaseTools(_client);

        var result = await tools.GetCase("C-1002", TestContext.Current.CancellationToken);

        using var json = JsonDocument.Parse(result);
        Assert.Equal("C-1002", json.RootElement.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Unknown_case_returns_not_found_message()
    {
        var tools = new CaseTools(_client);

        var result = await tools.GetCase("C-9999", TestContext.Current.CancellationToken);

        Assert.StartsWith("Error:", result);
        Assert.Contains("no case with id 'C-9999'", result);
    }

    [Fact]
    public async Task Unavailable_case_api_returns_unavailable_message()
    {
        var ct = TestContext.Current.CancellationToken;
        await _client.PostAsJsonAsync("/admin/faults", new { unavailable = true }, ct);
        var tools = new CaseTools(_client);

        var result = await tools.GetCase("C-1001", ct);

        Assert.Equal(CaseTools.UnavailableMessage, result);
    }

    [Fact]
    public async Task Unreachable_case_api_returns_unavailable_message()
    {
        using var http = new HttpClient(new ThrowingHandler()) { BaseAddress = new Uri("http://localhost:1/") };
        var tools = new CaseTools(http);

        var result = await tools.GetCase("C-1001", TestContext.Current.CancellationToken);

        Assert.Equal(CaseTools.UnavailableMessage, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1001")]
    [InlineData("C-1001/../../admin/reset")]
    [InlineData("C-1001?status=Approved")]
    public async Task Invalid_case_id_is_rejected_without_calling_the_api(string caseId)
    {
        var handler = new ThrowingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:1/") };
        var tools = new CaseTools(http);

        var result = await tools.GetCase(caseId, TestContext.Current.CancellationToken);

        Assert.StartsWith("Error:", result);
        Assert.Contains("not a valid case id", result);
        Assert.Equal(0, handler.Calls);
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            throw new HttpRequestException("Connection refused.", null, HttpStatusCode.ServiceUnavailable);
        }
    }
}
