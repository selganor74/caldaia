using application.infrastructure;
using domain.measures;
using domain.systemComponents;

namespace application.services;

// Provides reading from the Rotex Accumulator
public interface IRotexReader : IStartable
{
    object GetRotexConfig();
    AnalogInput<Temperature> ROTEX_TEMPERATURA_PANNELLI { get; }
    AnalogInput<Temperature> ROTEX_TEMPERATURA_ACCUMULO { get; }
    DigitalInput ROTEX_STATO_POMPA { get; }
}
