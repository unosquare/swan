namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using Networking;

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
