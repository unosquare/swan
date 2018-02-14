namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using Unosquare.Swan.Components;

    [TestFixture]
    public class WaitEvent
    {
        [Test]
        public void NormalCycle()
        {
            var wait = WaitEventFactory.Create(true);

            Assert.IsTrue(wait.IsCompleted);

            wait.Begin();

            Assert.IsFalse(wait.IsCompleted);
            Assert.IsTrue(wait.IsInProgress);

            wait.Complete();

            Assert.IsTrue(wait.IsCompleted);
            Assert.IsFalse(wait.IsInProgress);
        }

        [Test]
        public void IfDisposed_IsInProgressEqualsFalse()
        {
            var wait = WaitEventFactory.Create(true);
            wait.Begin();
            wait.Dispose();
            wait.Begin();

            Assert.IsFalse(wait.IsInProgress);
            Assert.IsFalse(wait.IsValid);
        }
    }

    [TestFixture]
    public class WaitEventSllim
    {
        [Test]
        public void NormalCycle()
        {
            var wait = WaitEventFactory.Create(true, true);
            Assert.IsTrue(wait.IsCompleted);

            wait.Begin();

            Assert.IsFalse(wait.IsCompleted);
            Assert.IsTrue(wait.IsInProgress);

            wait.Complete();

            Assert.IsTrue(wait.IsCompleted);
            Assert.IsFalse(wait.IsInProgress);
        }

        [Test]
        public void IfDisposed_IsInProgressEqualsFalse()
        {
            var wait = WaitEventFactory.Create(true, true);
            wait.Begin();
            wait.Dispose();
            wait.Begin();

            Assert.IsFalse(wait.IsInProgress);
            Assert.IsFalse(wait.IsValid);
        }
    }
}
