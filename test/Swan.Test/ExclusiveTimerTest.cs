namespace Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Threading;
    using Threading;

    [TestFixture]
    public class ExclusiveTimerTest
    {
        [Test]
        public void WithDefaultTimer_WaitsForOneIteration()
        {
            var i = 0;

            using (var timer = new ExclusiveTimer(() => i++, 0, 100))
            {
                Thread.Sleep(130);

                Assert.GreaterOrEqual(i, 1, "First iteration");

                Thread.Sleep(120);

                Assert.GreaterOrEqual(i, 2, "Second iteration");
            }
        }

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
}
