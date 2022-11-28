using domain.systemComponents;
using domain.measures;
using application.infrastructure;

namespace application.subSystems;

public class Rotex : IDisposable
{
    public AnalogInput<Temperature> ROTEX_TEMP_ACCUMULO { get; set; }

    public AnalogInput<Temperature> ROTEX_TEMP_PANNELLI { get; set; }

    public DigitalInput ROTEX_STATO_POMPA { get; set; }

    public DigitalInput TERMOSTATO_ROTEX { get; set; }

    public Rotex(
        AnalogInput<Temperature> rOTEX_TEMP_ACCUMULO,
        AnalogInput<Temperature> rOTEX_TEMP_PANNELLI,
        DigitalInput rOTEX_STATO_POMPA,
        DigitalInput tERMOSTATO_ROTEX
        )
    {
        ROTEX_TEMP_ACCUMULO = rOTEX_TEMP_ACCUMULO;
        ROTEX_TEMP_PANNELLI = rOTEX_TEMP_PANNELLI;
        ROTEX_STATO_POMPA = rOTEX_STATO_POMPA;
        TERMOSTATO_ROTEX = tERMOSTATO_ROTEX;
    }

    public void Dispose()
    {
        this.DisposeDisposables();
    }
}
#pragma warning restore CS8618
