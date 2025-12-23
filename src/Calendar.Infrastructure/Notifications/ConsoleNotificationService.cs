using Calendar.Application.Abstractions;
using Calendar.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Calendar.Infrastructure.Notifications;

public sealed class ConsoleNotificationService : INotificationService
{
    private readonly ILogger<ConsoleNotificationService> _logger;

    public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger) =>
        _logger = logger;

    public Task EventCreatedAsync(CalendarEvent ev, CancellationToken ct)
    {
        _logger.LogInformation("Notification: EventCreated {EventId} {Title}", ev.Id, ev.Title);
        return Task.CompletedTask;
    }

    public Task EventUpdatedAsync(CalendarEvent ev, CancellationToken ct)
    {
        _logger.LogInformation("Notification: EventUpdated {EventId} {Title}", ev.Id, ev.Title);
        return Task.CompletedTask;
    }

    public Task EventCancelledAsync(CalendarEvent ev, CancellationToken ct)
    {
        _logger.LogInformation("Notification: EventCancelled {EventId} {Title}", ev.Id, ev.Title);
        return Task.CompletedTask;
    }
}
