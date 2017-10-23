using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Swan.Exceptions;

namespace Unosquare.Swan.Test.NetworkTests
{
    public abstract class NetworkTest
    {
        protected const string GoogleDnsFqdn = "google-public-dns-a.google.com";

        protected const string Fqdn = "pool.ntp.org";

        protected readonly IPAddress PrivateIP = IPAddress.Parse("192.168.1.1");
        protected readonly IPAddress PublicIP = IPAddress.Parse("200.1.1.1");
        protected readonly IPAddress GoogleDns = IPAddress.Parse("8.8.8.8");
    }

    [TestFixture]
    public class QueryDns : NetworkTest
    {
        [Test]
        public void InvalidDnsAsParam_ThrowsDnsQueryException()
        {
            if (Runtime.OS == OperatingSystem.Osx)
                Assert.Inconclusive("OSX is returning time out");

            Assert.Throws<DnsQueryException>(() => Network.QueryDns("invalid.local", DnsRecordType.MX));
        }

        [TestCase(DnsRecordType.MX)]
        [TestCase(DnsRecordType.NS)]
        [TestCase(DnsRecordType.CNAME)]
        public void ValidDns_ReturnsQueryDns(DnsRecordType dnsRecordType)
        {
            if (Runtime.OS != OperatingSystem.Windows)
            {
                Assert.Ignore("Ignored");
            }
            else
            {
                var mxRecord = Network.QueryDns(GoogleDnsFqdn, dnsRecordType);

                Assert.AreEqual(DnsResponseCode.NoError, mxRecord.ResponseCode,
                    $"{GoogleDnsFqdn} {dnsRecordType} Record has no error");
            }
        }

        [Test]
        public void ValidDnsAndTXTAsDnsRecordType_ReturnsQueryDns()
        {
            if (Runtime.OS != OperatingSystem.Windows)
            {
                Assert.Ignore("Ignored");
            }
            else
            {
                var txtRecords = Network.QueryDns(GoogleDnsFqdn, DnsRecordType.TXT);

                Assert.IsTrue(txtRecords.AnswerRecords.Any());
            }
        }
    }

    [TestFixture]
    public class GetDnsHostEntry : NetworkTest
    {
        [Test]
        public void WithValidDns_ReturnsDnsEntry()
        {
            if (Runtime.OS == OperatingSystem.Osx)
                Assert.Inconclusive("OSX is returning time out");

            var googleDnsIPAddresses = Network.GetDnsHostEntry(GoogleDnsFqdn);

            var targetIP =
                googleDnsIPAddresses.FirstOrDefault(p =>
                    p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            Assert.IsNotNull(targetIP);

            var googleDnsPtrRecord = Network.GetDnsPointerEntry(targetIP);

            var resolvedPtrRecord = Network.GetDnsHostEntry(googleDnsPtrRecord);

            var resolvedIP =
                resolvedPtrRecord.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            Assert.IsNotNull(resolvedIP);
            Assert.IsTrue(resolvedIP.ToString().Equals(targetIP.ToString()));
        }

        [Test]
        public void WithValidDnsAndFinalDot_ReturnsDnsEntry()
        {
            if (Runtime.OS == OperatingSystem.Osx)
                Assert.Inconclusive("OSX is returning time out");

            var googleDnsIPAddressesWithFinalDot = Network.GetDnsHostEntry(GoogleDnsFqdn + ".");
            Assert.IsNotNull(googleDnsIPAddressesWithFinalDot,
                "GoogleDnsFqdn with trailing period resolution is not null");
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
        public void WithIPv6Address_ThrowsArgumentException()
        {
            var privateIP = IPAddress.Parse("2001:0db8:85a3:0000:1319:8a2e:0370:7344");

            Assert.Throws<ArgumentException>(() =>
            {
                privateIP.ToUInt32();
            });
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
        public void WithNoParam_ReturnsIPAddress()
        {
            var publicIPAddress = Network.GetPublicIPAddress();

            Assert.IsNotEmpty(publicIPAddress.ToString());
        }
    }

    [TestFixture]
    public class GetNetworkTimeUtc : NetworkTest
    {
        [Test]
        public void WithNoParam_ReturnsDateTime()
        {
            var publicIPAddress = Network.GetNetworkTimeUtc();

            Assert.That(publicIPAddress, Is.EqualTo(DateTime.UtcNow).Within(1).Minutes);
        }

        [Test]
        public void WithInvalidNtpServerName_ThrowsDnsQueryException()
        {
            Assert.Throws<DnsQueryException>(() => Network.GetNetworkTimeUtc("www"));
        }
    }

    [TestFixture]
    public class GetNetworkTimeUtcAsync : NetworkTest
    {
        [Test]
        public async Task WithNtpServerName_ReturnsDateTime()
        {
            var publicIPAddress = await Network.GetNetworkTimeUtcAsync(Fqdn);

            Assert.That(publicIPAddress, Is.EqualTo(DateTime.UtcNow).Within(1).Minutes);
        }

        [Test]
        public async Task WithIPAddress_ReturnsDateTime()
        {
            var ntpServerAddress = IPAddress.Parse("62.116.162.126");

            var publicIPAddress = await Network.GetNetworkTimeUtcAsync(ntpServerAddress);

            Assert.That(publicIPAddress, Is.EqualTo(DateTime.UtcNow).Within(1).Minutes);
        }
    }

    [TestFixture]
    public class GetDnsHostEntryAsync : NetworkTest
    {
        [Test]
        public async Task WithValidFqdn_ReturnsDnsHost()
        {
            var dnsHost = await Network.GetDnsHostEntryAsync(Fqdn, default(CancellationToken));

            Assert.IsNotEmpty(dnsHost.ToString());
        }

        [Test]
        public async Task WithValidFqdnAndIPAddress_ReturnsDnsHost()
        {
            var dnsHost = await Network.GetDnsHostEntryAsync(Fqdn, GoogleDns, Definitions.DnsDefaultPort,
                default(CancellationToken));

            Assert.IsNotEmpty(dnsHost.ToString());
        }
    }

    [TestFixture]
    public class GetDnsPointerEntryAsync : NetworkTest
    {
        [Test]
        public async Task WithValidFqdnAndIPAddress_ReturnsDnsHost()
        {
            var dnsPointer = await Network.GetDnsPointerEntryAsync(GoogleDns, GoogleDns, Definitions.DnsDefaultPort);

            Assert.AreEqual(dnsPointer, GoogleDnsFqdn);
        }

        [Test]
        public async Task WithValidIPAddress_ReturnsDnsHost()
        {
            var dnsPointer = await Network.GetDnsPointerEntryAsync(GoogleDns);

            Assert.AreEqual(dnsPointer, GoogleDnsFqdn);
        }
    }

    [TestFixture]
    public class QueryDnsAsync : NetworkTest
    {
        [Test]
        public async Task ValidDnsAsDnsServer_ReturnsQueryDns()
        {
            var dnsPointer =
                await Network.QueryDnsAsync(GoogleDnsFqdn, DnsRecordType.MX, GoogleDns, Definitions.DnsDefaultPort);

            Assert.AreEqual(DnsResponseCode.NoError, dnsPointer.ResponseCode);
        }

        [Test]
        public async Task ValidDnsAsParam_ReturnsQueryDns()
        {
            var dnsPointer = await Network.QueryDnsAsync(GoogleDnsFqdn, DnsRecordType.MX);

            Assert.AreEqual(DnsResponseCode.NoError, dnsPointer.ResponseCode);
        }
    }
}