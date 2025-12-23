using Calendar.Application.Abstractions;
using Calendar.Domain.Events;
using Calendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db) => _db = db;

    public Task<CalendarEvent?> GetAsync(Guid id, CancellationToken ct) =>
        _db.Events.Include(e => e.Attendees).FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task AddAsync(CalendarEvent ev, CancellationToken ct) =>
        _db.Events.AddAsync(ev, ct).AsTask();

    public Task DeleteAsync(CalendarEvent ev, CancellationToken ct)
    {
        _db.Events.Remove(ev);
        return Task.CompletedTask;
    }

    public async Task<List<CalendarEvent>> ListAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        EventStatus? status,
        CancellationToken ct
    )
    {
        var q = _db.Events.Include(e => e.Attendees).AsQueryable();

        if (from is not null)
            q = q.Where(e => e.EndTime >= from);
        if (to is not null)
            q = q.Where(e => e.StartTime <= to);
        if (status is not null)
            q = q.Where(e => e.Status == status);

        return await q.OrderBy(e => e.StartTime).ToListAsync(ct);
    }

    public async Task<List<CalendarEvent>> SearchAsync(string q, CancellationToken ct)
    {
        q = q.Trim();
        return await _db
            .Events.Include(e => e.Attendees)
            .Where(e =>
                e.Title.Contains(q)
                || (e.Description != null && e.Description.Contains(q))
                || e.Attendees.Any(a => a.Email.Contains(q) || a.Name.Contains(q))
            )
            .OrderBy(e => e.StartTime)
            .ToListAsync(ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
