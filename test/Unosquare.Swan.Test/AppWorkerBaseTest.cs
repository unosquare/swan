namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;
    using Mocks;

    [TestFixture]
    public class AppWorkerBaseTest
    {
        [Test]
        public void CanStartAndStopTest()
        {
            var mock = new AppWorkerMock();
            var exit = false;
            mock.OnExit = () => exit = true;
            Assert.AreEqual(AppWorkerState.Stopped, mock.State);
            mock.Start();
            Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();

            Assert.IsTrue(mock.IsBusy, "Worker is busy");
            Assert.AreEqual(AppWorkerState.Running, mock.State);
            mock.Stop();
            Assert.AreEqual(AppWorkerState.Stopped, mock.State);

            Assert.IsTrue(mock.ExitBecauseCancellation, "Exit because cancellation");
            Assert.IsTrue(exit, "Exit event was fired");
        }

        [Test]
        public async Task WorkingTest()
        {
            var mock = new AppWorkerMock();
            mock.Start();

            // Mock increase count by one every 100 ms, wait a little bit
            await Task.Delay(TimeSpan.FromSeconds(1));
            Assert.GreaterOrEqual(mock.Count, 5);
        }

        [Test]
        public async Task AppWorkerExceptionTest()
        {
            var mock = new AppWorkerMock();
            mock.Start();

            // Mock increase count by one every 100 ms, wait a little bit
            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.IsFalse(mock.IsBusy, "The AppWorker is not busy");
            Assert.IsFalse(mock.ExitBecauseCancellation, "The AppWorker doesn't exit because cancellation");
            Assert.IsNotNull(mock.Exception, "The AppWorker had an exception");
        }

        [Test]
        public void OnStateChangedTest()
        {
            var mock = new AppWorkerMock();
            var start = false;
            var stop = false;

            mock.StateChanged += (s, e) =>
            {
                if (e.NewState == AppWorkerState.Running) start = true;
                if (e.NewState == AppWorkerState.Stopped) stop = true;
            };

            mock.Start();
            mock.Stop();
            Assert.IsTrue(start);
            Assert.IsTrue(stop);
        }
    }
}
