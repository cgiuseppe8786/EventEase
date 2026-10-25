using System.Net;
using System.Text;
using System.Text.Json;
using EventEase.Services;

namespace EventEase.Tests;

/// <summary>In-memory replacement of localStorage/sessionStorage that also round-trips through JSON.</summary>
internal sealed class InMemoryBrowserStorage : IBrowserStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Dictionary<(StorageArea Area, string Key), string> Items { get; } = [];

    public ValueTask<T?> GetAsync<T>(StorageArea area, string key) =>
        ValueTask.FromResult(Items.TryGetValue((area, key), out var json) ? JsonSerializer.Deserialize<T>(json, JsonOptions) : default);

    public ValueTask SetAsync<T>(StorageArea area, string key, T value)
    {
        Items[(area, key)] = JsonSerializer.Serialize(value, JsonOptions);
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(StorageArea area, string key)
    {
        Items.Remove((area, key));
        return ValueTask.CompletedTask;
    }
}

internal sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;

    public override DateTimeOffset GetUtcNow() => UtcNow;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

/// <summary>Returns a fixed JSON payload and counts the requests it receives.</summary>
internal sealed class StubJsonHandler(string json) : HttpMessageHandler
{
    public int RequestCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}
