using System;
using System.Configuration;
using Application.Services;
using ArduinoCommunication;
using CaldaiaBackend.Application;
using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Projections.DataModels;
using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Application.Services.Mocks;
using CaldaiaBackend.Infrastructure;
using CaldaiaBackend.SelfHosted.Infrastructure.SignalRLogging;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Infrastructure.DomainEvents;
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
            RegisterMainComponents();

            BuildAppComponentsAndApplication();

            SetupLogging();

            StartupApplication();

            RunInOwinAndTopshelf();
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
                    s.WhenStopped(tc => tc.Stop());
                });
                hc.RunAsLocalSystem();

                hc.SetDescription("Arduino Backend");
                hc.SetDisplayName("Arduino Backend");
                hc.SetServiceName("arduinoBackend");
            });

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
            var logWriter = Container.Resolve<ILogWriter>();
#if DEBUG
            logWriter.SetLogLevel(LogLevel.Trace);
#else
            logWriter.SetLogLevel(LogLevel.Info);
#endif

            var clw = logWriter as CompositeLogWriter;
            if (clw != null)
            {
#if DEBUG
                var signalrLogWriter = new SignalRLogWriter(LogLevel.Info);
#else
                var signalrLogWriter = new SignalRLogWriter(LogLevel.Warning);
#endif
                clw.AddLogger(signalrLogWriter, LogLevelMode.Independent);
            }
        }

        private static void BuildAppComponentsAndApplication()
        {
            var factory = new CastleApplicationFactory(Container);
            factory.BuildApplication<ArduinoBackendApplication>();
        }

        private static void RegisterMainComponents()
        {
            Container.Register(
                Component
                    .For<IArduinoDataReader, IArduinoCommandIssuer>()
#if DEBUG
                    .ImplementedBy<ArduinoMock>()
#else
                    .ImplementedBy<CaldaiaControllerViaArduino>()
                    .UsingFactoryMethod((kernel) =>
                    {
                        var serialPort = ConfigurationManager.AppSettings["ArduinoComPort"];
                        var controller = new CaldaiaControllerViaArduino(serialPort, kernel.Resolve<IEventDispatcher>(), kernel.Resolve<ILoggerFactory>());
                        controller.Start();
                        return controller;
                    })
#endif
                    .LifestyleSingleton(),

                Component
                    .For<ITimeSlotBufferLoaderSaver<AccumulatorStatistics>>()
#if DEBUG
                    .ImplementedBy<InMemoryTimeBufferLoaderSaver<AccumulatorStatistics>>()
#else
                    .ImplementedBy<FileSystemTimeSlotLoaderSaver<AccumulatorStatistics>>()
                    .DependsOn(
                        Dependency
                            .OnAppSettingsValue("PathToJsonStorageFile", "PathToLast24HoursJson")
                        )
#endif
                    .LifestyleTransient(),

                Component
                    .For<Last24Hours>()
                    .LifestyleSingleton()
            );
        }
    }
}
