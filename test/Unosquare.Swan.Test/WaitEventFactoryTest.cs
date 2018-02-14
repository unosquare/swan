namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using Unosquare.Swan.Components;

    [TestFixture]
    public class WaitEvent
    {
        [Test]
        [TestCase(true,true)]
        [TestCase(true,false)]
        public void NormalCycle(bool initialValue, bool useSlim)
        {
            var wait = WaitEventFactory.Create(initialValue, useSlim);

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
}
