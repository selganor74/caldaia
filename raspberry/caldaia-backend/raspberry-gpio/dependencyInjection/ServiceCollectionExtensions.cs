using application.services;
using Microsoft.Extensions.DependencyInjection;
using raspberry_gpio;

namespace rotex.dependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRaspberryGpio(this IServiceCollection services)
    {
        services.AddSingleton<IGpioInputReader, RaspberryGpio>();
        return services;
    }
}
