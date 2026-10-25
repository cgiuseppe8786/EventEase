using EventEase.Models;
using EventEase.Services;

namespace EventEase.Tests;

public class UserSessionServiceTests
{
    private readonly InMemoryBrowserStorage _storage = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Login_StoresSessionAndRaisesOnChange()
    {
        var service = new UserSessionService(_storage, _time);
        var notifications = 0;
        service.OnChange += () => notifications++;

        await service.LoginAsync(" Maria Rossi ", " Maria@Example.IT ");

        Assert.True(service.IsLoggedIn);
        Assert.Equal("Maria Rossi", service.CurrentUser!.Name);
        Assert.Equal("maria@example.it", service.CurrentUser.Email);
        Assert.Contains((StorageArea.Session, UserSessionService.StorageKey), _storage.Items.Keys);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public async Task Initialize_RestoresSessionFromStorage()
    {
        await new UserSessionService(_storage, _time).LoginAsync("Maria Rossi", "maria@example.it");

        var restored = new UserSessionService(_storage, _time);
        await restored.InitializeAsync();

        Assert.Equal("maria@example.it", restored.CurrentUser?.Email);
    }

    [Fact]
    public async Task Session_ExpiresAfterConfiguredDuration()
    {
        var service = new UserSessionService(_storage, _time);
        await service.LoginAsync("Maria Rossi", "maria@example.it");

        _time.UtcNow += UserSessionService.SessionDuration + TimeSpan.FromMinutes(1);

        Assert.Null(service.CurrentUser);
        Assert.False(service.IsLoggedIn);
    }

    [Fact]
    public async Task Initialize_DiscardsExpiredSession()
    {
        var expired = new UserSession("Maria", "maria@example.it", _time.UtcNow.AddHours(-5), _time.UtcNow.AddHours(-1));
        await _storage.SetAsync(StorageArea.Session, UserSessionService.StorageKey, expired);

        var service = new UserSessionService(_storage, _time);
        await service.InitializeAsync();

        Assert.False(service.IsLoggedIn);
        Assert.Empty(_storage.Items);
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        var service = new UserSessionService(_storage, _time);
        await service.LoginAsync("Maria Rossi", "maria@example.it");

        await service.LogoutAsync();

        Assert.False(service.IsLoggedIn);
        Assert.Empty(_storage.Items);
    }
}
