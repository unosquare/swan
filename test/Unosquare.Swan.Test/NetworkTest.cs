using NUnit.Framework;
using System.Linq;
using System.Net;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class NetworkTest
    {
        private const string GoogleDnsFqdn = "google-public-dns-a.google.com";
        private readonly IPAddress PrivateIP = IPAddress.Parse("192.168.1.1");
        private readonly IPAddress PublicIP = IPAddress.Parse("200.1.1.1");

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

        [Test]
        public void IsPrivateAddressTest()
        {
            Assert.IsTrue(PrivateIP.IsPrivateAddress());
            Assert.IsFalse(PublicIP.IsPrivateAddress());
        }

        [Test]
        public void IPAddressToUint32Test()
        {
            Assert.AreEqual(3232235777, PrivateIP.ToUInt32());
            Assert.AreEqual(3355508993, PublicIP.ToUInt32());
        }

        [Test]
        public void QueryDnsTest()
        {
            var mxRecord = Network.QueryDns(GoogleDnsFqdn, DnsRecordType.MX);

            Assert.IsNotNull(mxRecord);
            Assert.AreEqual(DnsResponseCode.NoError, mxRecord.ResponseCode);

            var txtRecords = Network.QueryDns(GoogleDnsFqdn, DnsRecordType.TXT);

            Assert.IsNotNull(txtRecords);
            Assert.IsTrue(txtRecords.AnswerRecords.Any());
        }

        [Test]
        public void QueryDnsErrorTest()
        {
            Assert.Throws<DnsQueryException>(() => Network.QueryDns("invalid.local", DnsRecordType.MX));
        }
    }
}