using System.Device.Gpio;
using application.infrastructure;
using application.subSystems;
using domain.systemComponents;
using raspberry_gpio;

namespace api.prodConfig;

public class CaldaiaMetanoRaspberry : CaldaiaMetano
{
    public CaldaiaMetanoRaspberry(
        GpioController gpioCtrl,
        INotificationPublisher hub,
        ILogger<CaldaiaMetano> log) : base(hub, log)
    {
        RELAY_ACCENSIONE_CALDAIA = new RaspberryDigitalOutput(
            name: nameof(RELAY_ACCENSIONE_CALDAIA),
            gpioId: 23, // GPIO=23, PIN=16
            gpio: gpioCtrl,
            log: log
        );
    }
}
