using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class DigitalInput : AnalogInput<OnOff>
{
    public OnOffState DigitalValue => (LastMeasure?.Value ?? 0) == 0 ? OnOffState.OFF : OnOffState.ON;
    public override event EventHandler<OnOff>? OnValueRead;
    public event EventHandler<OnOff>? TransitionedFromOffToOn;
    public event EventHandler<OnOff>? TransitionedFromOnToOff;

    public override OnOff? LastMeasure
    {
        get
        {
            return _lastMeasure;
        }
        protected set
        {
            if (value is null)
                return;

            #pragma warning disable CS8600 
            EventHandler<OnOff> eventToFire = this.TransitionedFromOffToOn;
            #pragma warning restore CS8600

            bool shouldFireTransitionEvent = false;

            if (_lastMeasure is null)
                this.FirstTimeSet = value.UtcTimeStamp;

            if (!(_lastMeasure is null) && value.Value != _lastMeasure.Value)
            {
                shouldFireTransitionEvent = true;
                #pragma warning disable CS8600 
                eventToFire = value.Value < _lastMeasure.Value ? this.TransitionedFromOnToOff : this.TransitionedFromOffToOn;
                #pragma warning restore CS8600
            }

            this._lastMeasure = value;
            Fire(this.OnValueRead, value);
            
            if (shouldFireTransitionEvent && eventToFire != null)
                Fire(eventToFire, value);    
        }
    }

    public DigitalInput(string name, ILogger<DigitalInput> log) : base(name, log)
    {
    }
}
