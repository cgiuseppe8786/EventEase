using EventEase.Models;
using EventEase.Services;

namespace EventEase.Tests;

public class AttendanceServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly InMemoryBrowserStorage _storage = new();
    private readonly AttendanceService _service;

    public AttendanceServiceTests()
    {
        _service = new AttendanceService(_storage, new FakeTimeProvider(Now));
    }

    private static EventItem CreateEvent(int capacity = 10, int daysFromNow = 7) => new()
    {
        Id = 1,
        Name = "Meetup .NET",
        Location = "Roma",
        Category = "Meetup",
        Date = Now.DateTime.AddDays(daysFromNow),
        Capacity = capacity
    };

    private static RegistrationModel Registration(string email = "maria@example.it") => new()
    {
        FullName = "  Maria Rossi ",
        Email = email,
        AcceptTerms = true
    };

    [Fact]
    public async Task Register_ValidRequest_StoresNormalizedAttendee()
    {
        var result = await _service.RegisterAsync(CreateEvent(), Registration(" Maria@Example.IT "));

        Assert.True(result.Succeeded);
        Assert.Equal("Maria Rossi", result.Attendee!.FullName);
        Assert.Equal("maria@example.it", result.Attendee.Email);
        Assert.Equal(Now.DateTime, result.Attendee.RegisteredAt);
        Assert.Contains((StorageArea.Local, AttendanceService.StorageKey), _storage.Items.Keys);
    }

    [Fact]
    public async Task Register_SameEmailDifferentCase_IsRejectedAsDuplicate()
    {
        var ev = CreateEvent();
        await _service.RegisterAsync(ev, Registration("maria@example.it"));

        var result = await _service.RegisterAsync(ev, Registration("MARIA@example.it"));

        Assert.Equal(RegistrationStatus.AlreadyRegistered, result.Status);
        Assert.Equal(1, _service.GetRegisteredCount(ev.Id));
    }

    [Fact]
    public async Task Register_WhenFull_IsRejected()
    {
        var ev = CreateEvent(capacity: 1);
        await _service.RegisterAsync(ev, Registration("a@example.it"));

        var result = await _service.RegisterAsync(ev, Registration("b@example.it"));

        Assert.Equal(RegistrationStatus.EventFull, result.Status);
    }

    [Fact]
    public async Task Register_PastEvent_IsRejected()
    {
        var result = await _service.RegisterAsync(CreateEvent(daysFromNow: -1), Registration());

        Assert.Equal(RegistrationStatus.EventClosed, result.Status);
    }

    [Fact]
    public async Task CheckIn_UpdatesStatsAndRaisesOnChange()
    {
        var ev = CreateEvent(capacity: 4);
        var first = await _service.RegisterAsync(ev, Registration("a@example.it"));
        await _service.RegisterAsync(ev, Registration("b@example.it"));
        var notifications = 0;
        _service.OnChange += () => notifications++;

        await _service.SetCheckInAsync(first.Attendee!.Id, true);

        var stats = _service.GetStats(ev);
        Assert.Equal(new AttendanceStats(Registered: 2, CheckedIn: 1, Capacity: 4), stats);
        Assert.Equal(50, stats.AttendanceRate);
        Assert.Equal(2, stats.AvailableSeats);
        Assert.Equal(Now.DateTime, _service.GetAttendees(ev.Id)[0].CheckedInAt);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public async Task Cancel_CheckedInAttendee_IsNotAllowed()
    {
        var ev = CreateEvent();
        var registration = await _service.RegisterAsync(ev, Registration());
        await _service.SetCheckInAsync(registration.Attendee!.Id, true);

        Assert.False(await _service.CancelRegistrationAsync(registration.Attendee.Id));
        Assert.Equal(1, _service.GetRegisteredCount(ev.Id));
    }

    [Fact]
    public async Task Cancel_RegisteredAttendee_FreesTheSeat()
    {
        var ev = CreateEvent();
        var registration = await _service.RegisterAsync(ev, Registration());

        Assert.True(await _service.CancelRegistrationAsync(registration.Attendee!.Id));
        Assert.Equal(0, _service.GetRegisteredCount(ev.Id));
    }

    [Fact]
    public async Task Initialize_RestoresPersistedRegistrations()
    {
        var ev = CreateEvent();
        await _service.RegisterAsync(ev, Registration());

        var restored = new AttendanceService(_storage, new FakeTimeProvider(Now));
        await restored.InitializeAsync();

        Assert.True(restored.IsRegistered(ev.Id, "maria@example.it"));
        Assert.Single(restored.GetRegistrationsByEmail("MARIA@example.it"));
    }
}
