using NUnit.Framework;
using System.Linq;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class AppDomainTest
    {
        [Test]
        public void GetAssembliesTest()
        {
            var assemblies = Swan.Runtime.AppDomain.CurrentDomain.GetAssemblies();

            Assert.IsNotNull(assemblies);
            Assert.IsTrue(assemblies.Any());
        }
    }
}
