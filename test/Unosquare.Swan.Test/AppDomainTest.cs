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
            // NET452 sometimes is loading 17 or 20
            Assert.GreaterOrEqual(
#if NET452
                17,
#else
                4,
#endif
                assemblies.Count(),
                "Check assemblies are loaded fine");
        }

        [Test]
        public void GetAppDomain()
        {
            Assert.IsNotNull(CurrentApp.AppDomain);
        }
    }
}
