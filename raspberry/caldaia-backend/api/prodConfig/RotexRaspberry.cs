using System.Device.Gpio;
using application.infrastructure;
using application.subSystems;
using domain.systemComponents;
using domain.systemComponents.mocks;
using raspberry_gpio;
using rotex;

namespace api.prodConfig;

public class RotexRaspberry : Rotex
{
    private readonly RaspberryRotexReader serialRotex;

    public RotexRaspberry(
        RaspberryRotexReaderConfig rotexReaderConfig,
        GpioController gpioCtrl,
        INotificationPublisher hub,
        ILogger<Rotex> log
        ) : base(hub, log)
    {
        var tempAccumulo = new MockAnalogInput(
                nameof(ROTEX_TEMP_ACCUMULO),
                log
            );

        var tempPannelli = new MockAnalogInput(
                nameof(ROTEX_TEMP_PANNELLI),
                log
            );

        var statoPompaRotex = new MockDigitalInput(
                nameof(ROTEX_STATO_POMPA),
                log
            );

        var termostatoRotex = new RaspberryDigitalInput(
                nameof(TERMOSTATO_ROTEX),
                6,        // GPIO=6, PIN=31
                gpioCtrl,
                log
            );

        var termostatoRotexNegated = new LogicNot(
            nameof(TERMOSTATO_ROTEX) + " Negated",
            termostatoRotex,
            log
            );

        ROTEX_STATO_POMPA = statoPompaRotex;
        ROTEX_TEMP_ACCUMULO = tempAccumulo;
        ROTEX_TEMP_PANNELLI = tempPannelli;

        serialRotex = new RaspberryRotexReader(
            rotexReaderConfig,
            rOTEX_TEMPERATURA_PANNELLI: tempPannelli,
            rOTEX_TEMPERATURA_ACCUMULO: tempAccumulo,
            rOTEX_STATO_POMPA: statoPompaRotex,
            log
        );


    }

    protected override void Init()
    {
        // do nothing
        serialRotex.Start();
    }
}
