using EventEase.Models;

namespace EventEase.Services;

/// <summary>
/// Manages event registrations and check-ins. Data is persisted in localStorage,
/// so it survives page reloads.
/// </summary>
public sealed class AttendanceService(IBrowserStorage storage, TimeProvider time)
{
    public const string StorageKey = "eventease.attendees";

    private List<Attendee> _attendees = [];

    /// <summary>Raised whenever registrations or check-ins change.</summary>
    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        _attendees = await storage.GetAsync<List<Attendee>>(StorageArea.Local, StorageKey) ?? [];
    }

    public IReadOnlyList<Attendee> GetAttendees(int eventId) =>
        _attendees.Where(a => a.EventId == eventId).OrderBy(a => a.RegisteredAt).ToList();

    public IReadOnlyList<Attendee> GetRegistrationsByEmail(string email)
    {
        var normalized = NormalizeEmail(email);
        return _attendees.Where(a => a.Email == normalized).OrderBy(a => a.RegisteredAt).ToList();
    }

    public int GetRegisteredCount(int eventId) => _attendees.Count(a => a.EventId == eventId);

    public bool IsRegistered(int eventId, string email)
    {
        var normalized = NormalizeEmail(email);
        return _attendees.Exists(a => a.EventId == eventId && a.Email == normalized);
    }

    public AttendanceStats GetStats(EventItem ev)
    {
        var registered = 0;
        var checkedIn = 0;
        foreach (var attendee in _attendees)
        {
            if (attendee.EventId != ev.Id)
            {
                continue;
            }

            registered++;
            if (attendee.CheckedIn)
            {
                checkedIn++;
            }
        }

        return new AttendanceStats(registered, checkedIn, ev.Capacity);
    }

    public async Task<RegistrationResult> RegisterAsync(EventItem ev, RegistrationModel model)
    {
        var now = time.GetLocalNow().DateTime;

        if (ev.Date <= now)
        {
            return new RegistrationResult(RegistrationStatus.EventClosed);
        }

        if (IsRegistered(ev.Id, model.Email))
        {
            return new RegistrationResult(RegistrationStatus.AlreadyRegistered);
        }

        if (GetRegisteredCount(ev.Id) >= ev.Capacity)
        {
            return new RegistrationResult(RegistrationStatus.EventFull);
        }

        var attendee = new Attendee
        {
            Id = Guid.NewGuid(),
            EventId = ev.Id,
            FullName = model.FullName.Trim(),
            Email = NormalizeEmail(model.Email),
            Phone = model.Phone,
            RegisteredAt = now
        };

        _attendees.Add(attendee);
        await SaveAsync();
        return new RegistrationResult(RegistrationStatus.Success, attendee);
    }

    public async Task SetCheckInAsync(Guid attendeeId, bool checkedIn)
    {
        var attendee = _attendees.Find(a => a.Id == attendeeId);
        if (attendee is null || attendee.CheckedIn == checkedIn)
        {
            return;
        }

        attendee.CheckedIn = checkedIn;
        attendee.CheckedInAt = checkedIn ? time.GetLocalNow().DateTime : null;
        await SaveAsync();
    }

    public async Task<bool> CancelRegistrationAsync(Guid attendeeId)
    {
        var removed = _attendees.RemoveAll(a => a.Id == attendeeId && !a.CheckedIn) > 0;
        if (removed)
        {
            await SaveAsync();
        }

        return removed;
    }

    private async Task SaveAsync()
    {
        await storage.SetAsync(StorageArea.Local, StorageKey, _attendees);
        OnChange?.Invoke();
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
