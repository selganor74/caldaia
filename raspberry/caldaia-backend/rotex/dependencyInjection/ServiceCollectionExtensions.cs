using Microsoft.Extensions.DependencyInjection;

namespace rotex.dependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSerialRotexReader(this IServiceCollection services, RaspberryRotexReaderConfig config)
    {
        services.AddSingleton(config);
        services.AddSingleton<RaspberryRotexReader>();
        return services;
    }
}
