namespace Swan.Test;

using NUnit.Framework;
using Threading;

[TestFixture]
public class ExclusiveTimerTest
{
    [Test]
    public void WithFutureDate_WaitUntilDate()
    {
        var futureDate = DateTime.UtcNow.AddSeconds(1);
        ExclusiveTimer.WaitUntil(futureDate);
        Assert.Greater(DateTime.UtcNow, futureDate);
    }

    [Test]
    public void WithTimeSpan_WaitThatTime()
    {
        var timeSpan = TimeSpan.FromSeconds(1);
        var futureDate = DateTime.UtcNow.Add(timeSpan);
        ExclusiveTimer.Wait(timeSpan);
        Assert.Greater(DateTime.UtcNow, futureDate);
    }
}
