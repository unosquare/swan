using NUnit.Framework;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SingletonBaseTest
    {
        static readonly MockProvider Mocks = new MockProvider();
        static readonly MockProvider MocksInstance = MockProvider.Instance;

        [Test]
        public void GetInstanceNameTest()
        {
            Assert.AreEqual(nameof(MockProvider), MocksInstance.GetName());
        }

        [Test]
        public void GetTypeTest()
        {
            Assert.AreEqual(typeof(MockProvider), MocksInstance.GetType());
        }

        [Test]
        public void StringifyTest()
        {
            Assert.AreEqual(Mocks.Stringify(), MocksInstance.Stringify());
        }

        [Test]
        public void ToJsonTest()
        {
            Assert.AreEqual(Mocks.ToJson(), MocksInstance.ToJson());
        }

        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual(Mocks.ToString(), MocksInstance.ToString());
        }

        [Test]
        public void ToStringInvariantTest()
        {
            Assert.AreEqual(Mocks.ToStringInvariant(), MocksInstance.ToStringInvariant());
        }
    }
}
