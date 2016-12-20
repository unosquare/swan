namespace Unosquare.Swan.Utilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a response from a DNS server
    /// </summary>
    public class DnsQueryResponse
    {

        private readonly List<DnsRecord> m_AnswerRecords = new List<DnsRecord>();
        private readonly List<DnsRecord> m_AdditionalRecords = new List<DnsRecord>();
        private readonly List<DnsRecord> m_AuthorityRecords = new List<DnsRecord>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsQueryResponse"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        internal DnsQueryResponse(DnsClient.DnsClientResponse response)
        {
            Id = response.Id;
            IsAuthoritativeServer = response.IsAuthorativeServer;
            IsRecursionAvailable = response.IsRecursionAvailable;
            IsTruncated = response.IsTruncated;
            OperationCode = response.OperationCode;
            ResponseCode = response.ResponseCode;

            if (response.AnswerRecords != null)
                foreach (var record in response.AnswerRecords)
                    AnswerRecords.Add(new DnsRecord(record));

            if (response.AuthorityRecords != null)
                foreach (var record in response.AuthorityRecords)
                    AuthorityRecords.Add(new DnsRecord(record));

            if (response.AdditionalRecords != null)
                foreach (var record in response.AdditionalRecords)
                    AdditionalRecords.Add(new DnsRecord(record));
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is authoritative server.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is authoritative server; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthoritativeServer { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is truncated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is truncated; otherwise, <c>false</c>.
        /// </value>
        public bool IsTruncated { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is recursion available.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is recursion available; otherwise, <c>false</c>.
        /// </value>
        public bool IsRecursionAvailable { get; private set; }

        /// <summary>
        /// Gets the operation code.
        /// </summary>
        /// <value>
        /// The operation code.
        /// </value>
        public DnsOperationCode OperationCode { get; private set; }

        /// <summary>
        /// Gets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public DnsResponseCode ResponseCode { get; private set; }

        /// <summary>
        /// Gets the answer records.
        /// </summary>
        /// <value>
        /// The answer records.
        /// </value>
        public IList<DnsRecord> AnswerRecords { get { return m_AnswerRecords; } }

        /// <summary>
        /// Gets the additional records.
        /// </summary>
        /// <value>
        /// The additional records.
        /// </value>
        public IList<DnsRecord> AdditionalRecords { get { return m_AdditionalRecords; } }

        /// <summary>
        /// Gets the authority records.
        /// </summary>
        /// <value>
        /// The authority records.
        /// </value>
        public IList<DnsRecord> AuthorityRecords { get { return m_AuthorityRecords; } }
    }
}
