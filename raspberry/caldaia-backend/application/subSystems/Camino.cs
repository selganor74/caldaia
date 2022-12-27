using domain.systemComponents;
using domain.measures;
using application.infrastructure;
using Microsoft.Extensions.Logging;
using domain.systemComponents.mocks;

namespace application.subSystems;

public class Camino : Subsystem
{
    public AnalogInput CAMINO_TEMPERATURA { get; set; }
    public ComparatorWithHysteresis CAMINO_ON_OFF { get; set; }
    public DigitalOutput RELAY_POMPA_CAMINO { get; set; }

    public Camino(
        INotificationPublisher hub,
        ILogger<Camino> log
    ) : base(hub, log)
    {
        var camino_temperatura = new MockAnalogInput(nameof(CAMINO_TEMPERATURA), log);
        camino_temperatura.SetInput(new Temperature(45m));
        CAMINO_TEMPERATURA = camino_temperatura;

        CAMINO_ON_OFF = new ComparatorWithHysteresis(
            nameof(CAMINO_ON_OFF),
            CAMINO_TEMPERATURA,
            45m,
            40m,
            OnOffLogic.OnWhenRaising, TimeSpan.FromSeconds(60),
            log);

        var relayPompaCamino = new MockDigitalOutput(nameof(RELAY_POMPA_CAMINO), log);
        relayPompaCamino.SetToOff("init");
        RELAY_POMPA_CAMINO = relayPompaCamino;
    }

    protected override void Init()
    {
        ((MockAnalogInput)CAMINO_TEMPERATURA).StartSineInput(
            min: new Temperature(25),
            max: new Temperature(98),
            period: TimeSpan.FromMinutes(30),
            measuresPerPeriod: 3000
        );
    }

    public void Dispose()
    {
        this.DisposeDisposables(null);
    }
}
