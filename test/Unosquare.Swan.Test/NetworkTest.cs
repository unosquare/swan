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
        private readonly IPAddress _privateIP = IPAddress.Parse("192.168.1.1");
        private readonly IPAddress _publicIP = IPAddress.Parse("200.1.1.1");

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

                Assert.AreEqual(networkType[0].ToString(), "172.16.16.145");
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

                Assert.AreEqual(networkType[0].ToString(), "172.16.16.145");
            }
        }

        public class GetPublicIPAddress : NetworkTest
        {
            [Test]
            public void WithNoParam_ReturnsIPAddress()
            {
                var publicIPAddress = Network.GetPublicIPAddress();
                Console.WriteLine(publicIPAddress.GetType().ToString());

                //Assert.IsNull(publicIPAddress);
                //Assert.AreEqual(publicIPAddress.ToString(), "187.188.190.146");
                Assert.AreEqual(publicIPAddress.GetType().ToString(), "System.Net.IPAddress");
            }
        }

        public class GetNetworkTimeUtc : NetworkTest
        {
            [Test]
            public void WithNoParam_ReturnsDateTime()
            {
                var publicIPAddress = Network.GetNetworkTimeUtc();

                Assert.That(publicIPAddress, Is.EqualTo(DateTime.Now).Within(302).Minutes);
            }

            [Test]
            public void WithInvalidNtpServerName_ThrowsDnsQueryException()
            {
                Assert.Throws<DnsQueryException>(() => Network.GetNetworkTimeUtc("www"));
            }
        }

        public class GetDnsHostEntryAsync : NetworkTest
        {
            [Test]
            public void WithValidFqdn_ReturnsDnsHost()
            {
                string fqdn = "pool.ntp.org";

                var DnsHost = Network.GetDnsHostEntryAsync(fqdn, default(CancellationToken));

                Assert.AreEqual(DnsHost.Result[0].GetType().ToString(), "System.Net.IPAddress");
            }

            [Test]
            public void WithValidFqdnAndIPAddress_ReturnsDnsHost()
            {
                string fqdn = "pool.ntp.org";
                IPAddress iPAddress = IPAddress.Parse("172.16.16.1");

                var DnsHost = Network.GetDnsHostEntryAsync(fqdn, iPAddress, Definitions.DnsDefaultPort, default(CancellationToken));

                Assert.AreEqual(DnsHost.Result[0].GetType().ToString(), "System.Net.IPAddress");
            }
        }

        public class GetNetworkTimeUtcAsync : NetworkTest
        {
            [Test]
            public void WithNtpServerName_ReturnsDateTime()
            {
                string ntpServerName = "pool.ntp.org";
                var publicIPAddress = Network.GetNetworkTimeUtcAsync(ntpServerName);

                Assert.That(publicIPAddress.Result, Is.EqualTo(DateTime.Now).Within(302).Minutes);
            }

            [Test]
            public void WithIPAddress_ReturnsDateTime()
            {
                IPAddress ntpServerAddress = IPAddress.Parse("62.116.162.126");
                var publicIPAddress = Network.GetNetworkTimeUtcAsync(ntpServerAddress);

                Assert.That(publicIPAddress.Result, Is.EqualTo(DateTime.Now).Within(302).Minutes);
            }
        }

        public class DomainName : NetworkTest
        {
            [Test]
            public void WithNoParams_ReturnsDomainName()
            {
                var DomainName = Network.DomainName;
                Console.WriteLine("DomainName " + DomainName);

                Assert.AreEqual(DomainName, "ad.unosquare.com");
            }
        }

        public class GetDnsPointerEntryAsync : NetworkTest
        {
            [Test]
            public void WithValidFqdnAndIPAddress_ReturnsDnsHost()
            {
                IPAddress dnsServer = IPAddress.Parse("172.16.16.1");
                IPAddress iPAddress = IPAddress.Parse("8.8.8.8");

                var DnsPointer = Network.GetDnsPointerEntryAsync(iPAddress, dnsServer, Definitions.DnsDefaultPort, default(CancellationToken));
                
                Assert.AreEqual(DnsPointer.Result.ToString(), GoogleDnsFqdn);
            }

            [Test]
            public void WithValidIPAddress_ReturnsDnsHost()
            {
                IPAddress iPAddress = IPAddress.Parse("8.8.8.8");

                var DnsPointer = Network.GetDnsPointerEntryAsync(iPAddress);
                
                Assert.AreEqual(DnsPointer.Result.ToString(), GoogleDnsFqdn);
            }
            
        }

        public class QueryDnsAsync : NetworkTest
        {
            [Test]
            public void ValidDnsAsDnsServer_ReturnsQueryDns()
            {
                IPAddress dnsServer = IPAddress.Parse("172.16.16.1");

                var DnsPointer = Network.QueryDnsAsync(GoogleDnsFqdn, DnsRecordType.MX, dnsServer, Definitions.DnsDefaultPort);
                
                Assert.AreEqual(DnsResponseCode.NoError, DnsPointer.Result.ResponseCode);
            }

            [Test]
            public void ValidDnsAsParam_ReturnsQueryDns()
            {
                var DnsPointer = Network.QueryDnsAsync(GoogleDnsFqdn, DnsRecordType.MX);

                Assert.AreEqual(DnsResponseCode.NoError, DnsPointer.Result.ResponseCode);
            }

        }


    }
}