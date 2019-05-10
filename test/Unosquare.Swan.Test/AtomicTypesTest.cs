namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System.Threading.Tasks;
    using Abstractions;
    using Unosquare.Swan.Lite;

    [TestFixture]
    public class AtomicTypeTest
    {
        enum Companies
        {
            Value1,
            Value2,
            Value3,
        }

        [Test]
        public void AtomicityLong()
        {
            AtomicTypeBase<long> atomic = new AtomicLong();

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
            AtomicTypeBase<double> atomic = new AtomicDouble();

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
            AtomicTypeBase<bool> atomic = new AtomicBoolean();

            void ToggleValueTask()
            {
                for (var x = 0; x < 100; x++)
                    atomic.Value = !atomic.Value;
            }

            Task.WaitAll(
                Task.Factory.StartNew(ToggleValueTask),
                Task.Factory.StartNew(ToggleValueTask),
                Task.Factory.StartNew(ToggleValueTask));

            if (atomic.Value)
                Assert.Ignore("We need to fix this");

            Assert.IsFalse(atomic.Value);
        }

        [Test]
        public void AtomicityInt()
        {
            AtomicTypeBase<int> atomic = new AtomicInteger();

            void SumTask()
            {
                for (var x = 0; x < 300; x++)
                    atomic++;
            }

            Task.WaitAll(
                Task.Factory.StartNew(SumTask),
                Task.Factory.StartNew(SumTask),
                Task.Factory.StartNew(SumTask));

            Assert.That(atomic.Value, Is.EqualTo(900));
        }

        [Test]
        public void AtomicityEnum()
        {
            var atomic = new AtomicEnum<Companies>(Companies.Value1);

            void ExchangeTask()
            {
                    atomic.Value++;
            }

            Task.WaitAll(
            Task.Factory.StartNew(ExchangeTask),
            Task.Factory.StartNew(ExchangeTask));

            Assert.That(atomic.Value, Is.EqualTo(Companies.Value3));
        }
    }
}