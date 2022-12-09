using domain.systemComponents;
using domain.measures;
using application.infrastructure;

namespace application.subSystems;

public class Camino : IDisposable
{
    public AnalogInput CAMINO_TEMPERATURA { get; set; }
    public ComparatorWithHysteresis CAMINO_ON_OFF { get; set; }
    public DigitalOutput RELAY_POMPA_CAMINO { get; set; }

    public Camino(
        AnalogInput cAMINO_TEMPERATURA,
        ComparatorWithHysteresis cAMINO_ON_OFF,
        DigitalOutput rELAY_POMPA_CAMINO
        )
    {
        CAMINO_TEMPERATURA = cAMINO_TEMPERATURA;
        CAMINO_ON_OFF = cAMINO_ON_OFF;
        RELAY_POMPA_CAMINO = rELAY_POMPA_CAMINO;
    }


    public void Dispose()
    {
        this.DisposeDisposables(null);
    }
}
#pragma warning restore CS8618
