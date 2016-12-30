using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class TerminalTest
    {
        [Test]
        public void IsConsolePresentTest()
        {
            if (CurrentApp.OS == OperatingSystem.Windows)
            {
                // Funny, the console is not here :P
                Assert.IsFalse(Terminal.IsConsolePresent);
            }
            else
            {
                Assert.IsTrue(Terminal.IsConsolePresent);
            }
        }

        [Test]
        public void LoggingTest()
        {
            var messages = new List<LoggingEntryMock>();

            Terminal.OnLogMessageReceived += (s, e) =>
            {
                messages.Add(new LoggingEntryMock
                {
                    DateTime = e.UtcDate,
                    Exception = e.Exception,
                    Message = e.Message,
                    Source = e.Source,
                    Type = e.MessageType
                });
            };

            nameof(LogMessageType.Info).Info();
            nameof(LogMessageType.Debug).Debug();
            nameof(LogMessageType.Error).Error();
            nameof(LogMessageType.Trace).Trace();
            nameof(LogMessageType.Warning).Warn();

            Task.Delay(150).Wait();
            Assert.IsTrue(messages.All(x => x.Message == x.Type.ToString()));

            new Exception().Error(nameof(TerminalTest), nameof(LoggingTest));
            Task.Delay(150).Wait();

            Assert.IsTrue(messages.Any(x => x.Exception != null));
            Assert.IsTrue(messages.Any(x => x.Source == nameof(TerminalTest)));
            Assert.AreEqual($"[{nameof(TerminalTest)}] {nameof(LoggingTest)}", messages.First(x => x.Source == nameof(TerminalTest)).Message);
        }
    }
}