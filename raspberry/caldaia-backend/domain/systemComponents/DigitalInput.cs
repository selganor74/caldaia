using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class DigitalInput : AnalogInput<OnOff>
{
    public OnOffState DigitalValue => (LastMeasure?.Value ?? 0) == 0 ? OnOffState.OFF : OnOffState.ON;
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
                    if (value.Value < this._lastMeasure.Value)
                        Fire(this.TransitionedFromOnToOff, value);
                    else
                        Fire(this.TransitionedFromOffToOn, value);
                }
            }

            this._lastMeasure = value;
        }
    }

    public override event EventHandler<OnOff>? ValueChanged;
    public event EventHandler<OnOff>? TransitionedFromOffToOn;
    public event EventHandler<OnOff>? TransitionedFromOnToOff;

    public DigitalInput(string name, ILogger<DigitalInput> log) : base(name, log)
    {
    }
}
