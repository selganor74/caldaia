using domain.measures;
using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class DigitalInput : AnalogInput
{
    public OnOffState DigitalValue => (LastMeasure?.Value ?? 0) == 0 ? OnOffState.OFF : OnOffState.ON;
    public event EventHandler<OnOff>? TransitionedFromOffToOn;
    public event EventHandler<OnOff>? TransitionedFromOnToOff;

    public DigitalInput(string name, ILogger log) : base(name, log)
    {
    }
}
