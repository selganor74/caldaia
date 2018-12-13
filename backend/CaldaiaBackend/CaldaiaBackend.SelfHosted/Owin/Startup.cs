using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using CaldaiaBackend.Application;
using CaldaiaBackend.SelfHosted.Owin.IoC;
using Castle.MicroKernel.Registration;
using Infrastructure.Actions;
using Infrastructure.Actions.Command;
using Infrastructure.Actions.Query;
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
    }
}
