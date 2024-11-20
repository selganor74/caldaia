using System.Device.Gpio;
using application.infrastructure;
using application.subSystems;
using domain.systemComponents;
using raspberry_gpio;
using static raspberry_gpio.RaspberryGpio;

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
                (int)RelayOutput_GPIO.RELAY_BYPASS_TERMOSTATO,
                gpioCtrl,
                log
            );

        var termostatoAmbienti = new RaspberryDigitalInput(
                nameof(TERMOSTATO_AMBIENTI),
                (int)DigitalInput_GPIO.TERMOSTATO_AMBIENTI,    // GPIO=13, PIN=33
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
                gpioId: (int)RelayOutput_GPIO.RELAY_POMPA_RISCALDAMENTO,
                gpioCtrl,
                log
        );

        var termostatoRotex = new RaspberryDigitalInput(
            nameof(TERMOSTATO_ROTEX),
            (int)DigitalInput_GPIO.TERMOSTATO_ROTEX,
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
