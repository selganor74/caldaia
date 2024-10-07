using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class DigitalInput : AnalogInput
{
    public OnOffState DigitalValue => (LastMeasure?.Value ?? 0) == 0 ? OnOffState.OFF : OnOffState.ON;
    public event EventHandler<OnOff>? TransitionedFromOffToOn;
    public event EventHandler<OnOff>? TransitionedFromOnToOff;

    public override IMeasure? LastMeasure
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
                base.LastMeasure = value;
                return;
            }

            if (((OnOff)_lastMeasure).DigitalValue != ((OnOff)value).DigitalValue)
            {
                if(((OnOff)value).DigitalValue == OnOffState.ON) {
                    log.LogDebug($"{nameof(DigitalInput)}<{this.Name}>: Transitioned from Off to On");
                    this.TransitionedFromOffToOn?.Invoke(this, (OnOff)value);
                }
                else
                {
                    log.LogDebug($"{nameof(DigitalInput)}<{this.Name}>: Transitioned from On to Off");
                    this.TransitionedFromOnToOff?.Invoke(this, (OnOff)value);
                }
            }
            base.LastMeasure = value;
        }
    }


    public DigitalInput(string name, ILogger log) : base(name, log)
    {
    }
}
