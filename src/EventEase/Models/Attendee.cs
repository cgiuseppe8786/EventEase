namespace EventEase.Models;

/// <summary>
/// A participant registered to an event, including check-in information.
/// </summary>
public sealed class Attendee
{
    public Guid Id { get; set; }

    public int EventId { get; set; }

    public string FullName { get; set; } = string.Empty;

    /// <summary>Always stored trimmed and lower-case, so comparisons are case-insensitive.</summary>
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateTime RegisteredAt { get; set; }

    public bool CheckedIn { get; set; }

    public DateTime? CheckedInAt { get; set; }
}
