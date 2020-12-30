using System.Diagnostics;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Logging;

namespace CaldaiaBackend.SelfHosted.Installers
{
    class InfrastructureInstaller : IWindsorInstaller
    {
        private bool _enableAuth;

        public InfrastructureInstaller(bool enableAuth)
        {
            _enableAuth = enableAuth;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {

            container
                .UseInfrastructureActions()
                .UseInfrastructureActionsInstrumentation()
                .UseInfrastructurePushNotification()
                ;
            if (_enableAuth)
                container.UseInfrastructureActionsAuthorization();

            // Removes all trace lieteners to stop direct output to console of System.Diagnostics.Trace
			// Diagnostics Traces will be routed through infrastructures's logging facility.
            Trace.Listeners.Clear();
            container.Resolve<ILoggerFactory>()
                .CreateNewLogger("System.Diagnostics.Trace")
                .AttachToSystemDiagnosticsTrace();
        }
    }
}
