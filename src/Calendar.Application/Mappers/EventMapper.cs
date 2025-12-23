using Calendar.Application.DTOs;
using Calendar.Domain.Events;

namespace Calendar.Application.Mappers;

public static class EventMapper
{
  public static EventDto ToDto(CalendarEvent ev) =>
      new(
          ev.Id,
          ev.Title,
          ev.Description,
          ev.StartTime,
          ev.EndTime,
          ev.Status,
          ev.RowVersion,
          ev.Attendees.Select(a => new AttendeeDto(a.Id, a.Name, a.Email, a.Status)).ToList()
      );
}
