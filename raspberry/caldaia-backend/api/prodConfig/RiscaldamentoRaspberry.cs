using System.Device.Gpio;
using application.infrastructure;
using application.subSystems;
using domain.systemComponents;
using raspberry_gpio;

namespace api.prodConfig;

public class RiscaldamentoRaspberry : Riscaldamento
{
    public RiscaldamentoRaspberry(
        GpioController gpioCtrl,
        INotificationPublisher hub,
        ILogger<Riscaldamento> log) : base(hub, log)
    {
        RELAY_BYPASS_TERMOSTATO_AMBIENTE = new RaspberryDigitalOutput(
                nameof(RELAY_BYPASS_TERMOSTATO_AMBIENTE),
                22, // GPIO=22, PIN=15
                gpioCtrl,
                log
            );

        var termostatoAmbienti = new RaspberryDigitalInput(
                nameof(TERMOSTATO_AMBIENTI),
                5,    // GPIO=5, PIN=29
                gpioCtrl,
                log
            );

        TERMOSTATO_AMBIENTI = new LogicNot(
            nameof(TERMOSTATO_AMBIENTI) + " Negated",
            termostatoAmbienti,
            log
        );

        RELAY_POMPA_RISCALDAMENTO = new RaspberryDigitalOutput(
                nameof(RELAY_POMPA_RISCALDAMENTO),
                gpioId: 23, // GPIO=23, PIN=23
                gpioCtrl,
                log
        );

        var termostatoRotex = new RaspberryDigitalInput(
            nameof(TERMOSTATO_ROTEX),
            6,        // GPIO=6, PIN=31
            gpioCtrl,
            log
        );

        TERMOSTATO_ROTEX = new LogicNot(
            nameof(TERMOSTATO_ROTEX) + " Negated",
            termostatoRotex,
            log
        );
    }

    protected override void Init()
    {
        // do nothing !
    }
}
