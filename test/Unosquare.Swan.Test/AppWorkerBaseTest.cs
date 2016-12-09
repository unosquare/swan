using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Unosquare.Swan.Abstractions;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class AppWorkerBaseTest
    {
        private AppWorkerMock mock;

        [SetUp]
        public void Setup()
        {
            mock = new AppWorkerMock();
        }

        [Test]
        public void CanStartAndStopTest()
        {
            Assert.AreEqual(AppWorkerState.Stopped, mock.State);
            mock.Start();
            Assert.AreEqual(AppWorkerState.Running, mock.State);
            mock.Stop();
            Assert.AreEqual(AppWorkerState.Stopped, mock.State);
            Assert.IsTrue(mock.ExitBecauseCancellation);
        }
        
        [Test]
        public async Task IsBusyTest()
        {
            mock.Start();
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            Assert.IsTrue(mock.IsBusy);
        }

        [Test]
        public async Task WorkingTest()
        {
            mock.Start();
            // Mock increase count by one every 100 ms, wait a little bit
            await Task.Delay(TimeSpan.FromMilliseconds(600));
            Assert.GreaterOrEqual(mock.Count, 5);
        }
        
        [Test]
        public async Task ExceptionTest()
        {
            mock.Start();
            // Mock increase count by one every 100 ms, wait a little bit
            await Task.Delay(TimeSpan.FromMilliseconds(800));

            Assert.IsFalse(mock.IsBusy);
            Assert.IsFalse(mock.ExitBecauseCancellation);
            Assert.IsNotNull(mock.Exception);
        }

        [Test]
        public void OnExitTest()
        {
            var exit = false;
            mock.OnExit = () => exit = true;
            mock.Start();
            mock.Stop();
            Assert.IsTrue(exit);
        }

        [Test]
        public void OnStateChangedTest()
        {
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
        
        [TearDown]
        public void Kill()
        {
            if (mock?.State == AppWorkerState.Running)
                mock.Stop();
        }
    }
}
