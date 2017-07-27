﻿using NUnit.Framework;
using System.Linq;
using System.Net;
using Unosquare.Swan.Exceptions;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class NetworkTest
    {
        private const string GoogleDnsFqdn = "google-public-dns-a.google.com";
        private readonly IPAddress _privateIP = IPAddress.Parse("192.168.1.1");
        private readonly IPAddress _publicIP = IPAddress.Parse("200.1.1.1");

        [Test]
        public void SimpleResolveIPAddressTest()
        {
            if (Runtime.OS == OperatingSystem.Osx)
                Assert.Inconclusive("OSX is returning time out");

            var googleDnsIPAddresses = Network.GetDnsHostEntry(GoogleDnsFqdn);
            Assert.IsNotNull(googleDnsIPAddresses, "GoogleDnsFqdn resolution is not null");
            
            var googleDnsIPAddressesWithFinalDot = Network.GetDnsHostEntry(GoogleDnsFqdn + ".");
            Assert.IsNotNull(googleDnsIPAddressesWithFinalDot,
                "GoogleDnsFqdn with trailing period resolution is not null");
            
            var targetIP = googleDnsIPAddresses.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            Assert.IsNotNull(targetIP, "Google address is IPv4");

            var googleDnsPtrRecord = Network.GetDnsPointerEntry(targetIP);
            Assert.IsNotNull(googleDnsPtrRecord, "Google address DNS Pointer");

            var resolvedPtrRecord = Network.GetDnsHostEntry(googleDnsPtrRecord);
            Assert.IsNotNull(resolvedPtrRecord);

            var resolvedIP = resolvedPtrRecord.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            Assert.IsNotNull(resolvedIP, "Google resolution is IPv4");

            Assert.IsTrue(resolvedIP.ToString().Equals(targetIP.ToString()));
        }

        [Test]
        public void IsPrivateAddressTest()
        {
            Assert.IsTrue(_privateIP.IsPrivateAddress());
            Assert.IsFalse(_publicIP.IsPrivateAddress());
        }

        [Test]
        public void IPAddressToUint32Test()
        {
            Assert.AreEqual(3232235777, _privateIP.ToUInt32());
            Assert.AreEqual(3355508993, _publicIP.ToUInt32());
        }

        [Test]
        public void QueryDnsTest()
        {
            if (Runtime.OS != OperatingSystem.Windows)
            {
                Assert.Ignore("Ignored");
            }
            else
            {
                var mxRecord = Network.QueryDns(GoogleDnsFqdn, DnsRecordType.MX);

                Assert.IsNotNull(mxRecord);
                Assert.AreEqual(DnsResponseCode.NoError, mxRecord.ResponseCode);

                var txtRecords = Network.QueryDns(GoogleDnsFqdn, DnsRecordType.TXT);

                Assert.IsNotNull(txtRecords);
                Assert.IsTrue(txtRecords.AnswerRecords.Any());
            }
        }

        [Test]
        public void QueryDnsErrorTest()
        {
            if (Runtime.OS == OperatingSystem.Osx)
                Assert.Inconclusive("OSX is returning time out");

            Assert.Throws<DnsQueryException>(() => Network.QueryDns("invalid.local", DnsRecordType.MX));
        }
    }
}