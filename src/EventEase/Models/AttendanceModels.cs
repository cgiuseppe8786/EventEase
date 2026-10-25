namespace EventEase.Models;

public enum RegistrationStatus
{
    Success,
    AlreadyRegistered,
    EventFull,
    EventClosed
}

public sealed record RegistrationResult(RegistrationStatus Status, Attendee? Attendee = null)
{
    public bool Succeeded => Status == RegistrationStatus.Success;
}

/// <summary>
/// Aggregated numbers shown by the attendance tracker.
/// </summary>
public sealed record AttendanceStats(int Registered, int CheckedIn, int Capacity)
{
    public int AvailableSeats => Math.Max(Capacity - Registered, 0);

    public bool IsFull => Registered >= Capacity;

    /// <summary>Percentage (0-100) of registered attendees who checked in.</summary>
    public int AttendanceRate => Registered == 0 ? 0 : (int)Math.Round(CheckedIn * 100.0 / Registered);
}
