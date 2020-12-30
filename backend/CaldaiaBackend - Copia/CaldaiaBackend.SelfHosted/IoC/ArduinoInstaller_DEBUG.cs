using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Application.Services.Mocks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace CaldaiaBackend.SelfHosted.IoC
{
    class ArduinoInstaller_DEBUG : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<IArduinoDataReader, IArduinoCommandIssuer>()
                    .ImplementedBy<ArduinoMock>()
                    .LifestyleSingleton()
            );
        }
    }
}
