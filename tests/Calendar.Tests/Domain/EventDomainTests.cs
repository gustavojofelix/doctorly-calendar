using Calendar.Domain.Common;
using Calendar.Domain.Events;
using FluentAssertions;

namespace Calendar.Tests.Domain;

public class EventDomainTests
{
    [Fact]
    public void CreateEvent_EndBeforeStart_ShouldThrow()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddMinutes(-1);

        Action act = () => new CalendarEvent("Title", null, start, end);
        act.Should().Throw<DomainException>().WithMessage("*EndTime must be after StartTime*");
    }

    [Fact]
    public void AddAttendee_DuplicateEmail_ShouldThrow()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);
        var ev = new CalendarEvent("Title", null, start, end);

        ev.AddAttendee(new Attendee("A", "a@test.com"));

        Action act = () => ev.AddAttendee(new Attendee("B", "a@test.com"));
        act.Should().Throw<DomainException>().WithMessage("*unique*");
    }
}
