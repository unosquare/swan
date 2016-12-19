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
            Assert.AreEqual(
#if NET452
                4,
#else
                7,
#endif
                assemblies.Count(),
                "Check assemblies are loaded fine");
        }
    }
}
