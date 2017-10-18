using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
            public void InvalidDnsAsParam_DnsQueryExceptionThrown()
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
            public void WithIPv6Address_ArgumentExceptionThrown()
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

                Assert.AreEqual(publicIPAddress.ToString(), "187.188.190.146");
            }
        }

        public class GetNetworkTimeUtc : NetworkTest
        {
            [Test]
            public void WithNoParam_ReturnsDateTime()
            {
                var publicIPAddress = Network.GetNetworkTimeUtc();
                
                Assert.That(publicIPAddress, Is.EqualTo(DateTime.Now).Within(301).Minutes);
            }

            
        }

    }
}