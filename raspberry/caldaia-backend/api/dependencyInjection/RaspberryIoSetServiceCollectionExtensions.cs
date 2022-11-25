using System.Device.Gpio;
using application;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using raspberry_gpio;

namespace api.dependencyInjection;

public static class RaspberryIoSetServiceCollectionExtensions
{
    public static IServiceCollection AddRaspberryIOSet(this IServiceCollection services)
    {
        var injector = services.BuildServiceProvider();
        var gpioCtrl = new GpioController();

        var adcCamino = new Ads1115I2cAnalogInput(
                nameof(CaldaiaIOSet.CaminoTemp) + " ADC",
                busId: 1,
                addr: AdcAddress.GND, // Means ADDR pin on Ads1115 board is connected to GND
                input: AdcInput.A0_SE,
                readInterval: TimeSpan.FromSeconds(5),
#pragma warning disable CS8604
                log: injector.GetService<ILogger<Ads1115I2cAnalogInput>>()
#pragma warning restore CS8604
            );
        var ccaminoTmp = new AnalogInputConverter<Voltage, Temperature>(
            nameof(CaldaiaIOSet.CaminoTemp),
            adcCamino,
            (from) =>
            {
                return 2 * from;
            },
#pragma warning disable CS8604
                log: injector.GetService<ILogger<AnalogInputConverter<Voltage, Temperature>>>()
#pragma warning restore CS8604
            );

        var caldaiaIoSet = new CaldaiaIOSet(

            // Relay Outputs
            rELAY_CALDAIA: new RaspberryDigitalOutput(
                nameof(CaldaiaIOSet.RELAY_CALDAIA),
                17, // GPIO=17, PIN=11
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalOutput>>()
#pragma warning restore CS8604
            ),

            rELAY_POMPA_CAMINO: new RaspberryDigitalOutput(
                nameof(CaldaiaIOSet.RELAY_POMPA_CAMINO),
                27, // GPIO=27, PIN=13
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalOutput>>()
#pragma warning restore CS8604
            ),

            rELAY_BYPASS_TERMOSTATO_AMBIENTE: new RaspberryDigitalOutput(
                nameof(CaldaiaIOSet.RELAY_BYPASS_TERMOSTATO_AMBIENTE),
                22, // GPIO=22, PIN15
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalOutput>>()
#pragma warning restore CS8604
            ),

            rELAY_POMPA_RISCALDAMENTO: new RaspberryDigitalOutput(
                nameof(CaldaiaIOSet.RELAY_POMPA_RISCALDAMENTO),
                23, // GPIO=23, PIN=16
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalOutput>>()
#pragma warning restore CS8604
            ),

            // Digital Inputs
            tERMOSTATO_AMBIENTI: new RaspberryDigitalInput(
                nameof(CaldaiaIOSet.TERMOSTATO_AMBIENTI),
                5,    // GPIO=5, PIN=29
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalInput>>()
#pragma warning restore CS8604
            ),

            tERMOSTATO_ROTEX: new RaspberryDigitalInput(
                nameof(CaldaiaIOSet.TERMOSTATO_ROTEX),
                6,        // GPIO=6, PIN=31
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalInput>>()
#pragma warning restore CS8604
            ),

            caminoTemp: ccaminoTmp,

            rotexStatoPompa: new MockDigitalInput(
                nameof(CaldaiaIOSet.RotexStatoPompa),
#pragma warning disable CS8604
                injector.GetService<ILogger<MockDigitalInput>>()
#pragma warning restore CS8604
            ),

            rotexTempAccumulo: new MockAnalogInput<Temperature>(
                nameof(CaldaiaIOSet.RotexTempAccumulo),
#pragma warning disable CS8604
                injector.GetService<ILogger<MockAnalogInput<Temperature>>>()
#pragma warning restore CS8604
            ),

            rotexTempPannelli: new MockAnalogInput<Temperature>(
                nameof(CaldaiaIOSet.RotexTempPannelli),
#pragma warning disable CS8604
                injector.GetService<ILogger<MockAnalogInput<Temperature>>>()
#pragma warning restore CS8604
            )
        );

        var config = new CaldaiaConfig(TimeSpan.FromSeconds(1));

        services.AddSingleton(config);
        services.AddSingleton(caldaiaIoSet);

        return services;
    }
}
