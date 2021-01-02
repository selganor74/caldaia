using ArduinoCommunication;
using CaldaiaBackend.Application.Services;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Events;
using Infrastructure.Logging;

namespace CaldaiaBackend.SelfHosted.Installers
{
    class ArduinoInstaller_RELEASE
        : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var config = container.Resolve<Config>();

            container.Register(
                Component
                    .For<IArduinoDataReader, IArduinoCommandIssuer>()
                    .ImplementedBy<CaldaiaControllerViaArduino>()
                    .UsingFactoryMethod((kernel) =>
                    {
                        var serialPort = config.arduinoComPort;
                        var controller = new CaldaiaControllerViaArduino(serialPort, kernel.Resolve<IEventDispatcher>(), kernel.Resolve<ILoggerFactory>());
                        controller.Start();
                        return controller;
                    })
                    .LifestyleSingleton()
            );
        }
    }
}
