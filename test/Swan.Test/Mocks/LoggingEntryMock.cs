using Swan.Logging;
using System;

namespace Swan.Test.Mocks
{
    public class LoggingEntryMock
    {
        public LogLevel Type { get; set; }

        public DateTime DateTime { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }

        public object ExtendedData { get; set; }
    }
}