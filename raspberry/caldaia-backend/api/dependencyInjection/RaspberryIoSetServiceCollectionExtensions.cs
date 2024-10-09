using System.Device.Gpio;
using application;
using domain.systemComponents;
using domain.systemComponents.mocks;
using raspberry_gpio;
using application.subSystems;
using rotex;
using api.prodConfig;

namespace api.dependencyInjection;

public static class RaspberryIoSetServiceCollectionExtensions
{
    public static IServiceCollection AddRaspberryIOSet(this IServiceCollection services, RaspberryRotexReaderConfig rotexConfig)
    {
        services.AddSingleton(rotexConfig);

        services.AddSingleton<GpioController>();
        
        services.AddSingleton<CaldaiaMetano, CaldaiaMetanoRaspberry>();
        services.AddSingleton<Camino,CaminoRaspberry>();
        services.AddSingleton<Riscaldamento,RiscaldamentoRaspberry>();
        services.AddSingleton<Rotex,RotexRaspberry>();

        services.AddSingleton<CaldaiaIOSet>();

        return services;
    }
}
