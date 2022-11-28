using System.Device.Gpio;
using application;
using domain.measures;
using domain.systemComponents;
using domain.systemComponents.mocks;
using raspberry_gpio;
using application.subSystems;

namespace api.dependencyInjection;

public static class RaspberryIoSetServiceCollectionExtensions
{
    public static IServiceCollection AddRaspberryIOSet(this IServiceCollection services)
    {
        var injector = services.BuildServiceProvider();
        var gpioCtrl = new GpioController();

        var rotex = BuildRotex(injector, gpioCtrl);
        var camino = BuildCamino(injector, gpioCtrl);
        var caldaia = BuildCaldaiaMetano(injector, gpioCtrl);
        var riscaldamento = BuildRiscaldamento(injector, gpioCtrl);
        
        var caldaiaIoSet = new CaldaiaIOSet(
            cAMINO: camino,
            cALDAIA: caldaia,
            rOTEX: rotex,
            rISCALDAMENTO: riscaldamento
        );

        var config = new CaldaiaConfig(TimeSpan.FromSeconds(1));

        services.AddSingleton(config);
        services.AddSingleton(caldaiaIoSet);

        return services;
    }

    private static Camino BuildCamino(
        ServiceProvider injector,
        GpioController gpioCtrl
    )
    {
        var adcCamino = new Ads1115I2cAnalogInput(
                nameof(CaldaiaIOSet.CAMINO_TEMPERATURA) + " ADC",
                busId: 1,
                addr: AdcAddress.GND, // Means ADDR pin on Ads1115 board is connected to GND
                input: AdcInput.A0_SE,
                readInterval: TimeSpan.FromSeconds(5),
#pragma warning disable CS8604
                log: injector.GetService<ILogger<Ads1115I2cAnalogInput>>()
#pragma warning restore CS8604
            );


        var ccaminoTmp = new NtcVulcanoConverter(
            nameof(CaldaiaIOSet.CAMINO_TEMPERATURA),
            adcCamino,
#pragma warning disable CS8604
            log: injector.GetService<ILogger<NtcVulcanoConverter>>()
#pragma warning restore CS8604
            );

        var caminoOnOff = new ComparatorWithHysteresis<Temperature>(
            nameof(CaldaiaIOSet.CAMINO_ON_OFF),
            ccaminoTmp,
            riseThreshold: 45m,
            fallThreshold: 40m,
            logic: OnOffLogic.OnWhenRaising,
            TimeSpan.FromSeconds(60),
#pragma warning disable CS8604
            log: injector.GetService<ILogger<ComparatorWithHysteresis<Temperature>>>()
#pragma warning restore CS8604
        );

        var relayPompaCamino = new RaspberryDigitalOutput(
                nameof(CaldaiaIOSet.RELAY_POMPA_CAMINO),
                27, // GPIO=27, PIN=13
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalOutput>>()
#pragma warning restore CS8604
            );
        var camino = new Camino(
            cAMINO_TEMPERATURA: ccaminoTmp,
            cAMINO_ON_OFF: caminoOnOff,
            rELAY_POMPA_CAMINO: relayPompaCamino
        );

        return camino;

    }

    private static CaldaiaMetano BuildCaldaiaMetano(
        ServiceProvider injector,
        GpioController gpioCtrl
    )
    {
        var relayAccensioneCaldaia = new RaspberryDigitalOutput(
                nameof(CaldaiaIOSet.RELAY_CALDAIA),
                17, // GPIO=17, PIN=11
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalOutput>>()
#pragma warning restore CS8604
            );

        var caldaia = new CaldaiaMetano(
            rELAY_ACCENSIONE_CALDAIA: relayAccensioneCaldaia
        );

        return caldaia;
    }

    private static Rotex BuildRotex(
        ServiceProvider injector,
        GpioController gpioCtrl
    )
    { 
        var tempAccumulo = new MockAnalogInput<Temperature>(
                nameof(CaldaiaIOSet.ROTEX_TEMP_ACCUMULO),
#pragma warning disable CS8604
                injector.GetService<ILogger<MockAnalogInput<Temperature>>>()
#pragma warning restore CS8604
            );

        var tempPannelli = new MockAnalogInput<Temperature>(
                nameof(CaldaiaIOSet.ROTEX_TEMP_PANNELLI),
#pragma warning disable CS8604
                injector.GetService<ILogger<MockAnalogInput<Temperature>>>()
#pragma warning restore CS8604
            );

        var statoPompaRotex = new MockDigitalInput(
                nameof(CaldaiaIOSet.ROTEX_STATO_POMPA),
#pragma warning disable CS8604
                injector.GetService<ILogger<MockDigitalInput>>()
#pragma warning restore CS8604
            );

        var termostatoRotex = new RaspberryDigitalInput(
                nameof(CaldaiaIOSet.TERMOSTATO_ROTEX),
                6,        // GPIO=6, PIN=31
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalInput>>()
#pragma warning restore CS8604
            );


        tempAccumulo.StartSineInput(
            new Temperature(35),
            new Temperature(80),
            TimeSpan.FromMinutes(19),
            100
        );

        var rotex = new Rotex(
            rOTEX_TEMP_ACCUMULO: tempAccumulo,
            rOTEX_TEMP_PANNELLI: tempPannelli,
            rOTEX_STATO_POMPA: statoPompaRotex,
            tERMOSTATO_ROTEX: termostatoRotex 
        );

        return rotex;
    }

    private static Riscaldamento BuildRiscaldamento(
        ServiceProvider injector,
        GpioController gpioCtrl
    ) 
    {
        var relayBypassTermostatoAmbiente = new RaspberryDigitalOutput(
                nameof(CaldaiaIOSet.RELAY_BYPASS_TERMOSTATO_AMBIENTE),
                22, // GPIO=22, PIN15
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalOutput>>()
#pragma warning restore CS8604
            );

        var termostatoAmbienti = new RaspberryDigitalInput(
                nameof(CaldaiaIOSet.TERMOSTATO_AMBIENTI),
                5,    // GPIO=5, PIN=29
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalInput>>()
#pragma warning restore CS8604
            );

        var relayPompaRiscaldamento = new RaspberryDigitalOutput(
                nameof(CaldaiaIOSet.RELAY_POMPA_RISCALDAMENTO),
                23, // GPIO=23, PIN=16
                gpioCtrl,
#pragma warning disable CS8604
                injector.GetService<ILogger<RaspberryDigitalOutput>>()
#pragma warning restore CS8604
            );

        var riscaldamento = new Riscaldamento(
            rELAY_BYPASS_TERMOSTATO_AMBIENTE: relayBypassTermostatoAmbiente,
            tERMOSTATO_AMBIENTI: termostatoAmbienti,
            rELAY_POMPA_RISCALDAMENTO: relayPompaRiscaldamento  
        );

        return riscaldamento;
    }
}
