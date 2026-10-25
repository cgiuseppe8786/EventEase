using System.Text.Json;
using Microsoft.JSInterop;

namespace EventEase.Services;

public enum StorageArea
{
    /// <summary>localStorage: survives browser restarts.</summary>
    Local,

    /// <summary>sessionStorage: cleared when the tab is closed.</summary>
    Session
}

/// <summary>
/// Abstraction over the browser Web Storage API, so services can be unit tested without a browser.
/// </summary>
public interface IBrowserStorage
{
    ValueTask<T?> GetAsync<T>(StorageArea area, string key);

    ValueTask SetAsync<T>(StorageArea area, string key, T value);

    ValueTask RemoveAsync(StorageArea area, string key);
}

/// <summary>
/// Stores values as JSON in localStorage / sessionStorage through JS interop.
/// </summary>
public sealed class BrowserStorage(IJSRuntime js) : IBrowserStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<T?> GetAsync<T>(StorageArea area, string key)
    {
        try
        {
            var json = await js.InvokeAsync<string?>($"{AreaName(area)}.getItem", key);
            return json is null ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // Corrupted or outdated data must not crash the app: behave as if nothing was stored.
            return default;
        }
        catch (JSException)
        {
            // Storage can be unavailable (e.g. blocked by the browser's privacy settings).
            return default;
        }
    }

    public async ValueTask SetAsync<T>(StorageArea area, string key, T value)
    {
        try
        {
            await js.InvokeVoidAsync($"{AreaName(area)}.setItem", key, JsonSerializer.Serialize(value, JsonOptions));
        }
        catch (JSException)
        {
            // Quota exceeded or storage disabled: the app keeps working with in-memory state only.
        }
    }

    public async ValueTask RemoveAsync(StorageArea area, string key)
    {
        try
        {
            await js.InvokeVoidAsync($"{AreaName(area)}.removeItem", key);
        }
        catch (JSException)
        {
        }
    }

    private static string AreaName(StorageArea area) => area == StorageArea.Session ? "sessionStorage" : "localStorage";
}
