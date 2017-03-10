using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SingletonBaseTest
    {
        static MockProvider mocks = new MockProvider();
        static MockProvider mocksInstance = MockProvider.Instance;

        [Test]
        public void GetInstanceNameTest()
        {
            Assert.AreEqual(nameof(MockProvider), mocksInstance.GetName());
        }

        [Test]
        public void GetTypeTest()
        {
            Assert.AreEqual(typeof(MockProvider), mocksInstance.GetType());
        }

        [Test]
        public void StringifyTest()
        {
            Assert.AreEqual(mocks.Stringify(), mocksInstance.Stringify());
        }

        [Test]
        public void ToJsonTest()
        {
            Assert.AreEqual(mocks.ToJson(), mocksInstance.ToJson());
        }

        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual(mocks.ToString(), mocksInstance.ToString());
        }

        [Test]
        public void ToStringInvariantTest()
        {
            Assert.AreEqual(mocks.ToStringInvariant(), mocksInstance.ToStringInvariant());
        }
    }
}
