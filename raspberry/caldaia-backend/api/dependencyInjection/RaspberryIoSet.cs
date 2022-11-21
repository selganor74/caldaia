using System.Device.Gpio;
using application;
using raspberry_gpio;

namespace api.dependencyInjection;

public static class RaspberryIoSet
{
    public static IServiceCollection AddRaspberryIOSet(this IServiceCollection services)
    {
        var injector = services.BuildServiceProvider();
        var gpioCtrl = new GpioController();
        var caldaiaIoSet = new CaldaiaIOSet();

        // Relay Outputs
        caldaiaIoSet.RELAY_CALDAIA = new RaspberryDigitalOutput(
            nameof(caldaiaIoSet.RELAY_CALDAIA), 
            17, // GPIO=17, PIN=11
            gpioCtrl,
            #pragma warning disable CS8604 
            injector.GetService<ILogger<RaspberryDigitalOutput>>()
            #pragma warning restore CS8604 
        );

        caldaiaIoSet.RELAY_POMPA_CAMINO = new RaspberryDigitalOutput(
            nameof(caldaiaIoSet.RELAY_POMPA_CAMINO), 
            27, // GPIO=27, PIN=13
            gpioCtrl,
            #pragma warning disable CS8604 
            injector.GetService<ILogger<RaspberryDigitalOutput>>()
            #pragma warning restore CS8604 
        );

        caldaiaIoSet.RELAY_BYPASS_TERMOSTATO_AMBIENTE = new RaspberryDigitalOutput(
            nameof(caldaiaIoSet.RELAY_BYPASS_TERMOSTATO_AMBIENTE), 
            22, // GPIO=22, PIN15
            gpioCtrl,
            #pragma warning disable CS8604 
            injector.GetService<ILogger<RaspberryDigitalOutput>>()
            #pragma warning restore CS8604 
        );

        caldaiaIoSet.RELAY_POMPA_RISCALDAMENTO = new RaspberryDigitalOutput(
            nameof(caldaiaIoSet.RELAY_POMPA_RISCALDAMENTO), 
            23, // GPIO=23, PIN=16
            gpioCtrl,
            #pragma warning disable CS8604 
            injector.GetService<ILogger<RaspberryDigitalOutput>>()
            #pragma warning restore CS8604 
        );

        // Digital Inputs
        caldaiaIoSet.TERMOSTATO_AMBIENTI = new RaspberryDigitalInput(
            nameof(caldaiaIoSet.TERMOSTATO_AMBIENTI),
            5,    // GPIO=5, PIN=29
            gpioCtrl,
            #pragma warning disable CS8604 
            injector.GetService<ILogger<RaspberryDigitalInput>>()
            #pragma warning restore CS8604 
        );

        caldaiaIoSet.TERMOSTATO_ROTEX = new RaspberryDigitalInput(
            nameof(caldaiaIoSet.TERMOSTATO_ROTEX),
            6,        // GPIO=6, PIN=31
            gpioCtrl,
            #pragma warning disable CS8604 
            injector.GetService<ILogger<RaspberryDigitalInput>>()
            #pragma warning restore CS8604 
        );

        services.AddSingleton(caldaiaIoSet);
        
        return services;
    }
}
