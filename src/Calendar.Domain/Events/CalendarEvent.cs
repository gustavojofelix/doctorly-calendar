using Calendar.Domain.Common;

namespace Calendar.Domain.Events;

public sealed class CalendarEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }

    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }

    public EventStatus Status { get; private set; } = EventStatus.Active;

    // Concurrency token (EF will map RowVersion as a timestamp/rowversion)
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private readonly List<Attendee> _attendees = new();
    public IReadOnlyCollection<Attendee> Attendees => _attendees;

    private CalendarEvent() { } // EF

    public CalendarEvent(
        string title,
        string? description,
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<Attendee>? attendees = null
    )
    {
        SetCoreFields(title, description, start, end);

        if (attendees is not null)
        {
            foreach (var a in attendees)
                AddAttendee(a);
        }
    }

    public void Update(string title, string? description, DateTimeOffset start, DateTimeOffset end)
    {
        EnsureActive();
        SetCoreFields(title, description, start, end);
    }

    public void Cancel()
    {
        EnsureActive();
        Status = EventStatus.Cancelled;
    }

    public void AddAttendee(Attendee attendee)
    {
        EnsureActive();
        if (_attendees.Any(a => a.Email.Equals(attendee.Email, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Attendee email must be unique per event.");

        _attendees.Add(attendee);
    }

    public void SetAttendeeResponse(Guid attendeeId, AttendanceStatus status)
    {
        EnsureActive();
        var attendee =
            _attendees.FirstOrDefault(a => a.Id == attendeeId)
            ?? throw new DomainException("Attendee not found.");

        attendee.SetStatus(status);
    }

    private void SetCoreFields(
        string title,
        string? description,
        DateTimeOffset start,
        DateTimeOffset end
    )
    {
        Title = ValidateTitle(title);
        Description = ValidateDescription(description);
        ValidateTimeRange(start, end);
        StartTime = start;
        EndTime = end;
    }

    private void EnsureActive()
    {
        if (Status == EventStatus.Cancelled)
            throw new DomainException("Cannot modify a cancelled event.");
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        title = title.Trim();
        if (title.Length > 200)
            throw new DomainException("Title is too long.");
        return title;
    }

    private static string? ValidateDescription(string? description)
    {
        if (description is null)
            return null;
        description = description.Trim();
        if (description.Length == 0)
            return null;
        if (description.Length > 2000)
            throw new DomainException("Description is too long.");
        return description;
    }

    private static void ValidateTimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new DomainException("EndTime must be after StartTime.");
    }
}

public enum EventStatus
{
    Active = 0,
    Cancelled = 1,
}
