using CaldaiaBackend.SelfHosted.Infrastructure.EventLogLogging;
using CaldaiaBackend.SelfHosted.Infrastructure.SignalRLogging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Logging;

namespace CaldaiaBackend.SelfHosted.IoC
{
    class LoggingInstaller_RELEASE : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var logWriter = container.Resolve<ILogWriter>();

            var clw = logWriter as CompositeLogWriter;
            if (clw == null) return;

            var signalrLogWriter = new SignalRLogWriter(LogLevel.Warning);
            clw.AddLogger(signalrLogWriter, LogLevelMode.Synchronized);

            //var eventLogLogger = new EventLogWriter("arduinoBackend", LogLevel.Warning);
            //clw.AddLogger(eventLogLogger, LogLevelMode.Independent);

            logWriter.SetLogLevel(LogLevel.Info);
        }
    }
}