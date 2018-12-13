using System;
using System.Threading.Tasks;
using CaldaiaBackend.Application;
using CaldaiaBackend.Application.Commands;
using CaldaiaBackend.Application.Interfaces;
using CaldaiaBackend.ArduinoCommunication;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Infrastructure.Actions;
using Infrastructure.Actions.Command.Handler;
using Infrastructure.Actions.Query.Handler;
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
            controller = new CaldaiaControllerViaArduino("COM5");
            controller.Start();

            Container.Register(
                Component
                    .For<IArduinoDataReader, IArduinoCommandIssuer>()
                    .Instance( controller )
                );

            var factory = new CastleApplicationFactory(Container);
            factory.BuildApplication<ArduinoBackendApplication>();

            var logWriter = Container.Resolve<ILogWriter>();
            logWriter.SetLogLevel(LogLevel.Info);

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
