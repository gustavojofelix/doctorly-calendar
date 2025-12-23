using Calendar.Domain.Events;

namespace Calendar.Application.Abstractions;

public interface INotificationService
{
  Task EventCreatedAsync(CalendarEvent ev, CancellationToken ct);
  Task EventUpdatedAsync(CalendarEvent ev, CancellationToken ct);
  Task EventCancelledAsync(CalendarEvent ev, CancellationToken ct);
}
