using System.Text.Json;
using System.Text.Json.Serialization;
using CaseHandling.CaseApi;
using CaseHandling.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<FaultState>();
builder.Services.AddSingleton(sp =>
{
    var seedPath = builder.Configuration["CaseData:Path"]
        ?? Path.Combine(AppContext.BaseDirectory, "data", "cases", "cases.json");
    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    jsonOptions.Converters.Add(new JsonStringEnumConverter());
    return new CaseStore(seedPath, jsonOptions);
});

var app = builder.Build();

app.UseExceptionHandler();

// Simulates an unavailable dependency for reliability exercises; /admin stays reachable.
app.Use(async (context, next) =>
{
    var faults = context.RequestServices.GetRequiredService<FaultState>();
    if (faults.Unavailable && !context.Request.Path.StartsWithSegments("/admin"))
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new { title = "Case API is temporarily unavailable." });
        return;
    }

    await next();
});

var cases = app.MapGroup("/cases");

cases.MapGet("/", (CaseStore store, CaseStatus? status) =>
    store.GetAll().Where(c => status is null || c.Status == status));

cases.MapGet("/{id}", (string id, CaseStore store) =>
    store.Get(id) is { } found ? Results.Ok(found) : Results.NotFound());

cases.MapGet("/validation", (CaseStore store) =>
    store.GetAll().Select(c => new CaseValidationResult(c.Id, CaseValidator.Validate(c))));

cases.MapGet("/{id}/validation", (string id, CaseStore store) =>
    store.Get(id) is { } found ? Results.Ok(CaseValidator.Validate(found)) : Results.NotFound());

cases.MapPost("/{id}/notes", (string id, AddNoteRequest request, CaseStore store) =>
{
    if (store.Get(id) is null)
    {
        return Results.NotFound();
    }

    if (string.IsNullOrWhiteSpace(request.Author) || string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest("Author and text are required.");
    }

    return Results.Ok(store.AddNote(id, request.Author, request.Text));
});

cases.MapPut("/{id}/status", (string id, ChangeStatusRequest request, CaseStore store) =>
{
    if (store.Get(id) is not { } existing)
    {
        return Results.NotFound();
    }

    if (!CaseStatusTransitions.IsAllowed(existing.Status, request.Status))
    {
        return Results.Problem(
            title: "Status change not allowed.",
            detail: $"Cannot change status from {existing.Status} to {request.Status}.",
            statusCode: StatusCodes.Status409Conflict);
    }

    if (string.IsNullOrWhiteSpace(request.Reason))
    {
        return Results.Problem(
            title: "Reason is required.",
            statusCode: StatusCodes.Status400BadRequest);
    }

    return Results.Ok(store.SetStatus(id, request.Status, request.Reason));
});

var admin = app.MapGroup("/admin");

admin.MapGet("/changes", (CaseStore store) => store.Changes);

admin.MapPost("/reset", (CaseStore store, FaultState faults) =>
{
    store.Reset();
    faults.Unavailable = false;
    return Results.NoContent();
});

admin.MapPost("/faults", (FaultRequest request, FaultState faults) =>
{
    faults.Unavailable = request.Unavailable;
    return Results.Ok(new { faults.Unavailable });
});

app.Run();

public sealed record AddNoteRequest(string Author, string Text);

public sealed record ChangeStatusRequest(CaseStatus Status, string Reason);

public sealed record FaultRequest(bool Unavailable);

public sealed record CaseValidationResult(string CaseId, IReadOnlyList<ValidationIssue> Issues);

public sealed class FaultState
{
    public bool Unavailable { get; set; }
}

public partial class Program;
