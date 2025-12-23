using Calendar.Domain.Common;

namespace Calendar.Domain.Events;

public sealed class Attendee
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public AttendanceStatus Status { get; private set; } = AttendanceStatus.Pending;

    private Attendee() { } // EF

    public Attendee(string name, string email)
    {
        Name = ValidateName(name);
        Email = ValidateEmail(email);
    }

    public void SetStatus(AttendanceStatus status) => Status = status;

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Attendee name is required.");
        if (name.Length > 200)
            throw new DomainException("Attendee name is too long.");
        return name.Trim();
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Attendee email is required.");
        email = email.Trim();
        if (email.Length > 320)
            throw new DomainException("Attendee email is too long.");
        // pragmatic validation
        if (!email.Contains('@'))
            throw new DomainException("Attendee email is invalid.");
        return email;
    }
}

public enum AttendanceStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
}
