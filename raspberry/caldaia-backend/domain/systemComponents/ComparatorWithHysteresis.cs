using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public enum OnOffLogic
{
    OnWhenRaising,
    OffWhenRaising
}

public class ComparatorWithHysteresis : DigitalOutput, IDisposable
{
    private readonly AnalogInput source;
    private readonly decimal riseThreshold;
    private readonly decimal fallThreshold;
    private readonly OnOffLogic logic;

    public ComparatorWithHysteresis(
        string name,
        AnalogInput source,
        decimal riseThreshold,
        decimal fallThreshold,
        OnOffLogic logic,
        TimeSpan minTimeBetweenToggles,
        ILogger log
        ) : base(name, log)
    {
        this.SetMinTimeBetweenToggles(minTimeBetweenToggles);
        this.source = source;
        this.riseThreshold = riseThreshold;
        this.fallThreshold = fallThreshold;
        this.logic = logic;

        this._lastMeasure = new OnOff(OnOffState.OFF);

        source.OnValueRead += OnSourceNewValue;
    }

    private void OnSourceNewValue(object? source, IMeasure newValue)
    {
        if ((((OnOff)this.LastMeasure)?.IsOff() ?? true) && logic == OnOffLogic.OnWhenRaising)
        {
            if (newValue.Value > riseThreshold)
                this.SetToOn($"{this.source.Name} value ({newValue.FormattedValue}) > rise threshold ({riseThreshold})");

            return;
        }

        if ((((OnOff)this.LastMeasure)?.IsOn() ?? false) && logic == OnOffLogic.OnWhenRaising)
        {
            if (newValue.Value < fallThreshold)
                this.SetToOff($"{this.source.Name} value ({newValue.FormattedValue}) < fall threshold ({fallThreshold})");

            return;
        }

        if (((OnOff)this.LastMeasure).IsOff() && logic == OnOffLogic.OffWhenRaising)
        {
            if (newValue.Value < fallThreshold)
            {
                this.SetToOn($"{this.source.Name} value ({newValue.FormattedValue}) > threshold ({fallThreshold})");

                return;
            }
        }

        if (((OnOff)this.LastMeasure).IsOn() && logic == OnOffLogic.OffWhenRaising)
        {
            if (newValue.Value > fallThreshold)
            {
                this.SetToOff($"{this.source.Name} value ({newValue.FormattedValue}) > threshold ({fallThreshold})");

                return;
            }
        }
    }

    private bool isDisposing = false;
    public override void Dispose()
    {
        if (isDisposing)
            return;
        
        isDisposing = true;

        base.Dispose();

        if (this.source as IDisposable != null)
            log.LogDebug($"{Name} is disposing its source {source.Name} [{source.GetType().Name}]");

        (this.source as IDisposable)?.Dispose();
    }

    protected override void SetToOffImplementation()
    {
        // nothing to do here;
    }

    protected override void SetToOnImplementation()
    {
        // nothing to do here;
    }
}
