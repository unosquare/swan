namespace Unosquare.Swan.Networking
{
    using System;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Represents a DNS record entry
    /// </summary>
    public class DnsRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRecord"/> class.
        /// </summary>
        /// <param name="record">The record.</param>
        internal DnsRecord(DnsClient.IDnsResourceRecord record)
            : this()
        {
            Name = record.Name.ToString();
            Type = record.Type;
            Class = record.Class;
            TimeToLive = record.TimeToLive;
            Data = record.Data;

            // PTR
            PointerDomainName = (record as DnsClient.DnsPointerResourceRecord)?.PointerDomainName?.ToString();

            // A
            IPAddress = (record as DnsClient.DnsIPAddressResourceRecord)?.IPAddress;

            // NS
            NameServerDomainName = (record as DnsClient.DnsNameServerResourceRecord)?.NSDomainName?.ToString();

            // CNAME
            CanonicalDomainName = (record as DnsClient.DnsCanonicalNameResourceRecord)?.CanonicalDomainName.ToString();

            // MX
            MailExchangerDomainName = (record as DnsClient.DnsMailExchangeResourceRecord)?.ExchangeDomainName.ToString();
            MailExchangerPreference = (record as DnsClient.DnsMailExchangeResourceRecord)?.Preference;

            // SOA
            SoaMasterDomainName = (record as DnsClient.DnsStartOfAuthorityResourceRecord)?.MasterDomainName.ToString();
            SoaResponsibleDomainName = (record as DnsClient.DnsStartOfAuthorityResourceRecord)?.ResponsibleDomainName.ToString();
            SoaSerialNumber = (record as DnsClient.DnsStartOfAuthorityResourceRecord)?.SerialNumber;
            SoaRefreshInterval = (record as DnsClient.DnsStartOfAuthorityResourceRecord)?.RefreshInterval;
            SoaRetryInterval = (record as DnsClient.DnsStartOfAuthorityResourceRecord)?.RetryInterval;
            SoaExpireInterval = (record as DnsClient.DnsStartOfAuthorityResourceRecord)?.ExpireInterval;
            SoaMinimumTimeToLive = (record as DnsClient.DnsStartOfAuthorityResourceRecord)?.MinimumTimeToLive;
        }

        private DnsRecord()
        {
            // placeholder
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public DnsRecordType Type { get; }

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        public DnsRecordClass Class { get; }

        /// <summary>
        /// Gets the time to live.
        /// </summary>
        /// <value>
        /// The time to live.
        /// </value>
        public TimeSpan TimeToLive { get; }

        /// <summary>
        /// Gets the raw data of the record.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the data text bytes in ASCII encoding.
        /// </summary>
        /// <value>
        /// The data text.
        /// </value>
        public string DataText => Data == null ? string.Empty : Encoding.ASCII.GetString(Data);

        /// <summary>
        /// Gets the name of the pointer domain.
        /// </summary>
        /// <value>
        /// The name of the pointer domain.
        /// </value>
        public string PointerDomainName { get; }

        /// <summary>
        /// Gets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public IPAddress IPAddress { get; }

        /// <summary>
        /// Gets the name of the name server domain.
        /// </summary>
        /// <value>
        /// The name of the name server domain.
        /// </value>
        public string NameServerDomainName { get; }

        /// <summary>
        /// Gets the name of the canonical domain.
        /// </summary>
        /// <value>
        /// The name of the canonical domain.
        /// </value>
        public string CanonicalDomainName { get; }

        /// <summary>
        /// Gets the mail exchanger preference.
        /// </summary>
        /// <value>
        /// The mail exchanger preference.
        /// </value>
        public int? MailExchangerPreference { get; }

        /// <summary>
        /// Gets the name of the mail exchanger domain.
        /// </summary>
        /// <value>
        /// The name of the mail exchanger domain.
        /// </value>
        public string MailExchangerDomainName { get; }
        
        /// <summary>
        /// Gets the name of the soa master domain.
        /// </summary>
        /// <value>
        /// The name of the soa master domain.
        /// </value>
        public string SoaMasterDomainName { get; }

        /// <summary>
        /// Gets the name of the soa responsible domain.
        /// </summary>
        /// <value>
        /// The name of the soa responsible domain.
        /// </value>
        public string SoaResponsibleDomainName { get; }

        /// <summary>
        /// Gets the soa serial number.
        /// </summary>
        /// <value>
        /// The soa serial number.
        /// </value>
        public long? SoaSerialNumber { get; }

        /// <summary>
        /// Gets the soa refresh interval.
        /// </summary>
        /// <value>
        /// The soa refresh interval.
        /// </value>
        public TimeSpan? SoaRefreshInterval { get; }

        /// <summary>
        /// Gets the soa retry interval.
        /// </summary>
        /// <value>
        /// The soa retry interval.
        /// </value>
        public TimeSpan? SoaRetryInterval { get; }

        /// <summary>
        /// Gets the soa expire interval.
        /// </summary>
        /// <value>
        /// The soa expire interval.
        /// </value>
        public TimeSpan? SoaExpireInterval { get; }

        /// <summary>
        /// Gets the soa minimum time to live.
        /// </summary>
        /// <value>
        /// The soa minimum time to live.
        /// </value>
        public TimeSpan? SoaMinimumTimeToLive { get; }
    }
}
