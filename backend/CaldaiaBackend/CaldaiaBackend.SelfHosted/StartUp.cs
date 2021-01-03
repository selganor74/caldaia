using System;
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Newtonsoft.Json;
using Infrastructure.Application.Hosting.WebApi.SelfHost.Topshelf;
using Infrastructure.Logging;
using CaldaiaBackend.SelfHosted.Installers;
using Topshelf;
using Topshelf.HostConfigurators;

namespace CaldaiaBackend.SelfHosted
{
    /// <summary>
    /// The entry point for the application.
    /// Uses TopShelf as Host to easily install the application as a service in
    /// windows environments.
	/// 
	/// In this file you may want to review the "SetupLogging" to configure the Logging
	/// infrastructure.
    /// </summary>
    public class StartUp
    {
#if DEBUG
        private static readonly Environments ENVIRONMENT = Environments.DEBUG;
#endif
#if TEST
		/* If you get an error stating ENVIRONMENT is not defined, please add 
		   "TEST" (without quotes) to the "Conditional compilation symbols" 
		   for the "Test" Configuration, in the "Build" section under your 
		   startup project's Properties. If the "Test" configuration doesn't
		   exist, you may want to create it !								
		*/
		private static readonly Environments ENVIRONMENT = Environments.TEST;
#endif
#if RELEASE
		/* If you get an error stating ENVIRONMENT is not defined, please add 
		   "RELEASE" (without quotes) to the "Conditional compilation symbols" 
		   for the "Release" Configuration, in the "Build" section under your 
		   startup project's Properties.                                    
		*/
        private static readonly Environments ENVIRONMENT = Environments.RELEASE;
#endif

        private static IWindsorContainer container;
        private static ILogger log;

		/*
		IMPORTANT:	If you decide to use this StartUp file, please remember to 
					delete or comment default startup class in Program.cs
		*/
        public static void Main(params string[] param)
        {
            using (container = new WindsorContainer())
            {
                var config = ReadConfigFromJson(ENVIRONMENT);
                var logLevel = LogLevel.Trace;

                container.Register(
                    Component
                        .For<Config>()
                        .Instance(config),

                    Component
                        .For<IWindsorContainer>()
                        .Instance(container),

                    Component
                        .For<TopshelfServiceRunner<Application>>()
                        .DependsOn(Dependency.OnValue<LogLevel>(logLevel))
                        .DependsOn(Dependency.OnValue<Action<HostConfigurator>>(
                            new Action<HostConfigurator>(hc => {
                                // The default configuration provided by TopShelfServiceRunner is suitable for most situations,
                                // but if you need some advanced feature from topshelf, you can customize here the HostConfiguration.
                                // Whatever you do here will take precedence over default config, and you can check what the default does
                                // here: https://git.loccioni.com/IT/Loccioni.Infrastructure/-/blob/master/Infrastructure.Application.Hosting.WebApi.SelfHost.Topshelf/TopshelfServiceRunner.cs#L51
                                hc.EnableServiceRecovery(sr =>
                                {
                                    sr.RestartService(delayInMinutes: 1);
                                    sr.RestartService(delayInMinutes: 1);
                                    sr.RestartService(delayInMinutes: 1);

                                    sr.SetResetPeriod(days: 1);
                                });
                            }))),

                    Component
                        .For<Application>()
                        .DependsOn(
                            Dependency.OnValue<Environments>(ENVIRONMENT),
                            Dependency.OnValue<LogLevel>(logLevel)
                            )
                        .LifestyleSingleton()
                    );

                // Sets up minimal infrastructure needs
                container.Install(new InfrastructureLoggingInstaller(logLevel));

                // Resolve the application and start !
                var serviceRunner = container.Resolve<TopshelfServiceRunner<Application>>();
                serviceRunner.Start();
            }
        }

        private static Config ReadConfigFromJson(Environments env)
        {
            var envSubstring = env.ToString();
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var asText = File.ReadAllText(Path.Combine(basePath, $@"config.{envSubstring}.json"));
            var toReturn = JsonConvert.DeserializeObject<Config>(asText);
            return toReturn;
        }
    }
}