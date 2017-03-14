using System.Linq;
using NUnit.Framework;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SnmpClientTest
    {
        [Test]
        public void TestDiscovery()
        {
            var data = SnmpClient.Discover();
            Assert.IsNotNull(data);

            // TODO: we can't access snmp devices, how to test?
        }
    }
}
