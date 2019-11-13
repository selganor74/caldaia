using System;
using CaldaiaBackend.Application;
using CaldaiaBackend.SelfHosted.IoC;
using Castle.Windsor;
using Infrastructure.Application;
using Infrastructure.Hosting.IoC.CastleWindsor;
using Infrastructure.Logging;
using Topshelf;

namespace CaldaiaBackend.SelfHosted
{
    class Program
    {
        public static IWindsorContainer Container;
        private static ILogger log;

        static void Main(string[] args)
        {
            using (Container = new WindsorContainer())
            {
                RegisterMainComponents();

                var app = BuildAppComponentsAndApplication();
                using (app as IDisposable)
                {
                    SetupLogging();

                    StartupApplication();

                    RunInOwinAndTopshelf();
                }
            }
        }

        private static void RunInOwinAndTopshelf()
        {
            var loggerFactory = Container.Resolve<ILoggerFactory>();
            log = loggerFactory.CreateNewLogger(nameof(RunInOwinAndTopshelf));

            var tsh = HostFactory.Run(hc =>
            {
                hc.Service<WebAppRunner>(s =>
                {
                    s.ConstructUsing(name => new WebAppRunner(loggerFactory));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc =>
                    {
                        tc.Stop();
                        tc.Dispose();
                        Container.Dispose();
                    });
                });
                hc.RunAsLocalSystem();

                hc.SetDescription("Arduino Backend");
                hc.SetDisplayName("Arduino Backend");
                hc.SetServiceName("arduinoBackend");
            });

            if (tsh != TopshelfExitCode.Ok)
            {
                if (Environment.UserInteractive)
                    Console.WriteLine("Abnormal Exit. Press a key to close the console.");
                    Console.ReadKey();
            }

            var exitCode = (int)Convert.ChangeType(tsh, tsh.GetTypeCode());
            Environment.ExitCode = exitCode;
        }

        private static void StartupApplication()
        {
            var application = Container.Resolve<ArduinoBackendApplication>();
            application.Start();
        }

        private static void SetupLogging()
        {
#if DEBUG
            Container.Install(new LoggingInstaller_DEBUG());
#else
            Container.Install(new LoggingInstaller_RELEASE());
#endif
        }

        private static IApplication BuildAppComponentsAndApplication()
        {
            var factory = new CastleApplicationFactory(Container);
            return factory
                .WithPostContainerBuildAction(container =>
                {
#if DEBUG
                    container.Install(new ProjectionsInstaller_DEBUG());
#else
                    container.Install(new ProjectionsInstaller_RELEASE());
#endif
                })
                .BuildApplication<ArduinoBackendApplication>();
        }

        private static void RegisterMainComponents()
        {
#if DEBUG
            Container.Install(new ArduinoInstaller_DEBUG());
#else
            Container.Install(new ArduinoInstaller_RELEASE());
#endif
        }
    }
}
