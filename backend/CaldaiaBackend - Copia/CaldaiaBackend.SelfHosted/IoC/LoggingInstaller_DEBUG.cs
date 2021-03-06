﻿using CaldaiaBackend.SelfHosted.Infrastructure.SignalRLogging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Logging;

namespace CaldaiaBackend.SelfHosted.IoC
{
    class LoggingInstaller_DEBUG : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var logWriter = container.Resolve<ILogWriter>();
            logWriter.SetLogLevel(LogLevel.Trace);

            var clw = logWriter as CompositeLogWriter;
            if (clw == null) return;

            var signalrLogWriter = new SignalRLogWriter(LogLevel.Info);
            clw.AddLogger(signalrLogWriter, LogLevelMode.Independent);
        }
    }
}