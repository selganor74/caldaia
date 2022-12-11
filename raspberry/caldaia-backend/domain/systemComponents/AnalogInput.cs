using Microsoft.Extensions.Logging;

namespace domain.systemComponents;

public abstract class AnalogInput : Device
{
    public AnalogInput(
        string name,
        ILogger log
        ) : base(name, log)
    {
    }
}
