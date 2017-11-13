namespace Unosquare.Swan.Test.SingletonBaseTest
{
    using NUnit.Framework;
    using Unosquare.Swan.Test.Mocks;

    [TestFixture]
    public class GetName
    {
        [Test]
        public void WithMockProvider_GetsInstanceName()
        {
            Assert.AreEqual(nameof(MockProvider), MockProvider.Instance.GetName());
        }
    }

    [TestFixture]
    public class GetType
    {
        [Test]
        public void WithMockProvider_GetsType()
        {
            Assert.AreEqual(typeof(MockProvider), MockProvider.Instance.GetType());
        }
    }

    [TestFixture]
    public class Stringify
    {
        [Test]
        public void WithMockProvider_ReturnsStringifiedMock()
        {
            MockProvider Mocks = new MockProvider();
            // We need better testing here
            Assert.AreEqual(Mocks.Stringify(), MockProvider.Instance.Stringify());
        }
    }

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
