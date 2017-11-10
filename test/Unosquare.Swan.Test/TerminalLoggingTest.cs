using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.TerminalLoggingTests
{
    [TestFixture]
    public class OnLogMessageReceived
    {
        [Test]
        public void Logging()
        {
            Terminal.Flush();

            var messages = new List<LoggingEntryMock>();

            Terminal.OnLogMessageReceived += (s, e) =>
            {
                messages.Add(new LoggingEntryMock
                {
                    DateTime = e.UtcDate,
                    Exception = e.Exception,
                    Message = e.Message,
                    Source = e.Source,
                    Type = e.MessageType,
                    ExtendedData = e.ExtendedData
                });
            };

            nameof(LogMessageType.Info).Info();
            nameof(LogMessageType.Debug).Debug();
            nameof(LogMessageType.Error).Error();
            nameof(LogMessageType.Trace).Trace();
            nameof(LogMessageType.Warning).Warn();

            Task.Delay(200).Wait();

            Assert.IsTrue(messages.All(x => x.Message == x.Type.ToString()));

            new Exception().Error(nameof(OnLogMessageReceived), nameof(Logging));
            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.Exception != null));
            Assert.IsTrue(messages.Any(x => x.Source == nameof(OnLogMessageReceived)));
            Assert.AreEqual(nameof(Logging), messages.First(x => x.Source == nameof(OnLogMessageReceived)).Message);

            messages.Clear();
            nameof(LogMessageType.Info).Info("Test", 1);
            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(1, messages.First(x => x.ExtendedData != null).ExtendedData);
            Assert.AreEqual(nameof(LogMessageType.Info), messages.First(x => x.ExtendedData != null).Message);
        }
    }

    [TestFixture]
    public class Log
    {
        [Test]
        public void Message_MessageLogged()
        {
            Terminal.Flush();

            var messages = new List<LoggingEntryMock>();

            Terminal.OnLogMessageReceived += (s, e) =>
            {
                messages.Add(new LoggingEntryMock
                {
                    DateTime = e.UtcDate,
                    Exception = e.Exception,
                    Message = e.Message,
                    Source = e.Source,
                    Type = e.MessageType,
                    ExtendedData = e.ExtendedData
                });
            };

            Task.Delay(200).Wait();
            new Exception().Error(nameof(Log), nameof(Message_MessageLogged));
            Task.Delay(150).Wait();
            
            messages.Clear();
            nameof(LogMessageType.None).Log("Unosquare Labs", LogMessageType.None, 1);
            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(1, messages.First(x => x.ExtendedData != null).ExtendedData);
            Assert.AreEqual(nameof(LogMessageType.None), messages.First(x => x.ExtendedData != null).Message);
        }
    }

    [TestFixture]
    public class Debug
    {
        [Test]
        public void MessageWithException_MessageLogged()
        {
            Terminal.Flush();

            var messages = new List<LoggingEntryMock>();
            var extendedDataExpected = "System.Exception: Exception of type 'System.Exception' was thrown.";

            Terminal.OnLogMessageReceived += (s, e) =>
            {
                messages.Add(new LoggingEntryMock
                {
                    DateTime = e.UtcDate,
                    Exception = e.Exception,
                    Message = e.Message,
                    Source = e.Source,
                    Type = e.MessageType,
                    ExtendedData = e.ExtendedData
                });
            };

            new Exception().Debug("Test", "ArCiGo");

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual("ArCiGo", messages.First(x => x.ExtendedData != null).Message);
            Assert.AreEqual(extendedDataExpected, messages.First(x => x.ExtendedData != null).ExtendedData.ToString());

        }
    }
}
