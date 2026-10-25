using EventEase.Models;

namespace EventEase.Services;

/// <summary>
/// Holds the state of the current user session and keeps it in sessionStorage,
/// so it survives page reloads but ends when the tab is closed or the session expires.
/// </summary>
public sealed class UserSessionService(IBrowserStorage storage, TimeProvider time)
{
    public const string StorageKey = "eventease.session";

    public static readonly TimeSpan SessionDuration = TimeSpan.FromHours(4);

    private UserSession? _session;

    /// <summary>Raised when the user logs in or out.</summary>
    public event Action? OnChange;

    /// <summary>The current user, or null when nobody is logged in or the session has expired.</summary>
    public UserSession? CurrentUser => _session is { } session && session.ExpiresAt > time.GetUtcNow() ? session : null;

    public bool IsLoggedIn => CurrentUser is not null;

    public async Task InitializeAsync()
    {
        _session = await storage.GetAsync<UserSession>(StorageArea.Session, StorageKey);
        if (_session is not null && CurrentUser is null)
        {
            // Expired session left in storage: clean it up.
            _session = null;
            await storage.RemoveAsync(StorageArea.Session, StorageKey);
        }
    }

    public async Task LoginAsync(string name, string email)
    {
        var now = time.GetUtcNow();
        _session = new UserSession(name.Trim(), email.Trim().ToLowerInvariant(), now, now + SessionDuration);
        await storage.SetAsync(StorageArea.Session, StorageKey, _session);
        OnChange?.Invoke();
    }

    public async Task LogoutAsync()
    {
        _session = null;
        await storage.RemoveAsync(StorageArea.Session, StorageKey);
        OnChange?.Invoke();
    }
}
