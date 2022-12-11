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
                27, // GPIO=27, PIN=13
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
                gpioId: 17, // GPIO=17, PIN=11
                gpioCtrl,
                log
        );
    }
}
