namespace Swan.Test.SingletonBaseTest
{
    using NUnit.Framework;
    using Swan.Test.Mocks;

    [TestFixture]
    public class Dispose
    {
        [Test]
        public void WithDispose_DoesntThrowsException()
        {
            MockProvider.Instance.Dispose();

            // here we only check if we don't get any exception
            Assert.IsTrue(true);
        }

        [Test]
        public void WithDisposeTwiceTest_DoesntThrowsException()
        {
            MockProvider.Instance.Dispose();
            MockProvider.Instance.Dispose();

            // here we only check if we don't get any exception
            Assert.IsTrue(true);
        }
    }
}