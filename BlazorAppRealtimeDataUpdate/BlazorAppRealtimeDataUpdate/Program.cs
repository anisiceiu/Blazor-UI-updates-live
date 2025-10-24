using BlazorAppRealtimeDataUpdate.Components;
using BlazorAppRealtimeDataUpdate.Hubs;
using BlazorAppRealtimeDataUpdate.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

// Register services
builder.Services.AddSingleton<ResultWatcher>();
builder.Services.AddScoped<ResultService>();

var app = builder.Build();

// Force ResultWatcher to initialize so constructor runs
var watcher = app.Services.GetRequiredService<ResultWatcher>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");
app.MapGet("/testsignalr", async (IHubContext<NotificationHub> hubContext) =>
{
    await hubContext.Clients.All.SendAsync("ReceiveProductChange", "ManualTest");
    return Results.Ok("Sent");
});

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
