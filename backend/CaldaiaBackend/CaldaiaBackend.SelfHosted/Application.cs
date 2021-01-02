using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using CaldaiaBackend.Application;
using CaldaiaBackend.Application.Services;
using Castle.Windsor;
using Hangfire;
using Hangfire.MemoryStorage;
using Infrastructure.Application;
using Microsoft.AspNet.SignalR;
using Owin;

using Infrastructure.Events;
using Infrastructure.Logging;
using Infrastructure.Application.Hosting.WebApi.SelfHost;

using CaldaiaBackend.SelfHosted.Hangfire;
using CaldaiaBackend.SelfHosted.Infrastructure.Notification;
using CaldaiaBackend.SelfHosted.Installers;
using CaldaiaBackend.SelfHosted.IoC;
using CaldaiaBackend.SelfHosted.Owin;
using Castle.MicroKernel.Registration;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

namespace CaldaiaBackend.SelfHosted
{
    /// <summary>
    /// In this class you will complete the configuration of the container and
	/// the WebApiAppRunner.
	/// 
	/// In this file you may want to review the "ContainerSetup" and the "WebApiSetup" 
	/// methods to configure the whole application.
    /// </summary>
    public class Application : IApplication, IDisposable
    {
        private readonly IWindsorContainer _container;
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private WebApiAppRunner runner;

        private readonly Environments _environment;
        private readonly Config _config;

        public Application(
            IWindsorContainer container,
            Environments environment,
            Config config
            )
        {
            _environment = environment;
            _config = config;
            _container = container;

            try
            {
                loggerFactory = _container.Resolve<ILoggerFactory>();
            }
            catch
            {
                loggerFactory = new NullLoggerFactory();
            }
            logger = loggerFactory.CreateNewLogger(Assembly.GetExecutingAssembly().GetName().Name);
        }

        /// <summary>
        /// The entrypoint for the application
        /// </summary>
        public void Start()
        {
            ContainerSetup();

            var webAppOptions = WebApiSetup();
            StartWebApplication(webAppOptions);
        }

