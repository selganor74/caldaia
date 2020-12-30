using System;
using System.Collections.Generic;
using Infrastructure.Logging;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace CaldaiaBackend.SelfHosted.Infrastructure.SignalRLogging
{
    class SignalRLogWriter : ILogWriter
    {
        private LogLevel _logLevel;
        private dynamic LogNotifier => GlobalHost.ConnectionManager.GetHubContext("logs").Clients.All;

        public SignalRLogWriter(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void Trace(string componentName, string toLog, params object[] context)
        {
            if (_logLevel < LogLevel.Trace) return;

            var toSend = ContextToJson(context);

            var logEntry = BuildLogEntry(componentName, toLog, toSend);

            LogNotifier.Trace(logEntry);
        }

        public void Info(string componentName, string toLog, params object[] context)
        {
            if (_logLevel < LogLevel.Info) return;

            var toSend = ContextToJson(context);

            var logEntry = BuildLogEntry(componentName, toLog, toSend);

            LogNotifier.Info(logEntry);
        }

        public void Warning(string componentName, string toLog, params object[] context)
        {
            if (_logLevel < LogLevel.Warning) return;

            var toSend = ContextToJson(context);

            var logEntry = BuildLogEntry(componentName, toLog, toSend);

            LogNotifier.Warning(logEntry);
        }

        public void Error(string componentName, string toLog, params object[] context)
        {
            if (_logLevel < LogLevel.Error) return;

            var toSend = ContextToJson(context);

            var logEntry = BuildLogEntry(componentName, toLog, toSend);

            LogNotifier.Error(logEntry);
        }

        public LogLevel SetLogLevel(LogLevel newLevel)
        {
            var prev = _logLevel;
            _logLevel = newLevel;
            return prev;
        }

        public LogLevel GetLogLevel()
        {
            return _logLevel;
        }
        private static List<string> ContextToJson(object[] context)
        {
            var toSend = new List<string>();
            foreach (var ce in context)
            {
                try
                {
                    var ceAsString = JsonConvert.SerializeObject(ce);
                    toSend.Add(ceAsString);
                }
                catch (Exception)
                {
                    /* Nothing to do here ...*/
                }
            }

            return toSend;
        }
        private static LogEntry BuildLogEntry(string componentName, string toLog, List<string> toSend)
        {
            return new LogEntry
            {
                componentName = componentName,
                context = toSend,
                text = toLog
            };
        }

    }
}
