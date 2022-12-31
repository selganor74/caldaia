using application.infrastructure;
using domain;
using domain.meters;
using domain.systemComponents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace application.subSystems;

public abstract class Subsystem : IDisposable
{
    public List<DigitalMeter> DigitalMeters { get; } = new List<DigitalMeter>();
    public List<AnalogMeter> AnalogMeters { get; } = new List<AnalogMeter>();
    event EventHandler<IMeasure> OnValueRead;

    private readonly INotificationPublisher hub;
    private ILogger log { get; set; }

    protected Subsystem(
        INotificationPublisher hub,
        ILogger? log = null
        )
    {
        this.hub = hub;

        if (log == null)
            log = NullLoggerFactory.Instance.CreateLogger(GetType());

        this.log = log;
        var m = new Timer((state) => RegisterMeters(), null, TimeSpan.FromMilliseconds(250), Timeout.InfiniteTimeSpan);
        var e = new Timer((state) => RegisterEvents(), null, TimeSpan.FromMilliseconds(500), Timeout.InfiniteTimeSpan);
        var i = new Timer((state) => Init(), null, TimeSpan.FromMilliseconds(750), Timeout.InfiniteTimeSpan);
    }

    protected virtual void Init() 
    {
        // here you can do whatever initialization you like
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

    protected void RegisterMeters()
    {
        log.LogInformation($"Registering meters for Subsystem {GetType().Name}");
        var allProps = this.GetType().GetProperties().ToList();

        foreach (var prop in allProps)
        {
            var digital = prop.GetValue(this) as DigitalInput;
            if (digital != null)
            {
                this.DigitalMeters.Add(new DigitalMeter(digital));
            }

            var analog = prop.GetValue(this) as AnalogInput;
            if (analog != null && digital == null)
            {
                this.AnalogMeters.Add(new AnalogMeter(analog));
            }
        }

    }

    protected virtual void OnValueReadConcentrator(object? source, IMeasure data)
    {
        var device = source as IDevice;
        log.LogDebug($"{nameof(OnValueReadConcentrator)} Received event from {device?.Name ?? "Unknown"} : {data.ToString()}");
        try
        {
            OnValueRead?.Invoke(source, data);
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