        /// <summary>
        /// Finish application composition
        /// </summary>
        private void ContainerSetup()
        {
            _container.Install(new InfrastructureInstaller(enableAuth: false));
            switch (_environment)
            {
                case Environments.DEBUG:
                case Environments.TEST:
                    _container.Install(new LoggingInstaller_DEBUG());
                    _container.Install(new ProjectionsInstaller_DEBUG());
                    _container.Install(new ArduinoInstaller_DEBUG());
                    break;
                case Environments.RELEASE:
                    _container.Install(new LoggingInstaller_RELEASE());
                    _container.Install(new SmtpNotifierInstaller_RELEASE());
                    _container.Install(new ProjectionsInstaller_RELEASE());
                    _container.Install(new ArduinoInstaller_RELEASE());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _container.Register(
                Component
                    .For<INotificationPublisher, INotificationSubscriber>()
                    .ImplementedBy<InMemoryNotificationService>()
                    .LifestyleSingleton(),

                Component
                    .For<SignalRNotificationAdapter>()
                    .ImplementedBy<SignalRNotificationAdapter>()
                    .LifestyleSingleton(),

                Component
                    .For<ArduinoBackendApplication>()
                    .ImplementedBy<ArduinoBackendApplication>()
                    .LifestyleSingleton()
                );

            _container.CheckConfiguration();
        }

        /// <summary>
        /// Creates the SelfHostWebApiOptions object that will be used to
        /// run the application.
        /// </summary>
        private SelfHostWebApiOptions WebApiSetup()
        {
            var webAppOptions = new SelfHostWebApiOptions
            {
                DeployUrl = _config.deployUrl,
                ListenOnPort = _config.port,
                BaseUrl = _config.baseUrl,
                EnableWebApiTracing = false,
                EnableHttpLogging = false,

                // TokenIssuer = _config.authTokenIssuer,
                // TokenScope = "urn:gl-services-infrastructure",

                EnableAutoApi = false,
                AutoApiEnableAuthorize = false, // _config.enableAuth,
                AutoApiRoutePrefix = "infrastructure/api",

                CustomWebApiConfig = httpConfiguration =>
                {
                    httpConfiguration.UseCastleWindsor(_container);
                },

                CustomOwinConfig = appBuilder =>
                {
                    SetupSignalR(appBuilder, _config.deployUrl);

                    ConfigureHangfire(appBuilder, _config.deployUrl);

                    SetupHangfireJobs();

                    SetupFrontend(appBuilder);
                }
            };
            return webAppOptions;
        }

        private void SetupFrontend(IAppBuilder appBuilder)
        {
            var frontendPhysicalPath = "";

            switch (_environment)
            {
                case Environments.TEST:
                case Environments.DEBUG:
                    frontendPhysicalPath = "../../AngularAppDist/caldaia-frontend";
                    break;
                case Environments.RELEASE:
                    frontendPhysicalPath = "./AngularAppDist/caldaia-frontend";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!Directory.Exists(frontendPhysicalPath))
                throw new InvalidOperationException($"frontend path not found {frontendPhysicalPath}");

            appBuilder.UseStaticFiles(new StaticFileOptions
            {
                FileSystem = new PhysicalFileSystem(frontendPhysicalPath),
                RequestPath = new PathString("/app"),
                ServeUnknownFileTypes = true
            });
        }

        private void SetupHangfireJobs()
        {
            // SETUP YOUR SCHEDULED JOBS HERE //
            // RegisterHangfireJob<SomeJob>(j => j.Execute(), Cron.Never);
        }


        private void StartWebApplication(SelfHostWebApiOptions webAppOptions)
        {
            var dispatcher = _container.Resolve<IEventDispatcher>();

            logger.Warning($"{Environment.NewLine}Starting in {_environment} environment with config:{Environment.NewLine}{_config}");

            var arduinoApplication = _container.Resolve<ArduinoBackendApplication>();
            arduinoApplication.Start();

            runner = new WebApiAppRunner(
                webAppOptions,
                dispatcher,
                loggerFactory
            );
            runner.Start();

            logger.Info("Started!");
        }

        public void Stop()
        {
            runner?.Stop();
            logger.Info($"{Assembly.GetExecutingAssembly().GetName().Name} Stopped!");
        }

        private void ConfigureHangfire(IAppBuilder appBuilder, string deployUrl)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();
            GlobalConfiguration.Configuration.UseLogProvider(new InfrastructureLogProvider(loggerFactory, LogLevel.Warning));
            GlobalConfiguration.Configuration.UseActivator(new CastleJobActivator(_container));

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete });
            GlobalJobFilters.Filters.Add(new DisableConcurrentExecutionAttribute(1));
            var options = new BackgroundJobServerOptions
            {
                WorkerCount = 1
            };

            const string hangfireUri = "/hangfire";

            appBuilder.UseHangfireServer(options);
            appBuilder.UseHangfireDashboard(hangfireUri);
            logger.Info($"Hangfire dashboard available at {deployUrl}{hangfireUri}");
        }

        private void SetupSignalR(IAppBuilder appBuilder, string deployUrl)
        {
            const string signalrBase = "/signalr"; // <- MUST contain the leading slash !!!

            var signalrNotificationAdapter = _container.Resolve<SignalRNotificationAdapter>();
            signalrNotificationAdapter.Start();

            appBuilder.MapSignalR(signalrBase, new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            });
            logger.Info($"SignalR started on {deployUrl}{signalrBase}");
        }

        private void RegisterHangfireJob<TJob>(Expression<Action<TJob>> methodCall, Func<string> schedule)
        {
            RecurringJob.AddOrUpdate(typeof(TJob).Name, methodCall, schedule, TimeZoneInfo.Local);
            logger.Info($"Registered job [{typeof(TJob).Name,-25}] @ {schedule()}");
        }

        public void Dispose()
        {
            runner?.Stop();
        }
    }
}