using application.infrastructure;
using domain.systemComponents;

namespace application.subSystems;

public class Riscaldamento : IDisposable
{
    public DigitalOutput RELAY_BYPASS_TERMOSTATO_AMBIENTE { get; set; }
    public DigitalInput TERMOSTATO_AMBIENTI { get; set; }
    public DigitalOutput RELAY_POMPA_RISCALDAMENTO { get; set; }

    public Riscaldamento(DigitalOutput rELAY_BYPASS_TERMOSTATO_AMBIENTE, DigitalInput tERMOSTATO_AMBIENTI, DigitalOutput rELAY_POMPA_RISCALDAMENTO)
    {
        RELAY_BYPASS_TERMOSTATO_AMBIENTE = rELAY_BYPASS_TERMOSTATO_AMBIENTE;
        TERMOSTATO_AMBIENTI = tERMOSTATO_AMBIENTI;
        RELAY_POMPA_RISCALDAMENTO = rELAY_POMPA_RISCALDAMENTO;
    }

    public void Dispose()
    {
        this.DisposeDisposables();
    }
}
#pragma warning restore CS8618
