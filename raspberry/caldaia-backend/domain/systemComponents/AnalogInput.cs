using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class AnalogInput<TMeasure> where TMeasure : IMeasure
{
    protected readonly ILogger<AnalogInput<TMeasure>> log;
    protected TMeasure? _lastMeasure;

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
            {
                // this is the first time the value is set.
                // we can't say if value has changed or transitioned 
                this.FirstTimeSet = value.UtcTimeStamp;
                Fire(this.ValueChanged, value);
            }
            else
            {
                if (value.Value != _lastMeasure.Value)
                {
                    // value has changed
                    Fire(this.ValueChanged, value);
                }
            }

            this._lastMeasure = value;
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

    public virtual event EventHandler<TMeasure>? ValueChanged;

    protected virtual void Fire(EventHandler<TMeasure>? eventHandler, TMeasure measure)
    {
        if (eventHandler == null)
            return;

        try
        {
            eventHandler.Invoke(this, measure);
        }
        catch (Exception e)
        {
            log.LogError($"Errors emitting event from {this.Name}.{Environment.NewLine}{e}");
        }
    }
}
