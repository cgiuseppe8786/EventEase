namespace EventEase.Models;

/// <summary>
/// The signed-in user. Persisted in the browser's sessionStorage.
/// </summary>
public sealed record UserSession(string Name, string Email, DateTimeOffset StartedAt, DateTimeOffset ExpiresAt);
