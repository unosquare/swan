namespace Unosquare.Swan.Networking
{
    using Formatters;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    internal partial class DnsClient
    {
        public class DnsClientResponse : IDnsResponse
        {
            private readonly DnsResponse response;
            private readonly byte[] message;

            public static DnsClientResponse FromArray(DnsClientRequest request, byte[] message)
            {
                var response = DnsResponse.FromArray(message);
                return new DnsClientResponse(request, response, message);
            }

            internal DnsClientResponse(DnsClientRequest request, DnsResponse response, byte[] message)
            {
                Request = request;

                this.message = message;
                this.response = response;
            }

            internal DnsClientResponse(DnsClientRequest request, DnsResponse response)
            {
                Request = request;

                message = response.ToArray();
                this.response = response;
            }

            public DnsClientRequest Request
            {
                get;
                private set;
            }

            public int Id
            {
                get { return response.Id; }
                set { }
            }

            public IList<IDnsResourceRecord> AnswerRecords => response.AnswerRecords;

            public IList<IDnsResourceRecord> AuthorityRecords => new ReadOnlyCollection<IDnsResourceRecord>(response.AuthorityRecords);

            public IList<IDnsResourceRecord> AdditionalRecords => new ReadOnlyCollection<IDnsResourceRecord>(response.AdditionalRecords);

            public bool IsRecursionAvailable
            {
                get { return response.IsRecursionAvailable; }
                set { }
            }

            public bool IsAuthorativeServer
            {
                get { return response.IsAuthorativeServer; }
                set { }
            }

            public bool IsTruncated
            {
                get { return response.IsTruncated; }
                set { }
            }

            public DnsOperationCode OperationCode
            {
                get { return response.OperationCode; }
                set { }
            }

            public DnsResponseCode ResponseCode
            {
                get { return response.ResponseCode; }
                set { }
            }

            public IList<DnsQuestion> Questions => new ReadOnlyCollection<DnsQuestion>(response.Questions);

            public int Size => message.Length;

            public byte[] ToArray()
            {
                return message;
            }

            public override string ToString()
            {
                return response.ToString();
            }
        }
        
        public class DnsResponse : IDnsResponse
        {
            private static readonly Random RANDOM = new Random();

            private DnsHeader header;

            public static DnsResponse FromRequest(IDnsRequest request)
            {
                var response = new DnsResponse {Id = request.Id};
                
                foreach (DnsQuestion question in request.Questions)
                {
                    response.Questions.Add(question);
                }

                return response;
            }

            public static DnsResponse FromArray(byte[] message)
            {
                var header = DnsHeader.FromArray(message);
                var offset = header.Size;

                if (!header.Response || header.QuestionCount == 0)
                {
                    throw new ArgumentException("Invalid response message");
                }

                if (header.Truncated)
                {
                    return new DnsResponse(header,
                        DnsQuestion.GetAllFromArray(message, offset, header.QuestionCount),
                        new List<IDnsResourceRecord>(),
                        new List<IDnsResourceRecord>(),
                        new List<IDnsResourceRecord>());
                }

                return new DnsResponse(header,
                    DnsQuestion.GetAllFromArray(message, offset, header.QuestionCount, out offset),
                    DnsResourceRecordFactory.GetAllFromArray(message, offset, header.AnswerRecordCount, out offset),
                    DnsResourceRecordFactory.GetAllFromArray(message, offset, header.AuthorityRecordCount, out offset),
                    DnsResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out offset));
            }

            public DnsResponse(
                DnsHeader header, 
                IList<DnsQuestion> questions, 
                IList<IDnsResourceRecord> answers,
                IList<IDnsResourceRecord> authority, 
                IList<IDnsResourceRecord> additional)
            {
                this.header = header;
                this.Questions = questions;
                this.AnswerRecords = answers;
                this.AuthorityRecords = authority;
                this.AdditionalRecords = additional;
            }

            public DnsResponse()
            {
                header = new DnsHeader();
                Questions = new List<DnsQuestion>();
                AnswerRecords = new List<IDnsResourceRecord>();
                AuthorityRecords = new List<IDnsResourceRecord>();
                AdditionalRecords = new List<IDnsResourceRecord>();

                header.Response = true;
                header.Id = RANDOM.Next(UInt16.MaxValue);
            }

            public DnsResponse(IDnsResponse response)
            {
                header = new DnsHeader();
                Questions = new List<DnsQuestion>(response.Questions);
                AnswerRecords = new List<IDnsResourceRecord>(response.AnswerRecords);
                AuthorityRecords = new List<IDnsResourceRecord>(response.AuthorityRecords);
                AdditionalRecords = new List<IDnsResourceRecord>(response.AdditionalRecords);

                header.Response = true;

                Id = response.Id;
                IsRecursionAvailable = response.IsRecursionAvailable;
                IsAuthorativeServer = response.IsAuthorativeServer;
                OperationCode = response.OperationCode;
                ResponseCode = response.ResponseCode;
            }

            public IList<DnsQuestion> Questions { get; }

            public IList<IDnsResourceRecord> AnswerRecords { get; }

            public IList<IDnsResourceRecord> AuthorityRecords { get; }

            public IList<IDnsResourceRecord> AdditionalRecords { get; }

            public int Id
            {
                get { return header.Id; }
                set { header.Id = value; }
            }

            public bool IsRecursionAvailable
            {
                get { return header.RecursionAvailable; }
                set { header.RecursionAvailable = value; }
            }

            public bool IsAuthorativeServer
            {
                get { return header.AuthorativeServer; }
                set { header.AuthorativeServer = value; }
            }

            public bool IsTruncated
            {
                get { return header.Truncated; }
                set { header.Truncated = value; }
            }

            public DnsOperationCode OperationCode
            {
                get { return header.OperationCode; }
                set { header.OperationCode = value; }
            }

            public DnsResponseCode ResponseCode
            {
                get { return header.ResponseCode; }
                set { header.ResponseCode = value; }
            }

            public int Size
            {
                get
                {
                    return header.Size +
                        Questions.Sum(q => q.Size) +
                        AnswerRecords.Sum(a => a.Size) +
                        AuthorityRecords.Sum(a => a.Size) +
                        AdditionalRecords.Sum(a => a.Size);
                }
            }

            public byte[] ToArray()
            {
                UpdateHeader();
                var result = new MemoryStream(Size);

                result
                    .Append(header.ToArray())
                    .Append(Questions.Select(q => q.ToArray()))
                    .Append(AnswerRecords.Select(a => a.ToArray()))
                    .Append(AuthorityRecords.Select(a => a.ToArray()))
                    .Append(AdditionalRecords.Select(a => a.ToArray()));

                return result.ToArray();
            }

            public override string ToString()
            {
                UpdateHeader();

                return Json.SerializeOnly(
                    this, 
                    true,
                    nameof(Questions), 
                    nameof(AnswerRecords), 
                    nameof(AuthorityRecords), 
                    nameof(AdditionalRecords));
            }

            private void UpdateHeader()
            {
                header.QuestionCount = Questions.Count;
                header.AnswerRecordCount = AnswerRecords.Count;
                header.AuthorityRecordCount = AuthorityRecords.Count;
                header.AdditionalRecordCount = AdditionalRecords.Count;
            }
        }
    }
}
