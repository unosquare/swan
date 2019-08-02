namespace Swan.Test
{
    using System;
    using NUnit.Framework;
    using Components;

    [TestFixture]
    public class TimerControlTest
    {
        [Test]
        public void WithFutureDate_WaitUntilDate()
        {
            var futureDate = DateTime.UtcNow.AddSeconds(1);
            TimerControl.Instance.WaitUntil(futureDate);
            Assert.Greater(DateTime.UtcNow, futureDate);
        }

        [Test]
        public void WithTimeSpan_WaitThatTime()
        {
            var timeSpan = TimeSpan.FromSeconds(1);
            var futureDate = DateTime.UtcNow.Add(timeSpan);
            TimerControl.Instance.Wait(timeSpan);
            Assert.Greater(DateTime.UtcNow, futureDate);
        }
    }
}
