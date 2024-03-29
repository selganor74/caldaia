
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
        var tempAccumulo = new MockAnalogInput(nameof(ROTEX_TEMP_ACCUMULO), log);
        var tempPannelli = new MockAnalogInput(nameof(ROTEX_TEMP_PANNELLI), log);
        var statoPompa = new MockDigitalInput(nameof(ROTEX_STATO_POMPA), log);
        var termoRotex = new MockDigitalInput(nameof(TERMOSTATO_ROTEX), log);
        
        tempAccumulo.SetInput(new Temperature(50m));
        tempPannelli.SetInput(new Temperature(25m));
        statoPompa.Set(domain.measures.OnOffState.OFF);
        termoRotex.Set(domain.measures.OnOffState.OFF);

        ROTEX_TEMP_ACCUMULO = tempAccumulo;
        ROTEX_TEMP_PANNELLI = tempPannelli;
        ROTEX_STATO_POMPA = statoPompa;
        TERMOSTATO_ROTEX = termoRotex;
    }
}
