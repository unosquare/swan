using NUnit.Framework;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SingletonBaseTest
    {
        private static readonly MockProvider Mocks = new MockProvider();

        [Test]
        public void GetInstanceNameTest()
        {
            Assert.AreEqual(nameof(MockProvider), MockProvider.Instance.GetName());
        }

        [Test]
        public void GetTypeTest()
        {
            Assert.AreEqual(typeof(MockProvider), MockProvider.Instance.GetType());
        }

        [Test]
        public void StringifyTest()
        {
            // We need better testing here
            Assert.AreEqual(Mocks.Stringify(), MockProvider.Instance.Stringify());
        }
        
        [Test]
        public void DisposeTest()
        {
            MockProvider.Instance.Dispose();
            // here we only check if we don't get any exception
            Assert.IsTrue(true);
        }
    }
}
