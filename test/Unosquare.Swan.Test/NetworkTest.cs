using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Swan.Exceptions;
using Unosquare.Swan.Networking.Ldap;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class NetworkTest
    {
        private const string GoogleDnsFqdn = "google-public-dns-a.google.com";

        private const string Fqdn = "pool.ntp.org";
        private readonly IPAddress _privateIP = IPAddress.Parse("192.168.1.1");
        private readonly IPAddress _publicIP = IPAddress.Parse("200.1.1.1");
        private readonly IPAddress _googleDns = IPAddress.Parse("8.8.8.8");

        public class QueryDns : NetworkTest
        {
            [Test]
            public void InvalidDnsAsParam_ThrowsDnsQueryException()
            {
                if(Runtime.OS == OperatingSystem.Osx)
                    Assert.Inconclusive("OSX is returning time out");

                Assert.Throws<DnsQueryException>(() => Network.QueryDns("invalid.local", DnsRecordType.MX));
            }

            [Test]
            public void ValidDnsAndMXAsDnsRecordType_ReturnsQueryDns()
            {
                if(Runtime.OS != OperatingSystem.Windows)
                {
                    Assert.Ignore("Ignored");
                }
                else
                {
                    var mxRecord = Network.QueryDns(GoogleDnsFqdn, DnsRecordType.MX);

                    Assert.AreEqual(DnsResponseCode.NoError, mxRecord.ResponseCode);
                }
            }

            [Test]
            public void ValidDnsAndTXTAsDnsRecordType_ReturnsQueryDns()
            {
                if(Runtime.OS != OperatingSystem.Windows)
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

        public class GetDnsHostEntry : NetworkTest
        {
            [Test]
            public void WithValidDns_ReturnsDnsEntry()
            {
                if(Runtime.OS == OperatingSystem.Osx)
                    Assert.Inconclusive("OSX is returning time out");

                var googleDnsIPAddresses = Network.GetDnsHostEntry(GoogleDnsFqdn);

                var targetIP = googleDnsIPAddresses.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                var googleDnsPtrRecord = Network.GetDnsPointerEntry(targetIP);

                var resolvedPtrRecord = Network.GetDnsHostEntry(googleDnsPtrRecord);

                var resolvedIP = resolvedPtrRecord.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                Assert.IsTrue(resolvedIP.ToString().Equals(targetIP.ToString()));
            }

            [Test]
            public void WithValidDnsAndFinalDot_ReturnsDnsEntry()
            {
                if(Runtime.OS == OperatingSystem.Osx)
                    Assert.Inconclusive("OSX is returning time out");

                var googleDnsIPAddressesWithFinalDot = Network.GetDnsHostEntry(GoogleDnsFqdn + ".");
                Assert.IsNotNull(googleDnsIPAddressesWithFinalDot,
                    "GoogleDnsFqdn with trailing period resolution is not null");
            }
        }

        public class IsPrivateAddress : NetworkTest
        {
            [Test]
            public void PrivateIPWithValidAddress_ReturnsTrue()
            {
                Assert.IsTrue(_privateIP.IsPrivateAddress());
            }

            [Test]
            public void PublicIPWithValidAddress_ReturnsFalse()
            {
                Assert.IsFalse(_publicIP.IsPrivateAddress());
            }
        }

        public class ToUInt32 : NetworkTest
        {
            [Test]
            public void PrivateIPWithValidAddress_ReturnsAddressAsInt()
            {
                Assert.AreEqual(3232235777, _privateIP.ToUInt32());
            }

            [Test]
            public void PublicIPWithValidAddress_ReturnsAddressAsInt()
            {
                Assert.AreEqual(3355508993, _publicIP.ToUInt32());
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

        public class GetPublicIPAddress : NetworkTest
        {
            [Test]
            public void WithNoParam_ReturnsIPAddress()
            {
                var publicIPAddress = Network.GetPublicIPAddress();

                Assert.IsNotEmpty(publicIPAddress.ToString());
            }
        }

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
                var dnsHost = await Network.GetDnsHostEntryAsync(Fqdn, _googleDns, Definitions.DnsDefaultPort, default(CancellationToken));

                Assert.IsNotEmpty(dnsHost.ToString());
            }
        }

        public class GetDnsPointerEntryAsync : NetworkTest
        {
            [Test]
            public async Task WithValidFqdnAndIPAddress_ReturnsDnsHost()
            {
                var dnsPointer = await Network.GetDnsPointerEntryAsync(_googleDns, _googleDns, Definitions.DnsDefaultPort, default(CancellationToken));

                Assert.AreEqual(dnsPointer.ToString(), GoogleDnsFqdn);
            }

            [Test]
            public async Task WithValidIPAddress_ReturnsDnsHost()
            {
                var dnsPointer = await Network.GetDnsPointerEntryAsync(_googleDns);

                Assert.AreEqual(dnsPointer.ToString(), GoogleDnsFqdn);
            }

        }

        public class QueryDnsAsync : NetworkTest
        {
            [Test]
            public async Task ValidDnsAsDnsServer_ReturnsQueryDns()
            {
                var dnsPointer = await Network.QueryDnsAsync(GoogleDnsFqdn, DnsRecordType.MX, _googleDns, Definitions.DnsDefaultPort);

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
}