using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class AnalogInput<TMeasure> where TMeasure : IMeasure
{
    protected readonly ILogger<AnalogInput<TMeasure>> log;
    protected TMeasure? _lastMeasure;

    public Exception? LastError { get; protected set; }

    public decimal AnalogValue => LastMeasure?.Value ?? 0;
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
            Fire(this.ValueRead, value);
        }
    }

    public DateTime? FirstTimeSet { get; protected set; }
    public string Name { get; protected set; }

    public AnalogInput(
        string name,
        ILogger<AnalogInput<TMeasure>> log
        )
    {
        this.log = log;
        this.Name = name;
    }

    public virtual event EventHandler<TMeasure>? ValueRead;

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
