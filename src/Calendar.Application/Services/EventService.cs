using Calendar.Application.Abstractions;
using Calendar.Application.DTOs;
using Calendar.Application.Mappers;
using Calendar.Domain.Common;
using Calendar.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Application.Services;

public sealed class EventService
{
    private readonly IEventRepository _repo;
    private readonly INotificationService _notifications;

    public EventService(IEventRepository repo, INotificationService notifications)
    {
        _repo = repo;
        _notifications = notifications;
    }

    public async Task<EventDto> CreateAsync(CreateEventRequest req, CancellationToken ct)
    {
        var attendees = (req.Attendees ?? new()).Select(a => new Attendee(a.Name, a.Email));
        var ev = new CalendarEvent(
            req.Title,
            req.Description,
            req.StartTime,
            req.EndTime,
            attendees
        );

        await _repo.AddAsync(ev, ct);
        await _repo.SaveChangesAsync(ct);

        await _notifications.EventCreatedAsync(ev, ct);
        return EventMapper.ToDto(ev);
    }

    public async Task<EventDto> UpdateAsync(Guid id, UpdateEventRequest req, CancellationToken ct)
    {
        var ev = await _repo.GetAsync(id, ct) ?? throw new DomainException("Event not found.");

        // optimistic concurrency: caller must provide RowVersion they last read
        ev.Update(req.Title, req.Description, req.StartTime, req.EndTime);

        try
        {
            // repo will have original RowVersion tracked; set expected
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException(
                "Concurrency conflict: the event was updated by someone else. Please reload and retry."
            );
        }

        await _notifications.EventUpdatedAsync(ev, ct);
        return EventMapper.ToDto(ev);
    }

    public async Task CancelAsync(Guid id, byte[] rowVersion, CancellationToken ct)
    {
        var ev = await _repo.GetAsync(id, ct) ?? throw new DomainException("Event not found.");
        ev.Cancel();

        try
        {
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException(
                "Concurrency conflict: the event was updated by someone else. Please reload and retry."
            );
        }

        await _notifications.EventCancelledAsync(ev, ct);
    }

    public async Task<List<EventDto>> ListAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        EventStatus? status,
        CancellationToken ct
    )
    {
        var items = await _repo.ListAsync(from, to, status, ct);
        return items.Select(EventMapper.ToDto).ToList();
    }

    public async Task<List<EventDto>> SearchAsync(string q, CancellationToken ct)
    {
        var items = await _repo.SearchAsync(q, ct);
        return items.Select(EventMapper.ToDto).ToList();
    }

    public async Task<EventDto> GetAsync(Guid id, CancellationToken ct)
    {
        var ev = await _repo.GetAsync(id, ct) ?? throw new DomainException("Event not found.");
        return EventMapper.ToDto(ev);
    }

    public async Task SetAttendeeResponseAsync(
        Guid eventId,
        Guid attendeeId,
        SetAttendeeResponseRequest req,
        CancellationToken ct
    )
    {
        var ev = await _repo.GetAsync(eventId, ct) ?? throw new DomainException("Event not found.");
        ev.SetAttendeeResponse(attendeeId, req.Status);

        try
        {
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainException(
                "Concurrency conflict: the event was updated by someone else. Please reload and retry."
            );
        }
    }
}
