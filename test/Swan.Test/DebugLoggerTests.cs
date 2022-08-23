namespace Swan.Test;

using Logging;
using NUnit.Framework;

[TestFixture]
public class DebugLoggerTests
{
    [Test]
    public void DebugLoggerTest() =>
        Assert.That(DebugLogger.Instance, Is.Not.Null);

    [Test]
    public void LogLevelTest() =>
        Assert.That(DebugLogger.Instance.LogLevel,
            Is.EqualTo(DebugLogger.IsDebuggerAttached ? LogLevel.Trace : LogLevel.None));
}
