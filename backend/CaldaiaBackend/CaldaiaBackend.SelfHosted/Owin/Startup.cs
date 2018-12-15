using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using CaldaiaBackend.Application;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.SelfHosted.Owin.IoC;
using Castle.MicroKernel.Registration;
using Infrastructure.Actions;
using Infrastructure.Logging;
using Infrastructure.MiscPatterns.Notification;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace CaldaiaBackend.SelfHosted.Owin
{
    public class Startup
    {
        private static ILogger _log;

        public Startup()
        {
            _log = Program.Container.Resolve<ILoggerFactory>().CreateNewLogger(GetType().Name);
        }
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            try
            {
                // Configure Web API for self-host. 
                HttpConfiguration config = new HttpConfiguration();

                Program.Container.Register(
                    Classes.FromAssemblyContaining<Startup>()
                        .BasedOn<ApiController>()
                        .LifestyleTransient(),

                    Classes.FromAssemblyContaining<ArduinoBackendApplication>()
                        .BasedOn<IAction>()
                        .WithServiceAllInterfaces()
                        .LifestyleSingleton()
                    );
                // Setup IoC 
                config.Services.Replace(
                    typeof(IHttpControllerActivator),
                    new WindsorControllerActivator(Program.Container));

                // Setup routing
                //config.Routes.MapHttpRoute(
                //    name: "backend",
                //    routeTemplate: "api/{controller}/{id}",
                //    defaults: new { id = RouteParameter.Optional }
                //);

                config.MapHttpAttributeRoutes();

                config.Formatters.Remove(config.Formatters.XmlFormatter);

                config.EnsureInitialized();

                appBuilder.UseCors(CorsOptions.AllowAll);

                appBuilder.UseStaticFiles(new StaticFileOptions
                {
                    FileSystem = new PhysicalFileSystem("./AngularAppDist/caldaia-frontend"),
                    RequestPath = new Microsoft.Owin.PathString("/app"),
                    ServeUnknownFileTypes = true
                });

                SetupSignalR(appBuilder);
                appBuilder.UseWebApi(config);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("*** PRESS A KEY TO EXIT ***");
                Console.ReadKey();
                throw;
            }

        }

        private static void SetupSignalR(IAppBuilder appBuilder)
        {
            var appObservable = Program.Container.Resolve<INotificationSubscriber>();

            appObservable.Subscribe("data", (DataFromArduino data) => { NotifyToChannel("data", data); });
            appObservable.Subscribe("settings", (SettingsFromArduino settings) => { NotifyToChannel("settings", settings); });

            appBuilder.MapSignalR("/signalr", new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            });
        }

        public static void NotifyToChannel<T>(string channel, T data)
        {
            try
            {
                var channelNotifier = GlobalHost.ConnectionManager.GetHubContext(channel);
                channelNotifier?.Clients.All.notify(data);
            }
            catch (Exception e)
            {
                _log.Warning("Errors in Hub.", e);
            }
        }
    }
}
