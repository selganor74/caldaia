using domain.measures;
using Microsoft.Extensions.DependencyInjection;

namespace application.dependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddCaldaiaApplication(this IServiceCollection services)
        {
            services.AddSingleton<CaldaiaApplication>();
            return services;
        }

        public static IServiceProvider StartCaldaiaApplication(this IServiceProvider injector)
        {
            var app = injector.GetService<CaldaiaApplication>();
            
            app?.Start();

            return injector;
        }
    }
}
