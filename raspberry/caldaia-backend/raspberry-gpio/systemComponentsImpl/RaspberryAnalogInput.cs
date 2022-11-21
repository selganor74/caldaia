using domain;
using domain.systemComponents;
using Microsoft.Extensions.Logging;

namespace raspberry_gpio;

public class RaspberryAnalogInput<TMeasure> : AnalogInput<TMeasure> where TMeasure : IMeasure
{
    public RaspberryAnalogInput(
        string name, 
        int gpio,
        ILogger<RaspberryAnalogInput<TMeasure>> log) : base(name, log)
    {
    }
}
