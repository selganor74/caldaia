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
        private readonly Config _config;

        public ArduinoInstaller_RELEASE(Config config)
        {
            _config = config;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<IArduinoDataReader, IArduinoCommandIssuer>()
                    .ImplementedBy<CaldaiaControllerViaArduino>()
                    .UsingFactoryMethod((kernel) =>
                    {
                        var serialPort = _config.arduinoComPort;
                        var controller = new CaldaiaControllerViaArduino(serialPort, kernel.Resolve<IEventDispatcher>(), kernel.Resolve<ILoggerFactory>());
                        controller.Start();
                        return controller;
                    })
                    .LifestyleSingleton()
            );
        }
    }
}
