namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System.Threading;

    [TestFixture]
    class AtomicLongTest
    {
        private long count = 0;
        private AtomicLong atomic = new AtomicLong(0);

        [Test]
        public void Atomicity()
        {
            var t1 = new Thread(SumTask);
            var t2 = new Thread(SumTask);
            var t3 = new Thread(SumTask);

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            Assert.That(atomic.Value, Is.EqualTo(30000));
        }

        private void SumTask()
        {
            for (int x = 0; x < 10000; x++)
            {
                atomic.Value++;
            }
        }
    }
}
