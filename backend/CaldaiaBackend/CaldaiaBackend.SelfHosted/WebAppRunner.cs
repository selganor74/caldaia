using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CaldaiaBackend.SelfHosted.Owin;
using Infrastructure.Logging;
using Microsoft.Owin.Hosting;

namespace CaldaiaBackend.SelfHosted
{
    internal class WebAppRunner : IDisposable
    {
        private ILoggerFactory _loggerFactory;
        private ILogger log;
        private IDisposable _webApp;

        public WebAppRunner(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? new NullLoggerFactory();
            log = loggerFactory.CreateNewLogger(GetType().Name);
        }

        public void Start()
        {
            var baseAddress = ConfigurationManager.AppSettings["BaseAddress"];
            

            // Start OWIN host 
            _webApp = WebApp.Start<Startup>(url: baseAddress);
            log.Info($"Application listening on {baseAddress}");
        }

        public void Stop()
        {
            log.Info("Application stopped");
        }

        public void Dispose()
        {
            _webApp?.Dispose();
        }
    }
}
