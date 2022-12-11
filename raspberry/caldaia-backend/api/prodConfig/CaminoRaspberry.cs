using System.Device.Gpio;
using application.infrastructure;
using application.subSystems;
using domain.systemComponents;
using raspberry_gpio;

namespace api.prodConfig;

public class CaminoRaspberry : Camino
{
    public CaminoRaspberry(
        GpioController gpioCtrl,
        INotificationPublisher hub,
        ILogger<Camino> log) : base(hub, log)
    {
        var adcCamino = new Ads1115I2cAnalogInput(
            nameof(CAMINO_TEMPERATURA) + " ADC",
            busId: 1,
            addr: AdcAddress.GND, // Means ADDR pin on Ads1115 board is connected to GND
            input: AdcInput.A0_SE,
            readInterval: TimeSpan.FromSeconds(5),
            log: log
        );


        CAMINO_TEMPERATURA = new NtcVulcanoConverter(
            nameof(CAMINO_TEMPERATURA),
            adcCamino,
            log: log
            );

        CAMINO_ON_OFF = new ComparatorWithHysteresis(
            nameof(CAMINO_ON_OFF),
            CAMINO_TEMPERATURA,
            riseThreshold: 45m,
            fallThreshold: 40m,
            logic: OnOffLogic.OnWhenRaising,
            TimeSpan.FromSeconds(60),
            log: log
        );

        RELAY_POMPA_CAMINO = new RaspberryDigitalOutput(
                nameof(RELAY_POMPA_CAMINO),
                22, // GPIO=22, PIN15
                gpioCtrl,
                log: log
            );
    }
}
