using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace domain;

public interface IDevice
{
    string Name { get; }

    // returns true if LastMeasure is not null
    bool IsReady();

    DateTime? FirstTimeSet { get; }
    Exception? LastError { get; }
}

public interface IDevice<TMeasure> : IDevice where TMeasure : IMeasure
{
    TMeasure? LastMeasure { get; }
    event EventHandler<TMeasure>? OnValueRead;
}

public abstract class Device<TMeasure> : IDevice<TMeasure>
    where TMeasure : IMeasure
{
    public string Name { get; protected set; }
    public virtual event EventHandler<TMeasure>? OnValueRead;

    public DateTime? FirstTimeSet { get; protected set; }

    public Exception? LastError { get; protected set; }

    protected ILogger<Device<TMeasure>> log;
    protected TMeasure? _lastMeasure;
    public virtual TMeasure? LastMeasure
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

    protected Device(string name, ILogger<Device<TMeasure>>? log = null)
    {
        Name = name;
        this.log = log ?? NullLoggerFactory.Instance.CreateLogger<Device<TMeasure>>();
    }

    public bool IsReady()
    {
        return LastMeasure != null;
    }


    protected virtual void Fire(EventHandler<TMeasure>? eventHandler, TMeasure measure)
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
