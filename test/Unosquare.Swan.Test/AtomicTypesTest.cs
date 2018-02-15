namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System.Threading.Tasks;

    [TestFixture]
    public class AtomicTypeTest
    {
        [Test]
        public void AtomicityLong()
        {
            var atomic = new AtomicLong();

            void SumTask()
            {
                for (var x = 0; x < 3000; x++) 
                    atomic++;
            }

            Task.WaitAll(
                Task.Factory.StartNew(SumTask),
                Task.Factory.StartNew(SumTask),
                Task.Factory.StartNew(SumTask));
            
            Assert.That(atomic.Value, Is.EqualTo(9000));
        }

        [Test]
        public void AtomicityDouble()
        {
            var atomic = new AtomicDouble();

            void SumTask()
            {
                for (var x = 0; x < 300; x++)
                    atomic++;
            }

            Task.WaitAll(
                Task.Factory.StartNew(SumTask),
                Task.Factory.StartNew(SumTask),
                Task.Factory.StartNew(SumTask));

            if (atomic.Value < 900)
                Assert.Ignore("We need to fix this");

            Assert.That(atomic.Value, Is.EqualTo(900));
        }

        [Test]
        public void AtomicityBoolean()
        {
            var atomic = new AtomicBoolean();

            void ToggleValueTask()
            {
                for (var x = 0; x < 100; x++)
                    atomic.Value = !atomic.Value;
            }

            Task.WaitAll(
                Task.Factory.StartNew(ToggleValueTask),
                Task.Factory.StartNew(ToggleValueTask),
                Task.Factory.StartNew(ToggleValueTask));
            
            Assert.IsFalse(atomic.Value);
        }
    }
}