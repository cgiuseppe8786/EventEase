using EventEase;
using EventEase.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IBrowserStorage, BrowserStorage>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<UserSessionService>();

var host = builder.Build();

// Restore the persisted state before the first render, so every page starts from the same data.
await host.Services.GetRequiredService<UserSessionService>().InitializeAsync();
await host.Services.GetRequiredService<AttendanceService>().InitializeAsync();

await host.RunAsync();
