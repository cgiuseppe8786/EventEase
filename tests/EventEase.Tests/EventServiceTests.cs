using EventEase.Models;
using EventEase.Services;

namespace EventEase.Tests;

public class EventServiceTests
{
    private const string Json = """
        [
          { "id": 2, "name": "Workshop", "date": "2027-02-01T10:00:00", "location": "Torino", "category": "Workshop", "capacity": 20 },
          { "id": 1, "name": "Conferenza", "date": "2027-01-01T09:00:00", "location": "Milano", "category": "Conferenza", "capacity": 100 },
          { "id": 3, "name": "Meetup", "date": "2027-03-01T18:00:00", "location": "Roma", "category": "Workshop", "capacity": 50 }
        ]
        """;

    private readonly StubJsonHandler _handler = new(Json);
    private readonly EventService _service;

    public EventServiceTests()
    {
        _service = new EventService(new HttpClient(_handler) { BaseAddress = new Uri("https://localhost/") });
    }

    [Fact]
    public async Task GetEvents_ReturnsEventsOrderedByDate()
    {
        var events = await _service.GetEventsAsync();

        Assert.Equal([1, 2, 3], events.Select(e => e.Id));
    }

    [Fact]
    public async Task Data_IsDownloadedOnlyOnce()
    {
        await Task.WhenAll(_service.GetEventsAsync(), _service.GetEventByIdAsync(1), _service.GetCategoriesAsync());
        await _service.GetEventsAsync();

        Assert.Equal(1, _handler.RequestCount);
    }

    [Fact]
    public async Task GetEventById_UnknownId_ReturnsNull()
    {
        Assert.Null(await _service.GetEventByIdAsync(99));
    }

    [Fact]
    public async Task GetCategories_ReturnsDistinctSortedValues()
    {
        Assert.Equal(["Conferenza", "Workshop"], await _service.GetCategoriesAsync());
    }

    [Fact]
    public async Task Update_StoresACopyAndKeepsDateOrder()
    {
        var edited = (await _service.GetEventByIdAsync(1))!.Clone();
        edited.Name = "Conferenza (aggiornata)";
        edited.Date = new DateTime(2027, 4, 1, 9, 0, 0);

        await _service.UpdateEventAsync(edited);
        edited.Name = "Modifica successiva non salvata";

        var events = await _service.GetEventsAsync();
        Assert.Equal([2, 3, 1], events.Select(e => e.Id));
        Assert.Equal("Conferenza (aggiornata)", (await _service.GetEventByIdAsync(1))!.Name);
    }

    [Fact]
    public async Task Update_UnknownEvent_Throws()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateEventAsync(new EventItem { Id = 42 }));
    }
}
