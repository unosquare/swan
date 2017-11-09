using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.TerminalTests
{
    public abstract class TerminalTest { }

    [TestFixture]
    public class IsConsolePresent : TerminalTest
    {
        [Test]
        public void ConsolePresent_ReturnsTrue()
        {
            if (Runtime.OS == OperatingSystem.Windows)
                Assert.Ignore("Failing test on Windows");

            Assert.IsTrue(Terminal.IsConsolePresent);
        }
    }

    [TestFixture]
    public class AvailableWriters : TerminalTest
    {
        [Test]
        public void Writers_ReturnsNotEqualWriters()
        {
            if (Runtime.OS == OperatingSystem.Windows)
                Assert.Ignore("Windows doesn't provide writers");

            var writers = Terminal.AvailableWriters;

            Assert.AreNotEqual(writers, TerminalWriters.None, "Check for at least one available writer");
        }
    }

    [TestFixture]
    public class OutputEncoding : TerminalTest
    {
        [Test]
        public void DefaultEncoding_ReturnsEqualEncoding()
        {
            var defaultEncoding = Terminal.OutputEncoding;

            Assert.IsNotNull(defaultEncoding);

            Terminal.OutputEncoding = System.Text.Encoding.UTF8;

            Assert.AreEqual(Terminal.OutputEncoding, System.Text.Encoding.UTF8, "Change to UTF8 encoding");
        }
    }
}