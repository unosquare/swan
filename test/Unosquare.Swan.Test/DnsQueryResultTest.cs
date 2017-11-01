using NUnit.Framework;
using System;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test.DnsQueryResultTest
{
    public abstract class DnsQueryResultTest
    {
        protected const string GoogleDnsFqdn = "google-public-dns-a.google.com";
        protected DnsQueryResult txtRecords = Network.QueryDns(GoogleDnsFqdn, DnsRecordType.TXT);
    }

    [TestFixture]
    public class ID : DnsQueryResultTest
    {
        [Test]
        public void WithValidDns_ReturnsIDn()
        {
            Assert.IsNotNull(txtRecords.Id);
        }
    }

    [TestFixture]
    public class IsAuthoritativeServer : DnsQueryResultTest
    {
        [Test]
        public void WithValidDns_ReturnsFalse()
        {
            Assert.IsFalse(txtRecords.IsAuthoritativeServer);
        }
    }

    [TestFixture]
    public class IsTruncated : DnsQueryResultTest
    {
        [Test]
        public void WithValidDns_ReturnsFalse()
        {
            Assert.IsFalse(txtRecords.IsTruncated);
        }
    }

    [TestFixture]
    public class IsRecursionAvailable : DnsQueryResultTest
    {
        [Test]
        public void WithValidDns_ReturnsTrue()
        {
            Assert.IsTrue(txtRecords.IsRecursionAvailable);
        }
    }

    [TestFixture]
    public class OperationCode : DnsQueryResultTest
    {
        [Test]
        public void WithValidDns_ReturnsOperationCode()
        {
            Assert.IsNotNull(txtRecords.OperationCode);
        }
    }
    
}
