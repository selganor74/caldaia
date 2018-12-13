using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using CaldaiaBackend.Application;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.SelfHosted.Owin.IoC;
using CaldaiaBackend.SelfHosted.Owin.SignalR;
using Castle.MicroKernel.Registration;
using Infrastructure.Actions;
using Infrastructure.Actions.Command;
using Infrastructure.Actions.Query;
using Infrastructure.MiscPatterns.Notification;
using Microsoft.AspNet.SignalR;
using Owin;

namespace CaldaiaBackend.SelfHosted.Owin
{
    public class Startup
    {
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

                SetupSignalR(appBuilder);

                config.Formatters.Remove(config.Formatters.XmlFormatter);

                config.EnsureInitialized();

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
            Program.Container.Register(
                Component
                    .For<NotificationHub>()
                    .ImplementedBy<NotificationHub>()
                    .LifestyleSingleton()
                );

            var signalrHub = Program.Container.Resolve<NotificationHub>();
            var appObservable = Program.Container.Resolve<INotificationSubscriber>();

            appObservable.Subscribe("data", (DataFromArduino data) => { NotificationHub.NotifyToChannel("data", data); });
            appObservable.Subscribe("settings", (SettingsFromArduino settings) => { NotificationHub.NotifyToChannel("settings", settings); });

            appBuilder.MapSignalR("/signalr", new HubConfiguration()
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            });
        }
    }
}
