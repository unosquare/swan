namespace Unosquare.Swan.Test.TerminalLoggingTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Mocks;

    public abstract class TerminalLoggingTest : TestFixtureBase
    {
        protected string extendedDataExpected = "System.Exception: Exception of type 'System.Exception' was thrown.";

        [SetUp]
        public void SetupLoggingMessages()
        {
            Terminal.Flush();
        }

        protected void InitLog(List<LoggingEntryMock> messages)
        {
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
        }
    }

    [TestFixture]
    public class OnLogMessageReceived : TerminalLoggingTest
    {
        [Test]
        public void Logging()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

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
    public class Log : TerminalLoggingTest
    {
        [Test]
        public void Message_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);
            
            nameof(LogMessageType.None).Log("Unosquare Labs", LogMessageType.None, 1);

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(1, messages.First(x => x.ExtendedData != null).ExtendedData);
            Assert.AreEqual(nameof(LogMessageType.None), messages.First(x => x.ExtendedData != null).Message);
        }

        [Test]
        public void MessageWithType_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            nameof(LogMessageType.None).Log(typeof(string), LogMessageType.None, 1);

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(1, messages.First(x => x.ExtendedData != null).ExtendedData);
            Assert.AreEqual(nameof(LogMessageType.None), messages.First(x => x.ExtendedData != null).Message);
        }

        [Test]
        public void MessageWithException_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);
            
            new Exception().Log(typeof(string));

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.IsNotNull(messages.First(x => x.ExtendedData != null).Message);
        }
    }

    [TestFixture]
    public class Debug : TerminalLoggingTest
    {
        [Test]
        public void MessageWithException_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            new Exception().Debug("Unosquare Américas", nameof(Debug));

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(nameof(Debug), messages.First(x => x.ExtendedData != null).Message);
            Assert.AreEqual(extendedDataExpected, messages.First(x => x.ExtendedData != null).ExtendedData.ToString());

        }
    }

    [TestFixture]
    public class Trace : TerminalLoggingTest
    {
        [Test]
        public void MessageWithException_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            new Exception().Trace("Unosquare Américas", nameof(Trace));

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(nameof(Trace), messages.First(x => x.ExtendedData != null).Message);
            Assert.AreEqual(extendedDataExpected, messages.First(x => x.ExtendedData != null).ExtendedData.ToString());
        }

        [Test]
        public void MessageWithType_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            nameof(LogMessageType.Trace).Trace(typeof(string), 1);
            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(1, messages.First(x => x.ExtendedData != null).ExtendedData);
            Assert.AreEqual(nameof(LogMessageType.Trace), messages.First(x => x.ExtendedData != null).Message);
        }
    }

    [TestFixture]
    public class Warn : TerminalLoggingTest
    {
        [Test]
        public void MessageWithType_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            nameof(LogMessageType.Trace).Warn(typeof(string), 1);

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(1, messages.First(x => x.ExtendedData != null).ExtendedData);
            Assert.AreEqual(nameof(LogMessageType.Trace), messages.First(x => x.ExtendedData != null).Message);
        }

        [Test]
        public void MessageWithException_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            new Exception().Warn("Unosquare Américas", nameof(Warn));

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(nameof(Warn), messages.First(x => x.ExtendedData != null).Message);
            Assert.AreEqual(extendedDataExpected, messages.First(x => x.ExtendedData != null).ExtendedData.ToString());
        }
    }

    [TestFixture]
    public class Info : TerminalLoggingTest
    {
        [Test]
        public void MessageWithException_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            new Exception().Info("Unosquare Américas", nameof(Info));

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(nameof(Info), messages.First(x => x.ExtendedData != null).Message);
            Assert.AreEqual(extendedDataExpected, messages.First(x => x.ExtendedData != null).ExtendedData.ToString());
        }
    }

    [TestFixture]
    public class Error : TerminalLoggingTest
    {
        [Test]
        public void MessageWithType_MessageLogged()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            nameof(LogMessageType.Error).Error(typeof(string), 1);

            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.ExtendedData != null));
            Assert.AreEqual(1, messages.First(x => x.ExtendedData != null).ExtendedData);
            Assert.AreEqual(nameof(LogMessageType.Error), messages.First(x => x.ExtendedData != null).Message);
        }
    }

    [TestFixture]
    public class Dump : TerminalLoggingTest
    {
        [Test]
        public void NullObject_ReturnsNothing()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            NullObj.Dump(typeof(string).Name);

            Assert.IsFalse(messages.Any(x => x.ExtendedData != null));
        }

        [Test]
        public void NullObjectAcceptingType_ReturnsNothing()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            NullObj.Dump(typeof(string));

            Assert.IsFalse(messages.Any(x => x.ExtendedData != null));
        }

        [Test]
        public void NotNullObjectAcceptingType_ReturnsNothing()
        {
            var messages = new List<LoggingEntryMock>();
            InitLog(messages);

            nameof(Dump).Dump(typeof(string).Name);

            Task.Delay(150).Wait();

            Assert.AreEqual(nameof(Dump), messages.Last(x => x.ExtendedData != null).ExtendedData);
            Assert.AreEqual(typeof(string).Name, messages.Last(x => x.ExtendedData != null).Source);
        }
    }
}