using application.subSystems;
using domain.measures;
using Microsoft.Extensions.DependencyInjection;

namespace application.dependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddMockIOSet(this IServiceCollection services)
        {
            services.AddSingleton<CaldaiaMetano>();
            services.AddSingleton<Camino>();
            services.AddSingleton<Riscaldamento>();
            services.AddSingleton<Rotex>();
            
            services.AddSingleton<CaldaiaIOSet>();

            return services;
        }

        public static IServiceCollection AddCaldaiaApplication(this IServiceCollection services, CaldaiaConfig config)
        {
            services.AddSingleton(config);

            services.AddSingleton<CaldaiaApplication>();
            return services;
        }

        public static CaldaiaApplication StartCaldaiaApplication(this IServiceProvider injector)
        {
            var app = injector.GetService<CaldaiaApplication>();
            
            app!.Start();

            return app;
        }
    }
}
