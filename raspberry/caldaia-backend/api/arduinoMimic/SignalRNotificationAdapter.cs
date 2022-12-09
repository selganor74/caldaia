using api.signalr;
using application;
using application.infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace api.arduinoMimic;
public class SignalRNotificationAdapter
{
    private readonly ILogger<SignalRNotificationAdapter> _log;
    private readonly INotificationSubscriber notificationHub;
    private readonly IHubContext<DataHub> signalrDataHub;

    public SignalRNotificationAdapter(
        INotificationSubscriber appObservable,
        IHubContext<DataHub> signalrDataHub,
        ILogger<SignalRNotificationAdapter> log
        )
    {
        _log = log;
        notificationHub = appObservable;
        this.signalrDataHub = signalrDataHub;
    }

    public void Start()
    {
        _log.LogInformation("Subscribing to status-reading channel.");

        notificationHub.Subscribe<CaldaiaAllValues>(
            "status-reading",
            (CaldaiaAllValues data) =>
                {
                    _log.LogDebug($"Converting {nameof(CaldaiaAllValues)} to {nameof(DataFromArduino)}");
                    NotifyData(DataFromArduino.From(data));
                });

        notificationHub.Subscribe<CaldaiaAllValues>(
            "status-reading",
            (CaldaiaAllValues data) =>
                {
                    _log.LogDebug($"Directly sending CaldaiaAllValues to channel caldaia-state.");
                    signalrDataHub.Clients.All.SendAsync("caldaia-state", data);
                });

        //_appObservable.Subscribe("settings", (SettingsFromArduino settings) => { NotifyToChannel("settings", settings); });
        //_appObservable.Subscribe("raw", (string rawData) => { NotifyToChannel("raw", rawData); });
    }

    private void NotifyData(DataFromArduino data)
    {
        try
        {
            _log.LogDebug("Notifying status to datahub clients.");
            signalrDataHub.Clients.All.SendAsync("notify", data);
        }
        catch (Exception e)
        {
            _log.LogWarning("Errors in Hub.", e);
        }
    }
}
