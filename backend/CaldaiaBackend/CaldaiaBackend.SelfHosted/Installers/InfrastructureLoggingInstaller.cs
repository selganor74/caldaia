using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Logging;

namespace CaldaiaBackend.SelfHosted.Installers
{
    class InfrastructureLoggingInstaller : IWindsorInstaller
    {
        private readonly LogLevel logLevel;

        public InfrastructureLoggingInstaller(LogLevel logLevel)
        {
            this.logLevel = logLevel;
        }
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .UseInfrastructureConsoleLogger(logLevel)
                .UseInfrastructureLog4Net()
                .UseInfrastructureEvents();
        }
    }
}
