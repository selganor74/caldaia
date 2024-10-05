using application.infrastructure;
using domain;
using domain.meters;
using domain.systemComponents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;

namespace application.subSystems;

public abstract class Subsystem : IDisposable
{
    public List<DigitalMeter> DigitalMeters { get; } = new List<DigitalMeter>();
    public List<AnalogMeter> AnalogMeters { get; } = new List<AnalogMeter>();
    event EventHandler<IMeasure> OnValueRead;

    private readonly INotificationPublisher hub;
    protected ILogger log { get; set; }

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

    private Dictionary<object, Stopwatch> timeouts = new Dictionary<object, Stopwatch>();
    private Dictionary<object, int> flippingCounters = new Dictionary<object, int>();
    private Dictionary<object, Timer> latestValueTimeouts = new Dictionary<object, Timer>();

    private TimeSpan READ_LIMIT_TIMEOUT = TimeSpan.FromSeconds(1);
    // if we get more than FLIPPING_THRESHOLD / READ_LIMIT_TIMEOUT we consider the input as flipping
    private int FLIPPING_THRESHOLD = 10;
    private bool _reportAsNotReady;

    protected virtual void OnValueReadConcentrator(object? source, IMeasure data)
    {
        var device = source as IDevice;
        if (device is null)
            return;

        // we use this timeout to limit the number of readings we get from the raspberry ...
        // and to signal "flipping" inputs
        Stopwatch deviceTimeout;
        lock (timeouts)
        {
            deviceTimeout = timeouts.GetValueOrDefault(source!)!;
            if (deviceTimeout is null)
            {
                deviceTimeout = Stopwatch.StartNew();
                timeouts.Add(source!, deviceTimeout);
            }
        }

        if (deviceTimeout.Elapsed < READ_LIMIT_TIMEOUT)
        {
            lock (flippingCounters)
            {
                var fc = flippingCounters.GetValueOrDefault(source!);
                fc++;
                flippingCounters[source!] = fc;
                if (fc == FLIPPING_THRESHOLD)
                {
                    log.LogDebug($"{nameof(OnValueReadConcentrator)} {device?.Name ?? "Unknown"} is FLIPPING !!! ");
                }
            }

            lock (latestValueTimeouts)
            {
                var lvt = latestValueTimeouts.GetValueOrDefault(source!);
                if (lvt is not null)
                {
                    lvt?.Change(Timeout.Infinite, Timeout.Infinite);
                    lvt?.Dispose();
                    latestValueTimeouts.Remove(source!);
                }

                latestValueTimeouts[source!] = 
                    new Timer(
                        callback: (obj) => OnValueRead?.Invoke(source!, data),
                        state: null,
                        dueTime: READ_LIMIT_TIMEOUT,
                        period: Timeout.InfiniteTimeSpan
                        );

            }

            deviceTimeout.Restart();
            return;
        }
        else
        {
            flippingCounters[source!] = 0;
            deviceTimeout.Restart();
        }

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

    public virtual void SetAsNotReady()
    {
        log.LogDebug($"Subsystem {GetType().Name} Has been marked as 'Unavailable'");
        this._reportAsNotReady = true;
    } 

    public virtual bool IsReady()
    {
        if (this._reportAsNotReady)
        {
            return false;
        }

        var alldevices = new List<IDevice>();
        var allProps = this.GetType().GetProperties().ToList();

        foreach (var prop in allProps)
        {
            var device = prop.GetValue(this) as IDevice;
            if (device is not null)
            {
                alldevices.Add(device);
            }
        }

        return alldevices
            .DefaultIfEmpty()
            .All(d =>
            {
                var result = d!.IsReady();
                if (!result)
                    log.LogDebug($"{d.Name} IsReady: {result}");

                return result;
            });
    }
}
