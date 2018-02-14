namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    [TestFixture]
    public class AtomicLongTest
    {
        private AtomicLong atomic = new AtomicLong(0);

        [Test]
        public void Atomicity()
        {
            var t1 = new Task(new Action(SumTask));
            var t2 = new Task(new Action(SumTask));
            var t3 = new Task(new Action(SumTask));

            t1.Start();
            t2.Start();
            t3.Start();

            Task.WaitAll(t1, t2, t3);
            Assert.That(atomic.Value, Is.EqualTo(3000));
        }

        private void SumTask()
        {
            for (int x = 0; x < 1000; x++)
            {
                atomic.Value++;
            }
        }
    }
}
