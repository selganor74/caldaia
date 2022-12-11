using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace domain;

public interface IDevice
{
    string Name { get; }

    IMeasure? LastMeasure { get; }
    event EventHandler<IMeasure>? OnValueRead;

    // returns true if LastMeasure is not null
    bool IsReady();

    DateTime? FirstTimeSet { get; }
    Exception? LastError { get; }
}

public abstract class Device : IDevice
{
    public string Name { get; protected set; }
    public virtual event EventHandler<IMeasure>? OnValueRead;

    public DateTime? FirstTimeSet { get; protected set; }

    public Exception? LastError { get; protected set; }

    protected ILogger log;
    protected virtual IMeasure? _lastMeasure { get; set; }
    public virtual IMeasure? LastMeasure
    {
        get
        {
            return _lastMeasure;
        }
        protected set
        {
            if (value is null)
                return;

            if (_lastMeasure is null)
                this.FirstTimeSet = value.UtcTimeStamp;

            this._lastMeasure = value;
            Fire(this.OnValueRead, value);
        }
    }

    protected Device(string name, ILogger? log = null)
    {
        Name = name;
        this.log = log ?? NullLoggerFactory.Instance.CreateLogger<Device>();
    }

    public bool IsReady()
    {
        return LastMeasure != null;
    }


    protected virtual void Fire(EventHandler<IMeasure>? eventHandler, IMeasure measure)
    {
        if (eventHandler == null)
            return;

        try
        {
            LastError = null;
            eventHandler.Invoke(this, measure);
        }
        catch (Exception e)
        {
            LastError = e;
            log.LogError($"Errors emitting event from {this.Name}.{Environment.NewLine}{e}");
        }
    }
}
