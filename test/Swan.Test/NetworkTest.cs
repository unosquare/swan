namespace Swan.Test.NetworkTests
{
    using NUnit.Framework;
    using System.Linq;
    using System;
    using Net;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Threading.Tasks;
    using Net.Dns;

    public abstract class NetworkTest
    {
        protected const string GoogleDnsFqdn = "google-public-dns-a.google.com";

        protected IPAddress PrivateIP { get; } = IPAddress.Parse("192.168.1.1");
        protected IPAddress PublicIP { get; } = IPAddress.Parse("200.1.1.1");
        protected IPAddress NullIP { get; } = null;
    }

    [TestFixture]
    public class QueryDns : NetworkTest
    {
        [Test]
        public void InvalidDnsAsParam_ThrowsDnsQueryException()
        {
            Assert.ThrowsAsync<DnsQueryException>(() => Network.QueryDnsAsync("invalid.local", DnsRecordType.MX));
        }

        [TestCase(DnsRecordType.MX)]
        [TestCase(DnsRecordType.NS)]
        [TestCase(DnsRecordType.SOA)]
        [TestCase(DnsRecordType.SRV)]
        [TestCase(DnsRecordType.WKS)]
        [TestCase(DnsRecordType.CNAME)]
        public async Task ValidDns_ReturnsQueryDns(DnsRecordType dnsRecordType)
        {
            try
            {
                var records = await Network.QueryDnsAsync(GoogleDnsFqdn, dnsRecordType);

                Assert.IsFalse(records.IsAuthoritativeServer, $"IsAuthoritativeServer, Testing with {dnsRecordType}");
                Assert.IsFalse(records.IsTruncated, $"IsTruncated, Testing with {dnsRecordType}");
                Assert.IsTrue(records.IsRecursionAvailable, $"IsRecursionAvailable, Testing with {dnsRecordType}");
                Assert.AreEqual("Query",
                    records.OperationCode.ToString(),
                    $"OperationCode, Testing with {dnsRecordType}");
                Assert.AreEqual(DnsResponseCode.NoError,
                    records.ResponseCode,
                    $"{GoogleDnsFqdn} {dnsRecordType} Record has no error");
                Assert.AreEqual(dnsRecordType == DnsRecordType.TXT,
                    records.AnswerRecords.Any(),
                    $"AnswerRecords, Testing with {dnsRecordType}");

                await Task.Delay(100);
            }
            catch (DnsQueryException)
            {
                Assert.Ignore("Timeout");
            }
        }

        [Test]
        public async Task ValidDnsMultipleCalls_ReturnsDifferentId()
        {
            try
            {
                var record = await Network.QueryDnsAsync(GoogleDnsFqdn, DnsRecordType.TXT);
                await Task.Delay(100);
                var records = await Network.QueryDnsAsync(GoogleDnsFqdn, DnsRecordType.TXT);

                Assert.AreNotEqual(records.Id, record.Id, "Different Id");
            }
            catch (DnsQueryException)
            {
                Assert.Ignore("Timeout");
            }
        }

        [Test]
        public void WithNullFqdn_ReturnsQueryDns()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await Network.QueryDnsAsync(null, DnsRecordType.TXT));
        }
    }

    [TestFixture]
    public class GetDnsHostEntry : NetworkTest
    {
        [Test]
        public async Task WithValidDns_ReturnsDnsEntry()
        {
            try
            {
                var googleDnsIPAddresses = await Network.GetDnsHostEntryAsync(GoogleDnsFqdn);

                var targetIP =
                    googleDnsIPAddresses.FirstOrDefault(p =>
                        p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                Assert.IsNotNull(targetIP);

                var googleDnsPtrRecord = await Network.GetDnsPointerEntryAsync(targetIP);

                var resolvedPtrRecord = await Network.GetDnsHostEntryAsync(googleDnsPtrRecord);

                var resolvedIP =
                    resolvedPtrRecord.FirstOrDefault(p =>
                        p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                Assert.IsNotNull(resolvedIP);
            }
            catch (DnsQueryException)
            {
                Assert.Ignore("Timeout");
            }
        }

        [Test]
        public async Task WithValidDnsAndFinalDot_ReturnsDnsEntryAsync()
        {
            var googleDnsIPAddressesWithFinalDot = await Network.GetDnsHostEntryAsync(GoogleDnsFqdn + ".");
            Assert.IsNotNull(googleDnsIPAddressesWithFinalDot,
                "GoogleDnsFqdn with trailing period resolution is not null");
        }

        [Test]
        public void WithNullFqdn_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => Network.GetDnsHostEntryAsync(null));
        }
    }

    [TestFixture]
    public class IsPrivateAddress : NetworkTest
    {
        [Test]
        public void PrivateIPWithValidAddress_ReturnsTrue()
        {
            Assert.IsTrue(PrivateIP.IsPrivateAddress());
        }

        [Test]
        public void PublicIPWithValidAddress_ReturnsFalse()
        {
            Assert.IsFalse(PublicIP.IsPrivateAddress());
        }

        [Test]
        public void WithNullAddress_ReturnsFalse()
        {
            Assert.Throws<ArgumentNullException>(() => NullIP.IsPrivateAddress());
        }
    }

    [TestFixture]
    public class ToUInt32 : NetworkTest
    {
        [Test]
        public void PrivateIPWithValidAddress_ReturnsAddressAsInt()
        {
            Assert.AreEqual(3232235777, PrivateIP.ToUInt32());
        }

        [Test]
        public void PublicIPWithValidAddress_ReturnsAddressAsInt()
        {
            Assert.AreEqual(3355508993, PublicIP.ToUInt32());
        }

        [Test]
        public void WithNullAddress_ReturnsFalse()
        {
            Assert.Throws<ArgumentNullException>(() => NullIP.ToUInt32());
        }

        [Test]
        public void WithIPv6Address_ThrowsArgumentException()
        {
            var privateIP = IPAddress.Parse("2001:0db8:85a3:0000:1319:8a2e:0370:7344");

            Assert.Throws<ArgumentException>(() => privateIP.ToUInt32());
        }
    }

    [TestFixture]
    public class GetIPv4Addresses : NetworkTest
    {
        [Test]
        public void Wireless80211AsParam_ReturnsIPv4Address()
        {
            var networkType = Network.GetIPv4Addresses(NetworkInterfaceType.Wireless80211);

            Assert.IsNotNull(networkType);
        }

        [Test]
        public void LoopbackAsParam_ReturnsIPv4Address()
        {
            var networkType = Network.GetIPv4Addresses(NetworkInterfaceType.Loopback);

            Assert.AreEqual(networkType[0].ToString(), "127.0.0.1");
        }

        [Test]
        public void WithNoParam_ReturnsIPv4Address()
        {
            var networkType = Network.GetIPv4Addresses();

            Assert.IsNotNull(networkType);
        }
    }

    [TestFixture]
    public class GetPublicIPAddress : NetworkTest
    {
        [Test]
        public async Task WithNoParam_ReturnsIPAddress()
        {
            var publicIPAddress = await Network.GetPublicIPAddressAsync();

            Assert.IsNotEmpty(publicIPAddress.ToString());
        }
    }

    [TestFixture]
    public class GetNetworkTimeUtc : NetworkTest
    {
        [Test]
        public void WithInvalidNtpServerName_ThrowsDnsQueryException()
        {
            Assert.ThrowsAsync<DnsQueryException>(() => Network.GetNetworkTimeUtcAsync("www"));
        }

        [Test]
        public void WithNullNtpServerName_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => Network.GetNetworkTimeUtcAsync(NullIP));
        }

        [Test]
        public async Task WithIPAddressAndPort_ReturnsDateTime()
        {
            var ntpServerAddress = IPAddress.Parse("127.0.0.1");

            var publicIPAddress = await Network.GetNetworkTimeUtcAsync(ntpServerAddress, 1203);

            Assert.AreEqual(publicIPAddress, new DateTime(1900, 1, 1));
        }
    }
}