using NUnit.Framework;
using Swan.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swan.Test
{
    [TestFixture]
    public class ThreadWorkerBaseTest
    {
        [Test]
        public async Task ThreadWorkerCycle()
        {
            var worker = new WorkerTest("Test", TimeSpan.FromSeconds(1));
            await worker.StartAsync();
            await Task.Delay(1000);
            Assert.That(worker.Counter.Value, Is.GreaterThan(0));
        }
    }

    internal class WorkerTest : ThreadWorkerBase
    {
        public AtomicTypeBase<int> Counter { get; private set; } = new AtomicInteger(0);

        public WorkerTest(string name, TimeSpan period)
            : base(name, ThreadPriority.Highest, period, null)
        {
        }

        protected override void ExecuteCycleLogic(CancellationToken cancellationToken)
            => Counter++;

        protected override void OnCycleException(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
