using System.Net.Http.Json;
using EventEase.Models;

namespace EventEase.Services;

/// <summary>
/// Provides the event catalogue. Data is downloaded once from <see cref="DataPath"/>
/// and then served from memory.
/// </summary>
public sealed class EventService(HttpClient http)
{
    public const string DataPath = "sample-data/events.json";

    private Task<List<EventItem>>? _loading;

    /// <summary>All events, ordered by date.</summary>
    public async Task<IReadOnlyList<EventItem>> GetEventsAsync() => await LoadAsync();

    public async Task<EventItem?> GetEventByIdAsync(int id) => (await LoadAsync()).Find(e => e.Id == id);

    public async Task<IReadOnlyList<string>> GetCategoriesAsync() =>
        (await LoadAsync()).Select(e => e.Category).Distinct().Order().ToList();

    public async Task UpdateEventAsync(EventItem updated)
    {
        var events = await LoadAsync();
        var index = events.FindIndex(e => e.Id == updated.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Evento {updated.Id} non trovato.");
        }

        // Store a copy: callers keep editing their own instance without side effects on the cache.
        events[index] = updated.Clone();
        SortByDate(events);
    }

    // Every caller shares the same Task, so concurrent components trigger a single HTTP request.
    private Task<List<EventItem>> LoadAsync() => _loading ??= FetchAsync();

    private async Task<List<EventItem>> FetchAsync()
    {
        try
        {
            var events = await http.GetFromJsonAsync<List<EventItem>>(DataPath) ?? [];
            SortByDate(events);
            return events;
        }
        catch
        {
            // Do not cache a failure: the next call retries the download.
            _loading = null;
            throw;
        }
    }

    private static void SortByDate(List<EventItem> events) => events.Sort(static (a, b) => a.Date.CompareTo(b.Date));
}
