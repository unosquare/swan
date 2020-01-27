﻿namespace Swan.Net.Dns
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a response from a DNS server.
    /// </summary>
    public class DnsQueryResult
    {
        private readonly List<DnsRecord> _mAnswerRecords = new List<DnsRecord>();
        private readonly List<DnsRecord> _mAdditionalRecords = new List<DnsRecord>();
        private readonly List<DnsRecord> _mAuthorityRecords = new List<DnsRecord>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsQueryResult"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        internal DnsQueryResult(DnsClient.IDnsResponse response)
            : this()
        {
            Id = response.Id;
            IsAuthoritativeServer = response.IsAuthorativeServer;
            IsRecursionAvailable = response.IsRecursionAvailable;
            IsTruncated = response.IsTruncated;
            OperationCode = response.OperationCode;
            ResponseCode = response.ResponseCode;

            if (response.AnswerRecords != null)
            {
                foreach (var record in response.AnswerRecords)
                    AnswerRecords.Add(new DnsRecord(record));
            }

            if (response.AuthorityRecords != null)
            {
                foreach (var record in response.AuthorityRecords)
                    AuthorityRecords.Add(new DnsRecord(record));
            }

            if (response.AdditionalRecords != null)
            {
                foreach (var record in response.AdditionalRecords)
                    AdditionalRecords.Add(new DnsRecord(record));
            }
        }

        private DnsQueryResult()
        {
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is authoritative server.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is authoritative server; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthoritativeServer { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is truncated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is truncated; otherwise, <c>false</c>.
        /// </value>
        public bool IsTruncated { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is recursion available.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is recursion available; otherwise, <c>false</c>.
        /// </value>
        public bool IsRecursionAvailable { get; }

        /// <summary>
        /// Gets the operation code.
        /// </summary>
        /// <value>
        /// The operation code.
        /// </value>
        public DnsOperationCode OperationCode { get; }

        /// <summary>
        /// Gets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public DnsResponseCode ResponseCode { get; }

        /// <summary>
        /// Gets the answer records.
        /// </summary>
        /// <value>
        /// The answer records.
        /// </value>
        public IList<DnsRecord> AnswerRecords => _mAnswerRecords;

        /// <summary>
        /// Gets the additional records.
        /// </summary>
        /// <value>
        /// The additional records.
        /// </value>
        public IList<DnsRecord> AdditionalRecords => _mAdditionalRecords;

        /// <summary>
        /// Gets the authority records.
        /// </summary>
        /// <value>
        /// The authority records.
        /// </value>
        public IList<DnsRecord> AuthorityRecords => _mAuthorityRecords;
    }
}
