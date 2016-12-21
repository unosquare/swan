using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Utilities;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class NetworkTest
    {
        private const string GoogleDnsFqdn = "google-public-dns-a.google.com";

        [Test]
        public void SimpleResolveIPAddressTest()
        {

            var googleDnsIPAddresses = Network.GetDnsHostEntry(GoogleDnsFqdn);
            Assert.IsNotNull(googleDnsIPAddresses);

            var targetIP = googleDnsIPAddresses.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            Assert.IsNotNull(targetIP);

            var googleDnsPtrRecord = Network.GetDnsPointerEntry(targetIP);
            Assert.IsNotNull(googleDnsPtrRecord);

            var resolvedPtrRecord = Network.GetDnsHostEntry(googleDnsPtrRecord);
            Assert.IsNotNull(resolvedPtrRecord);

            var resolvedIP = resolvedPtrRecord.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            Assert.IsNotNull(resolvedIP);

            Assert.IsTrue(resolvedIP.ToString().Equals(targetIP.ToString()));
        }

    }
}
