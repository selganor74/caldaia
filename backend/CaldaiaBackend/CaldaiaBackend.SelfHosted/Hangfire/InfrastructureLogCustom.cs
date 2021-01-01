using System;
using Hangfire.Logging;
using Infrastructure.Logging;
using InfrastructureLogLevel = Infrastructure.Logging.LogLevel;
using LogLevel = Hangfire.Logging.LogLevel;

namespace CaldaiaBackend.SelfHosted.Hangfire
{
    internal class InfrastructureLogCustom : ILog
    {
        private readonly ILogger _log;
        private readonly InfrastructureLogLevel _logLevel;

        public InfrastructureLogCustom(ILogger log, InfrastructureLogLevel logLevel)
        {
            _log = log;
            _logLevel = logLevel;
        }

        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
        {
            // this way all loglevels are enabled
            if (messageFunc == null && exception == null)
            {
                return isLogLevelEnabled(logLevel);
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    {
                        if (exception == null)
                            _log.Trace(messageFunc?.Invoke());
                        else
                            _log.Trace(messageFunc?.Invoke(), exception);

                        break;
                    }
                case LogLevel.Info:
                    {
                        if (exception == null)
                            _log.Info(messageFunc?.Invoke());
                        else
                            _log.Info(messageFunc?.Invoke(), exception);

                        break;
                    }
                case LogLevel.Warn:
                    {
                        if (exception == null)
                            _log.Warning(messageFunc?.Invoke());
                        else
                            _log.Warning(messageFunc?.Invoke(), exception);

                        break;
                    }
                case LogLevel.Error:
                case LogLevel.Fatal:
                    {
                        if (exception == null)
                            _log.Trace(messageFunc?.Invoke());
                        else
                            _log.Trace(messageFunc?.Invoke(), exception);

                        break;
                    }

            }
            return true;
        }

        private bool isLogLevelEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    {
                        return _logLevel >= InfrastructureLogLevel.Trace;
                    }
                case LogLevel.Info:
                    {
                        return _logLevel >= InfrastructureLogLevel.Info;
                    }
                case LogLevel.Warn:
                    {
                        return _logLevel >= InfrastructureLogLevel.Warning;
                    }
                case LogLevel.Error:
                case LogLevel.Fatal:
                    {
                        return _logLevel >= InfrastructureLogLevel.Error;
                    }
            }

            return false;
        }
    }
}
