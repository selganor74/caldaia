
using Microsoft.AspNetCore.SignalR;

namespace api.arduinoMimic;

public class DataHub : Hub
{
    private readonly ILogger<DataHub> _log;

    public DataHub(ILogger<DataHub> log)
    {
        this._log = log;
    }

    public Task NotifyAll(DataFromArduino data) => Clients.All.SendAsync("data", data);

    public override Task OnConnectedAsync()
    {
        _log.LogInformation($"Client connected to {GetType().Name}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _log.LogInformation($"Client disconnected from {GetType().Name}", exception);
        return base.OnDisconnectedAsync(exception);
    }

}
