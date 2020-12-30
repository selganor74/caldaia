using System;
using System.Collections.Generic;

namespace CaldaiaBackend.SelfHosted.Infrastructure.SignalRLogging
{
    internal class LogEntry
    {
        public string timestamp = DateTime.UtcNow.ToString("o");
        public string componentName { get; set; }
        public string text { get; set; }
        public IEnumerable<string> context { get; set; }
    }

}