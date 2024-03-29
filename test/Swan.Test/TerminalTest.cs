﻿namespace Swan.Test;

using Platform;
using System.Text;

[TestFixture]
public class IsConsolePresent
{
    [Test]
    public void ConsolePresent_ReturnsTrue()
    {
        if (OperatingSystem.IsWindows())
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
        if (OperatingSystem.IsWindows())
            Assert.Ignore("Windows doesn't provide writers");

        var writers = Terminal.AvailableWriters;

        Assert.AreNotEqual(writers, TerminalWriterFlags.None, "Check for at least one available writer");
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

        Terminal.OutputEncoding = Encoding.UTF8;

        Assert.AreEqual(Terminal.OutputEncoding, Encoding.UTF8, "Change to UTF8 encoding");
    }
}
