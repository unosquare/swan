namespace Swan.Test
{
    using NUnit.Framework;
    using Swan.Threading;

    [TestFixture]
    public class ThreadWorkerBaseTest
    {
        [Test]
        public async Task ThreadWorkerCycle()
        {
            var worker = new WorkerTest("Test", TimeSpan.FromSeconds(1));
            _ = worker.StartAsync();
            await Task.Delay(1000);
            Assert.That(worker.Counter.Value, Is.GreaterThan(0));
        }
    }

    internal class WorkerTest : ThreadWorkerBase
    {
        public AtomicInteger Counter { get; private set; } = new();

        public WorkerTest(string name, TimeSpan period)
            : base(name, ThreadPriority.Highest, period, null)
        {
        }

        protected override void ExecuteCycleLogic(CancellationToken cancellationToken)
            => Counter.Increment();

        protected override void OnCycleException(Exception ex) => throw new NotImplementedException();
    }
}
