using System;
using CaldaiaBackend.Application;
using CaldaiaBackend.Application.Interfaces;
using CaldaiaBackend.Application.Interfaces.Mocks;
using CaldaiaBackend.ArduinoCommunication;
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
        private static CaldaiaControllerViaArduino controller;

        static void Main(string[] args)
        {

            // TODO: Read Com Configuration from AppSettings

            Container.Register(
                Component
                    .For<IArduinoDataReader, IArduinoCommandIssuer>()
                    //*
                    .ImplementedBy<ArduinoMock>()
                    //*/
                    /*
                    .ImplementedBy<CaldaiaControllerViaArduino>()
                    .UsingFactoryMethod((kernel) =>
                    {
                        controller = new CaldaiaControllerViaArduino("COM5", kernel.Resolve<ILoggerFactory>());
                        controller.Start();
                        return controller;
                    })
                    //*/
                    .LifestyleSingleton()
                );

            var factory = new CastleApplicationFactory(Container);
            factory.BuildApplication<ArduinoBackendApplication>();

            var logWriter = Container.Resolve<ILogWriter>();
            logWriter.SetLogLevel(LogLevel.Trace);

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
