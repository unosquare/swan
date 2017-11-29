namespace Unosquare.Swan.Networking
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// DnsClient public interfaces
    /// </summary>
    internal partial class DnsClient
    {
        public interface IDnsMessage
        {
            IList<DnsQuestion> Questions { get; }

            int Size { get; }
            byte[] ToArray();
        }

        public interface IDnsMessageEntry
        {
            DnsDomain Name { get; }
            DnsRecordType Type { get; }
            DnsRecordClass Class { get; }

            int Size { get; }
            byte[] ToArray();
        }

        public interface IDnsResourceRecord : IDnsMessageEntry
        {
            TimeSpan TimeToLive { get; }
            int DataLength { get; }
            byte[] Data { get; }
        }

        public interface IDnsRequest : IDnsMessage
        {
            int Id { get; set; }
            DnsOperationCode OperationCode { get; set; }
            bool RecursionDesired { get; set; }
        }

        public interface IDnsResponse : IDnsMessage
        {
            int Id { get; set; }
            IList<IDnsResourceRecord> AnswerRecords { get; }
            IList<IDnsResourceRecord> AuthorityRecords { get; }
            IList<IDnsResourceRecord> AdditionalRecords { get; }
            bool IsRecursionAvailable { get; set; }
            bool IsAuthorativeServer { get; set; }
            bool IsTruncated { get; set; }
            DnsOperationCode OperationCode { get; set; }
            DnsResponseCode ResponseCode { get; set; }
        }
        
        public interface IDnsRequestResolver
        {
            DnsClientResponse Request(DnsClientRequest request);
        }
    }
}
