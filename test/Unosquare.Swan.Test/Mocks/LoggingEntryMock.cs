using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan.Test.Mocks
{
    public class LoggingEntryMock
    {
        public LogMessageType Type { get; set; }

        public DateTime DateTime { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }

        public IDictionary<string, object> Properties { get; set; }
    }
}
