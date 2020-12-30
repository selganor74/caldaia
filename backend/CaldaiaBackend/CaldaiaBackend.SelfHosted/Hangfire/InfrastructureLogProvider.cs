using Hangfire.Logging;
using Infrastructure.Logging;
using LogLevel = Infrastructure.Logging.LogLevel;

namespace CaldaiaBackend.SelfHosted.Hangfire
{
    internal class InfrastructureLogProvider : ILogProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly LogLevel _logLevel;

        public InfrastructureLogProvider(ILoggerFactory loggerFactory, LogLevel logLevel = LogLevel.Info)
        {
            _loggerFactory = loggerFactory;
            _logLevel = logLevel;
        }

        public ILog GetLogger(string name)
        {
            return new InfrastructureLog(_loggerFactory.CreateNewLogger(name), _logLevel);
        }
    }

}
