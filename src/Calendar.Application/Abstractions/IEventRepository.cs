using Calendar.Domain.Events;

namespace Calendar.Application.Abstractions;

public interface IEventRepository
{
    Task<CalendarEvent?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(CalendarEvent ev, CancellationToken ct);
    Task DeleteAsync(CalendarEvent ev, CancellationToken ct);
    Task<List<CalendarEvent>> ListAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        EventStatus? status,
        CancellationToken ct
    );
    Task<List<CalendarEvent>> SearchAsync(string q, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
