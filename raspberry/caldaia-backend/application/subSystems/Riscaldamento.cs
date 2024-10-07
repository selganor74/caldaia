using application.infrastructure;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using Microsoft.Extensions.Logging;

namespace application.subSystems;

public class Riscaldamento : Subsystem
{
    public DigitalOutput RELAY_BYPASS_TERMOSTATO_AMBIENTE { get; set; }
    public DigitalInput TERMOSTATO_AMBIENTI { get; set; }
    public DigitalOutput RELAY_POMPA_RISCALDAMENTO { get; set; }
    public DigitalInput TERMOSTATO_ROTEX { get; set; }

    public Riscaldamento(
        INotificationPublisher hub,
        ILogger<Riscaldamento> log
    ) : base(hub, log)
    {
        var bypass = new MockDigitalOutput(nameof(RELAY_BYPASS_TERMOSTATO_AMBIENTE),log);
        bypass.SetToOff("init");
        RELAY_BYPASS_TERMOSTATO_AMBIENTE = bypass;

        var termoAmb = new MockDigitalInput(nameof(TERMOSTATO_AMBIENTI), log);
        termoAmb.Set(OnOffState.OFF);
        TERMOSTATO_AMBIENTI = termoAmb;

        var pompa = new MockDigitalOutput(nameof(RELAY_POMPA_RISCALDAMENTO), log);
        pompa.SetToOff("init");
        RELAY_POMPA_RISCALDAMENTO = pompa;


        var termoRotex = new MockDigitalOutput(nameof(TERMOSTATO_ROTEX), log);
        termoRotex.SetToOff("init");
        TERMOSTATO_ROTEX = termoRotex;
    }
}
