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

            Terminal.Settings.OnMessageLogged = (s, t, d, src, m, ex) =>
            {
                messages.Add(new LoggingEntryMock
                {
                    DateTime = d,
                    Exception = ex,
                    Message = m,
                    Source = src,
                    Type = t
                });
            };

            nameof(LoggingMessageType.Info).Info();
            nameof(LoggingMessageType.Debug).Debug();
            nameof(LoggingMessageType.Error).Error();
            nameof(LoggingMessageType.Trace).Trace();
            nameof(LoggingMessageType.Warning).Warn();

            Task.Delay(100).Wait();
            Assert.IsTrue(messages.All(x => x.Message == x.Type.ToString()));

            new Exception().Error(nameof(TerminalTest), nameof(LoggingTest));
            Task.Delay(100).Wait();

            Assert.IsTrue(messages.Any(x => x.Exception != null));
            Assert.IsTrue(messages.Any(x => x.Source == nameof(TerminalTest)));
            Assert.IsTrue(messages.Any(x => x.Message == nameof(LoggingTest)));
        }

        [Test]
        public void TerminalOutputTest()
        {
            // TODO: I need to work on this case
            using (var ms = new MemoryStream())
            {
                using (var textWriter = new StreamWriter(ms))
                {
                    Terminal.WriteLine("TEST", textWriter);
                    Terminal.WriteLineError("TEST", textWriter);
                }
            }
        }
    }
}