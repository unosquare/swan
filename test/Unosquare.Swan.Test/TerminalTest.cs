﻿namespace Swan.Test.TerminalTests
{
    using NUnit.Framework;

    [TestFixture]
    public class IsConsolePresent
    {
        [Test]
        public void ConsolePresent_ReturnsTrue()
        {
            if (SwanRuntime.OS == OperatingSystem.Windows)
                Assert.Ignore("Failing test on Windows");

            Assert.IsTrue(Terminal.IsConsolePresent);
        }
    }

    [TestFixture]
    public class AvailableWriters
    {
        [Test]
        public void Writers_ReturnsNotEqualWriters()
        {
            if (SwanRuntime.OS == OperatingSystem.Windows)
                Assert.Ignore("Windows doesn't provide writers");

            var writers = Terminal.AvailableWriters;

            Assert.AreNotEqual(writers, TerminalWriters.None, "Check for at least one available writer");
        }
    }

    [TestFixture]
    public class OutputEncoding
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