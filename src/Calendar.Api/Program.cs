using Calendar.Application.DTOs;
using Calendar.Application.Services;
using Calendar.Domain.Common;
using Calendar.Domain.Events;
using Calendar.Infrastructure;
using Calendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("CalendarDb") ?? "Data Source=calendar.db";
builder.Services.AddInfrastructure(cs);

var app = builder.Build();

// Auto-migrate on startup (acceptable for a tech test; document in README)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var group = app.MapGroup("/api/events").WithTags("Events");

// Create
group.MapPost(
    "/",
    async (CreateEventRequest req, EventService svc, CancellationToken ct) =>
    {
        try
        {
            var dto = await svc.CreateAsync(req, ct);
            return Results.Created($"/api/events/{dto.Id}", dto);
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
);

// Get by id
group.MapGet(
    "/{id:guid}",
    async (Guid id, EventService svc, CancellationToken ct) =>
    {
        try
        {
            var dto = await svc.GetAsync(id, ct);
            return Results.Ok(dto);
        }
        catch (DomainException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
);

// Update
group.MapPut(
    "/{id:guid}",
    async (
        Guid id,
        UpdateEventRequest req,
        EventService svc,
        AppDbContext db,
        CancellationToken ct
    ) =>
    {
        // Concurrency: set original RowVersion expected by client
        var tracked = await db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (tracked is null)
            return Results.NotFound(new { error = "Event not found." });

        db.Entry(tracked).Property(e => e.RowVersion).OriginalValue = req.RowVersion;

        try
        {
            var dto = await svc.UpdateAsync(id, req, ct);
            return Results.Ok(dto);
        }
        catch (DomainException ex)
        {
            // BadRequest used for domain + concurrency messaging (simple for tech test)
            return Results.BadRequest(new { error = ex.Message });
        }
    }
);

// Cancel (soft)
group.MapDelete(
    "/{id:guid}",
    async (Guid id, byte[] rowVersion, EventService svc, AppDbContext db, CancellationToken ct) =>
    {
        var tracked = await db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (tracked is null)
            return Results.NotFound(new { error = "Event not found." });

        db.Entry(tracked).Property(e => e.RowVersion).OriginalValue = rowVersion;

        try
        {
            await svc.CancelAsync(id, rowVersion, ct);
            return Results.NoContent();
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
);

// List with filters
group.MapGet(
    "/",
    async (
        DateTimeOffset? from,
        DateTimeOffset? to,
        EventStatus? status,
        EventService svc,
        CancellationToken ct
    ) =>
    {
        var items = await svc.ListAsync(from, to, status, ct);
        return Results.Ok(items);
    }
);

// Search
group.MapGet(
    "/search",
    async (string q, EventService svc, CancellationToken ct) =>
    {
        if (string.IsNullOrWhiteSpace(q))
            return Results.BadRequest(new { error = "q is required." });
        var items = await svc.SearchAsync(q, ct);
        return Results.Ok(items);
    }
);

// Attendee response (nice-to-have)
group.MapPost(
    "/{eventId:guid}/attendees/{attendeeId:guid}/response",
    async (
        Guid eventId,
        Guid attendeeId,
        SetAttendeeResponseRequest req,
        AppDbContext db,
        EventService svc,
        CancellationToken ct
    ) =>
    {
        var tracked = await db.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (tracked is null)
            return Results.NotFound(new { error = "Event not found." });

        db.Entry(tracked).Property(e => e.RowVersion).OriginalValue = req.RowVersion;

        try
        {
            await svc.SetAttendeeResponseAsync(eventId, attendeeId, req, ct);
            return Results.NoContent();
        }
        catch (DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
);

app.Run();

// Needed for WebApplicationFactory in tests
public partial class Program { }
