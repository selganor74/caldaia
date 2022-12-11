using application.infrastructure;
using domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace application.subSystems;

public abstract class Subsystem : IDisposable
{
    private readonly INotificationPublisher hub;

    private ILogger log { get; set; }

    event EventHandler<IMeasure> OnValueRead;

    protected Subsystem(
        INotificationPublisher hub,
        ILogger? log = null
        )
    {
        this.hub = hub;

        if (log == null)
            log = NullLoggerFactory.Instance.CreateLogger(GetType());

        this.log = log;
        var t = new Timer((state) => RegisterEvents(),null, TimeSpan.FromMilliseconds(500), Timeout.InfiniteTimeSpan );
    }

    protected void RegisterEvents()
    {
        log.LogInformation($"Registering events for Subsystem {GetType().Name}");
        var allProps = this.GetType().GetProperties().ToList();

        foreach (var prop in allProps)
        {
            var device = prop.GetValue(this) as IDevice;
            if (device != null)
            {
                device.OnValueRead += this.OnValueReadConcentrator;
            }
        }
    }

    protected virtual void OnValueReadConcentrator(object? source, IMeasure data)
    {
        var device = source as IDevice;
        log.LogDebug($"{nameof(OnValueReadConcentrator)} Received event from {device?.Name ?? "Unknown"} : {data.ToString()}");
        try
        {
            OnValueRead.Invoke(source, data);
        }
        catch (Exception e)
        {
            log.LogError($"{nameof(OnValueReadConcentrator)} Errors invoking handlers.{Environment.NewLine}{e}");
        }
    }

    public void Dispose()
    {
        this.DisposeDisposables(null);
    }

    public bool IsReady()
    {
        if (this == null)
            return false;

        var alldevices = new List<IDevice>();
        var allProps = this.GetType().GetProperties().ToList();

        foreach (var prop in allProps)
        {
            var device = prop.GetValue(this) as IDevice;
            if (device != null)
            {
                alldevices.Add(device);
            }
        }

        return alldevices
            .DefaultIfEmpty()
            .All(d =>
            {
                if (d is null)
                    return false;

                var result = d.IsReady();
                if (!result)
                    log.LogDebug($"{d.Name} IsReady: {result}");
                
                return result;
            });
    }
}
