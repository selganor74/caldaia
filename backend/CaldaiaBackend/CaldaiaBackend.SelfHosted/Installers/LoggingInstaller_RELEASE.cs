using CaldaiaBackend.SelfHosted.Infrastructure.SignalRLogging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Logging;
using Infrastructure.Logging.Implementations;

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
            clw.AddLogger(signalrLogWriter, CompositeLogLevelMode.Synchronized);

            //var eventLogLogger = new EventLogWriter("arduinoBackend", LogLevel.Warning);
            //clw.AddLogger(eventLogLogger, LogLevelMode.Independent);

            logWriter.SetLogLevel(LogLevel.Info);
        }
    }
}