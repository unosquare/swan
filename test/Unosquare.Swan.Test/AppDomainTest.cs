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
            var assemblies = Runtime.GetAssemblies();

            Assert.IsNotNull(assemblies);
            Assert.IsTrue(assemblies.Any());
            // NET452 sometimes is loading 17 or 20 and dotnet 4
            Assert.GreaterOrEqual(assemblies.Length, 4, "Check assemblies are loaded fine");
        }

        [Test]
        public void GetAppDomain()
        {
            Assert.IsNotNull(Runtime.GetAssemblies());
        }
    }
}
