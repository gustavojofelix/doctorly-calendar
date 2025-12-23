using Calendar.Domain.Events;

namespace Calendar.Application.DTOs;

public sealed record AttendeeDto(Guid Id, string Name, string Email, AttendanceStatus Status);

public sealed record EventDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    EventStatus Status,
    byte[] RowVersion,
    List<AttendeeDto> Attendees
);

public sealed record CreateEventRequest(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    List<CreateAttendeeRequest>? Attendees
);

public sealed record CreateAttendeeRequest(string Name, string Email);

public sealed record UpdateEventRequest(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    byte[] RowVersion
);

public sealed record SetAttendeeResponseRequest(AttendanceStatus Status, byte[] RowVersion);
