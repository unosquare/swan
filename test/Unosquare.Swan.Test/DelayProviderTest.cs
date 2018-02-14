namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using Unosquare.Swan.Components;

    [TestFixture]
    public class DelayProviderThreadSleep
    {
        [Test]
        public void WaitOne_TakesCertainTime()
        {
            using (var delay = new DelayProvider(DelayProvider.DelayStrategy.ThreadSleep))
            {
                var time = delay.WaitOne();
                var mil = time.Milliseconds;
                Assert.Greater(mil, 1);
            }               
        }
    }

    [TestFixture]
    public class DelayProviderTaskWait
    {
        [Test]
        public void WaitOne_TakesCertainTime()
        {
            using (var delay = new DelayProvider(DelayProvider.DelayStrategy.TaskDelay))
            {
                var time = delay.WaitOne();
                var mil = time.Milliseconds;
                Assert.Greater(mil, 1);
            }
        }
    }

    [TestFixture]
    public class DelayProviderThreadPool
    {
        [Test]
        public void WaitOne_TakesCertainTime()
        {
            using (var delay = new DelayProvider(DelayProvider.DelayStrategy.ThreadPool))
            {
                var time = delay.WaitOne();
                var mil = time.Milliseconds;
                Assert.Greater(mil, 1);
            }
        }
    }
}
