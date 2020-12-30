using System.Configuration;
using ArduinoCommunication;
using CaldaiaBackend.Application.Services;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.DomainEvents;
using Infrastructure.Logging;

namespace CaldaiaBackend.SelfHosted.IoC
{
    class ArduinoInstaller_RELEASE
        : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<IArduinoDataReader, IArduinoCommandIssuer>()
                    .ImplementedBy<CaldaiaControllerViaArduino>()
                    .UsingFactoryMethod((kernel) =>
                    {
                        var serialPort = ConfigurationManager.AppSettings["ArduinoComPort"];
                        var controller = new CaldaiaControllerViaArduino(serialPort, kernel.Resolve<IEventDispatcher>(), kernel.Resolve<ILoggerFactory>());
                        controller.Start();
                        return controller;
                    })
                    .LifestyleSingleton()
            );
        }
    }
}
