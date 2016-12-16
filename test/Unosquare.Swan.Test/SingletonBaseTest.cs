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
        [Test]
        public void GetInstanceNameTest()
        {
            Assert.AreEqual(nameof(MockProvider), MockProvider.Instance.GetName());
        }
    }
}
