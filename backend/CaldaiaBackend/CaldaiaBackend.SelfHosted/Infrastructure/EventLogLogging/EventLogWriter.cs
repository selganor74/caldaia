using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Infrastructure.Logging;
using Newtonsoft.Json;

namespace CaldaiaBackend.SelfHosted.Infrastructure.EventLogLogging
{
    class EventLogWriter : ILogWriter, IDisposable
    {
        private const string EventLogSectionName = "Application";
        private readonly string _applicationName;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private LogLevel _logLevel;

        public EventLogWriter(string applicationName, LogLevel logLevel)
        {
            _applicationName = applicationName;
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            SetLogLevel(logLevel);
        }

        private string BuildEventLogSource(string componentName)
        {
            return _applicationName + "." + componentName;
        }

        private string ComposeMessageAndContext(string toLog, params object[] context)
        {
            var sb = new StringBuilder();
            int threadId;
            try
            {
                threadId = Thread.CurrentThread.ManagedThreadId;
            }
            catch
            {
                threadId = 0;
            }

            string timestamp = DateTime.UtcNow.ToString("o");

            sb.Append(timestamp);
            sb.Append(" [" + threadId + "] ");

            sb.AppendLine(toLog);
            foreach (var o in context)
            {
                sb.AppendLine("Context: ");
                sb.AppendLine($"[{o?.GetType().Name}] " + JsonConvert.SerializeObject(o, Formatting.Indented, _jsonSerializerSettings));
            }

            return sb.ToString();
        }

        public void Trace(string componentName, string toLog, params object[] context)
        {
            if (_logLevel < LogLevel.Trace)
                return;

            using (var eventLog = new EventLog(EventLogSectionName))
            {
                eventLog.Source = BuildEventLogSource(componentName);
                eventLog.WriteEntry(
                    ComposeMessageAndContext(toLog, context), 
                    EventLogEntryType.SuccessAudit, 
                    eventID: 101, 
                    category: 1
                    );
            }
        }

        public void Info(string componentName, string toLog, params object[] context)
        {
            if (_logLevel < LogLevel.Info)
                return;

            using (var eventLog = new EventLog(EventLogSectionName))
            {
                eventLog.Source = BuildEventLogSource(componentName);
                eventLog.WriteEntry(
                    ComposeMessageAndContext(toLog, context),
                    EventLogEntryType.Information,
                    eventID: 101,
                    category: 1
                );
            }
        }

        public void Warning(string componentName, string toLog, params object[] context)
        {
            if (_logLevel < LogLevel.Warning)
                return;

            using (var eventLog = new EventLog(EventLogSectionName))
            {
                eventLog.Source = BuildEventLogSource(componentName);
                eventLog.WriteEntry(
                    ComposeMessageAndContext(toLog, context),
                    EventLogEntryType.Warning,
                    eventID: 101,
                    category: 1
                );
            }
        }

        public void Error(string componentName, string toLog, params object[] context)
        {
            if (_logLevel < LogLevel.Errors)
                return;

            using (var eventLog = new EventLog(EventLogSectionName))
            {
                eventLog.Source = BuildEventLogSource(componentName);
                eventLog.WriteEntry(
                    ComposeMessageAndContext(toLog, context),
                    EventLogEntryType.Error,
                    eventID: 101,
                    category: 1
                );
            }
        }

        public LogLevel SetLogLevel(LogLevel newLevel)
        {
            var previous = _logLevel;
            _logLevel = newLevel;
            return previous;
        }

        public LogLevel GetLogLevel()
        {
            return _logLevel;
        }

        public void Dispose()
        {
        }
    }
}
