using System;
using System.Configuration;
using ArduinoCommunication;
using CaldaiaBackend.Application;
using CaldaiaBackend.Application.Interfaces;
using CaldaiaBackend.Application.Interfaces.Mocks;
using CaldaiaBackend.SelfHosted.Infrastructure.SignalRLogging;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Infrastructure.Hosting.IoC.CastleWindsor;
using Infrastructure.Logging;
using Topshelf;

namespace CaldaiaBackend.SelfHosted
{
    class Program
    {
        public static IWindsorContainer Container = new WindsorContainer();
        private static ILogger log;

        static void Main(string[] args)
        {

            Container.Register(
                Component
                    .For<IArduinoDataReader, IArduinoCommandIssuer>()
#if DEBUG
                    .ImplementedBy<ArduinoMock>()
#endif
#if RELEASE
                    .ImplementedBy<CaldaiaControllerViaArduino>()
                    .UsingFactoryMethod((kernel) =>
                    {
                        var serialPort = ConfigurationManager.AppSettings["ArduinoComPort"];
                        var controller = new CaldaiaControllerViaArduino(serialPort, kernel.Resolve<ILoggerFactory>());
                        controller.Start();
                        return controller;
                    })
#endif
                    .LifestyleSingleton()
                );

            var factory = new CastleApplicationFactory(Container);
            factory.BuildApplication<ArduinoBackendApplication>();

            var logWriter = Container.Resolve<ILogWriter>();
            logWriter.SetLogLevel(LogLevel.Trace);

            var clw = logWriter as CompositeLogWriter;
            if (clw != null)
            {
                var signalrLogWriter = new SignalRLogWriter(LogLevel.Warning);
                clw.AddLogger(signalrLogWriter, LogLevelMode.Independent);
            }

            var loggerFactory = Container.Resolve<ILoggerFactory>();
            log = loggerFactory.CreateNewLogger(nameof(Program));

            var application = Container.Resolve<ArduinoBackendApplication>();
            application.Start();

            var tsh = HostFactory.Run(hc =>
            {
                hc.Service<WebAppRunner>(s =>
                {
                    s.ConstructUsing(name => new WebAppRunner(loggerFactory));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                hc.RunAsLocalSystem();

                hc.SetDescription("Arduino Backend");
                hc.SetDisplayName("Arduino Backend");
                hc.SetServiceName("arduinoBackend");
            });

            var exitCode = (int) Convert.ChangeType(tsh, tsh.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
