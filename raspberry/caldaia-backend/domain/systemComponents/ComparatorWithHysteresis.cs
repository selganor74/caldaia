using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public enum OnOffLogic
{
    OnWhenRaising,
    OffWhenRaising
}

public class ComparatorWithHysteresis<TMeasure> : DigitalOutput, IDisposable
    where TMeasure : IMeasure
{
    private readonly AnalogInput<TMeasure> source;
    private readonly decimal riseThreshold;
    private readonly decimal fallThreshold;
    private readonly OnOffLogic logic;

    public ComparatorWithHysteresis(
        string name,
        AnalogInput<TMeasure> source,
        decimal riseThreshold,
        decimal fallThreshold,
        OnOffLogic logic,
        TimeSpan minTimeBetweenToggles,
        ILogger<ComparatorWithHysteresis<TMeasure>> log
        ) : base(name, log)
    {
        this.SetMinTimeBetweenToggles(minTimeBetweenToggles);
        this.source = source;
        this.riseThreshold = riseThreshold;
        this.fallThreshold = fallThreshold;
        this.logic = logic;
        source.ValueRead += OnSourceNewValue;
    }

    private void OnSourceNewValue(object? source, TMeasure newValue)
    {
        if ((this.LastMeasure?.IsOff() ?? true) && logic == OnOffLogic.OnWhenRaising)
        {
            if (newValue.Value > riseThreshold)
                this.SetToOn($"value ({newValue.Value}) > rise threshold ({riseThreshold})");

            return;
        }

        if ((this.LastMeasure?.IsOn() ?? false) && logic == OnOffLogic.OnWhenRaising)
        {
            if (newValue.Value < fallThreshold)
                this.SetToOff($"value ({newValue.Value}) < fall threshold ({fallThreshold})");

            return;
        }

        if (this.LastMeasure.IsOff() && logic == OnOffLogic.OffWhenRaising)
        {
            if (newValue.Value < fallThreshold)
            {
                this.SetToOn($"value ({newValue.Value}) > threshold ({fallThreshold})");

                return;
            }
        }

        if (this.LastMeasure.IsOn() && logic == OnOffLogic.OffWhenRaising)
        {
            if (newValue.Value > fallThreshold)
            {
                this.SetToOff($"value ({newValue.Value}) > threshold ({fallThreshold})");

                return;
            }
        }
    }

    public override void Dispose() {
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
