using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class AnalogInput<TMeasure> : Device<TMeasure>
    where TMeasure : IMeasure
{
    public AnalogInput(
        string name,
        ILogger<AnalogInput<TMeasure>> log
        ) : base(name, log)
    {
    }
}
