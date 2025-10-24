using BlazorAppRealtimeDataUpdate.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Data;

public class ResultWatcher
{
    private readonly string _connectionString;
    private readonly IHubContext<NotificationHub> _hubContext;

    public ResultWatcher(IHubContext<NotificationHub> hubContext, IConfiguration config)
    {
        _hubContext = hubContext;
        _connectionString = config.GetConnectionString("DefaultConnection");

        // Start the dependency listener ONCE
        SqlDependency.Start(_connectionString);

        // Begin watching table
        WatchTable();
    }

    private void WatchTable()
    {
        // Create a new connection and command every time we subscribe
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            "SELECT [Id], [Score], [Result] FROM dbo.Results", connection);

        // Must open connection before assigning dependency
        connection.Open();

        // Create a dependency and associate it with the command
        var dependency = new SqlDependency(command);

        // Attach handler
        dependency.OnChange += async (sender, e) =>
        {
            try
            {
                // Only fire when there’s an actual data change
                if (e.Type == SqlNotificationType.Change)
                {
                    Console.WriteLine($"SQL change detected: {e.Info}");

                    // Notify all connected Blazor clients
                    await _hubContext.Clients.All
                        .SendAsync("ReceiveResultChange", e.Type.ToString());


                }

                // Always re-subscribe after each notification
                WatchTable();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Watcher error: {ex.Message}");
            }
        };

        // Execute the query so that SqlDependency starts listening
        using var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
    }
}
