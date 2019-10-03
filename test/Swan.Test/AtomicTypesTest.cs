namespace Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;
    using Threading;

    [TestFixture]
    public class AtomicTypeTest
    {
        private enum Companies
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
                Task.Run(SumTask),
                Task.Run(SumTask),
                Task.Run(SumTask));

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
                Task.Run(SumTask),
                Task.Run(SumTask),
                Task.Run(SumTask));

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
                Task.Run(ToggleValueTask),
                Task.Run(ToggleValueTask),
                Task.Run(ToggleValueTask));

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
                Task.Run(SumTask),
                Task.Run(SumTask),
                Task.Run(SumTask));

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
                Task.Run(ExchangeTask),
                Task.Run(ExchangeTask));

            Assert.GreaterOrEqual(2, (int) atomic.Value);
        }

        [Test]
        public void AtomicityDateTime()
        {
            var currentDate = DateTime.Now;

            AtomicTypeBase<DateTime> atomic = new AtomicDateTime(currentDate);

            void ToggleValueTask()
            {
                for (var x = 0; x < 10; x++)
                    atomic++;
            }

            Task.WaitAll(
                Task.Run(ToggleValueTask),
                Task.Run(ToggleValueTask),
                Task.Run(ToggleValueTask));

            var expected = currentDate.AddTicks(30).Date;

            Assert.That(atomic.Value.Date == expected);
        }

        [Test]
        public void CompareTo()
        {
            long objectValue = 10;
            long objectOtherValue = 100;
            AtomicTypeBase<long> original = new AtomicLong(10);
            AtomicTypeBase<long> copy = new AtomicLong(10);
            AtomicTypeBase<long> other = new AtomicLong(100);

            Assert.That(original.CompareTo(copy), Is.EqualTo(0));
            Assert.That(original.CompareTo(other), Is.Not.EqualTo(0));

            Assert.That(original.CompareTo(copy as object), Is.EqualTo(0));
            Assert.That(original.CompareTo(objectValue as object), Is.EqualTo(0));
            Assert.That(original.CompareTo(null as object), Is.EqualTo(1));

            Assert.That(original.CompareTo(objectValue), Is.EqualTo(0));
            Assert.That(original.CompareTo(objectOtherValue), Is.Not.EqualTo(0));

            Assert.Throws<ArgumentException>(() => original.CompareTo(new object()));
        }

        [Test]
        public void Equals()
        {
            long objectValue = 10;
            long objectOtherValue = 100;
            AtomicTypeBase<long> original = new AtomicLong(10);
            AtomicTypeBase<long> copy = new AtomicLong(10);
            AtomicTypeBase<long> other = new AtomicLong(100);

            Assert.That(original.Equals(copy));
            Assert.That(original.Equals(other), Is.False);

            Assert.That(original.Equals(copy as object));
            Assert.That(original.Equals(objectValue as object));
            Assert.That(original.Equals(null as object), Is.False);

            Assert.That(original.Equals(objectValue));
            Assert.That(original.Equals(objectOtherValue), Is.False);
        }
    }
}
