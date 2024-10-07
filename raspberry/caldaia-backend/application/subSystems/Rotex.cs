
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

    public Rotex(
        INotificationPublisher hub,
        ILogger<Rotex> log
    ) : base(hub, log)
    {
        SetUpMockInputOutputs(log);
    }

    private void SetUpMockInputOutputs(ILogger log)
    {
        var tempAccumulo = new MockAnalogInput(nameof(ROTEX_TEMP_ACCUMULO), log);
        var tempPannelli = new MockAnalogInput(nameof(ROTEX_TEMP_PANNELLI), log);
        var statoPompa = new MockDigitalInput(nameof(ROTEX_STATO_POMPA), log);

        tempAccumulo.SetInput(new Temperature(50m));
        tempPannelli.SetInput(new Temperature(25m));
        statoPompa.Set(OnOffState.OFF);

        ROTEX_TEMP_ACCUMULO = tempAccumulo;
        ROTEX_TEMP_PANNELLI = tempPannelli;
        ROTEX_STATO_POMPA = statoPompa;
    }

    public override void SetAsNotReady()
    {
        SetUpMockInputOutputs(log);

        base.SetAsNotReady();
    }
}
