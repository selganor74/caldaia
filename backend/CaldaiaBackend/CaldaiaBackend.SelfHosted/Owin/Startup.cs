//using System;
//using System.Web.Http;
//using System.Web.Http.Dispatcher;
//using CaldaiaBackend.Application;
//using CaldaiaBackend.SelfHosted.Owin.IoC;
//using Castle.MicroKernel.Registration;
//using Infrastructure.Actions;
//using Infrastructure.Logging;
//using Microsoft.Owin.Cors;
//using Microsoft.Owin.FileSystems;
//using Microsoft.Owin.StaticFiles;
//using Owin;

//namespace CaldaiaBackend.SelfHosted.Owin
//{
//    public class Startup : IDisposable
//    {
//        private static ILogger _log;
//        private SignalRNotificationAdapter _signalRNotificationHub;

//        public Startup()
//        {
//            Program.Container.Register(
//                Classes.FromAssemblyContaining<Startup>()
//                    .BasedOn<ApiController>()
//                    .LifestyleTransient(),

//                Classes.FromAssemblyContaining<ArduinoBackendApplication>()
//                    .BasedOn<IAction>()
//                    .WithServiceAllInterfaces()
//                    .LifestyleSingleton(),

//                Component
//                    .For<SignalRNotificationAdapter>()
//                    .ImplementedBy<SignalRNotificationAdapter>()
//                    .LifestyleSingleton()

//            );

//            _log = Program.Container.Resolve<ILoggerFactory>().CreateNewLogger(GetType().Name);
//            _signalRNotificationHub = Program.Container.Resolve<SignalRNotificationAdapter>();
//        }
//        // This code configures Web API. The Startup class is specified as a type
//        // parameter in the WebApp.Start method.
//        public void Configuration(IAppBuilder appBuilder)
//        {
//            try
//            {
//                appBuilder.UseCors(CorsOptions.AllowAll);

//                // Configure Web API for self-host. 
//                HttpConfiguration config = new HttpConfiguration();

//                // Setup IoC 
//                config.Services.Replace(
//                    typeof(IHttpControllerActivator),
//                    new WindsorControllerActivator(Program.Container));

//                config.MapHttpAttributeRoutes();

//                config.Formatters.Remove(config.Formatters.XmlFormatter);

//                config.EnsureInitialized();

//                appBuilder.UseStaticFiles(new StaticFileOptions
//                {
//                    FileSystem = new PhysicalFileSystem("./AngularAppDist/caldaia-frontend"),
//                    RequestPath = new Microsoft.Owin.PathString("/app"),
//                    ServeUnknownFileTypes = true
//                });

//                _signalRNotificationHub.SetupSignalR(appBuilder);
//                appBuilder.UseWebApi(config);
//            }
//            catch (Exception e)
//            {
//                if (Environment.UserInteractive)
//                {
//                    Console.WriteLine(e);
//                    Console.WriteLine("*** PRESS A KEY TO EXIT ***");
//                    Console.ReadKey();
//                }
//                throw;
//            }
//        }

//        public void Dispose()
//        {
//        }
//    }
//}
