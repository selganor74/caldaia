
using application.infrastructure;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using Microsoft.Extensions.Logging;

namespace application.subSystems;

public class Rotex : Subsystem
{
    public AnalogInput ROTEX_TEMP_ACCUMULO { get; set; }

    public AnalogInput ROTEX_TEMP_PANNELLI { get; set; }

    public DigitalInput ROTEX_STATO_POMPA { get; set; }

    public DigitalInput TERMOSTATO_ROTEX { get; set; }

    public Rotex(
        INotificationPublisher hub,
        ILogger<Rotex> log
    ) : base(hub, log)
    {
        ROTEX_TEMP_ACCUMULO = new MockAnalogInput(nameof(ROTEX_TEMP_ACCUMULO), log);
        ROTEX_TEMP_PANNELLI = new MockAnalogInput(nameof(ROTEX_TEMP_PANNELLI), log);
        ROTEX_STATO_POMPA = new MockDigitalInput(nameof(ROTEX_STATO_POMPA), log);
        TERMOSTATO_ROTEX = new MockDigitalInput(nameof(TERMOSTATO_ROTEX), log);

        ((MockAnalogInput)ROTEX_TEMP_ACCUMULO).SetInput(new Temperature(50m));
        ((MockAnalogInput)ROTEX_TEMP_PANNELLI).SetInput(new Temperature(25m));
        ((MockDigitalInput)ROTEX_STATO_POMPA).Set(domain.measures.OnOffState.OFF);
        ((MockDigitalInput)TERMOSTATO_ROTEX).Set(domain.measures.OnOffState.OFF);
    }
}
