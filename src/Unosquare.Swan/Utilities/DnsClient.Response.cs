namespace Unosquare.Swan.Utilities
{
    using Formatters;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    partial class DnsClient
    {
        public class DnsClientResponse : IDnsResponse
        {
            private DnsResponse response;
            private byte[] message;

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
            private IList<DnsQuestion> questions;
            private IList<IDnsResourceRecord> answers;
            private IList<IDnsResourceRecord> authority;
            private IList<IDnsResourceRecord> additional;

            public static DnsResponse FromRequest(IDnsRequest request)
            {
                var response = new DnsResponse();

                response.Id = request.Id;

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

            public DnsResponse(DnsHeader header, IList<DnsQuestion> questions, IList<IDnsResourceRecord> answers,
                    IList<IDnsResourceRecord> authority, IList<IDnsResourceRecord> additional)
            {
                this.header = header;
                this.questions = questions;
                this.answers = answers;
                this.authority = authority;
                this.additional = additional;
            }

            public DnsResponse()
            {
                header = new DnsHeader();
                questions = new List<DnsQuestion>();
                answers = new List<IDnsResourceRecord>();
                authority = new List<IDnsResourceRecord>();
                additional = new List<IDnsResourceRecord>();

                header.Response = true;
                header.Id = RANDOM.Next(UInt16.MaxValue);
            }

            public DnsResponse(IDnsResponse response)
            {
                header = new DnsHeader();
                questions = new List<DnsQuestion>(response.Questions);
                answers = new List<IDnsResourceRecord>(response.AnswerRecords);
                authority = new List<IDnsResourceRecord>(response.AuthorityRecords);
                additional = new List<IDnsResourceRecord>(response.AdditionalRecords);

                header.Response = true;

                Id = response.Id;
                IsRecursionAvailable = response.IsRecursionAvailable;
                IsAuthorativeServer = response.IsAuthorativeServer;
                OperationCode = response.OperationCode;
                ResponseCode = response.ResponseCode;
            }

            public IList<DnsQuestion> Questions => questions;

            public IList<IDnsResourceRecord> AnswerRecords => answers;

            public IList<IDnsResourceRecord> AuthorityRecords => authority;

            public IList<IDnsResourceRecord> AdditionalRecords => additional;

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
                        questions.Sum(q => q.Size) +
                        answers.Sum(a => a.Size) +
                        authority.Sum(a => a.Size) +
                        additional.Sum(a => a.Size);
                }
            }

            public byte[] ToArray()
            {
                UpdateHeader();
                var result = new MemoryStream(Size);

                result
                    .Append(header.ToArray())
                    .Append(questions.Select(q => q.ToArray()))
                    .Append(answers.Select(a => a.ToArray()))
                    .Append(authority.Select(a => a.ToArray()))
                    .Append(additional.Select(a => a.ToArray()));

                return result.ToArray();
            }

            public override string ToString()
            {
                UpdateHeader();

                return JsonEx.SerializeOnly(this, true,
                    nameof(Questions), nameof(AnswerRecords), nameof(AuthorityRecords), nameof(AdditionalRecords));
            }

            private void UpdateHeader()
            {
                header.QuestionCount = questions.Count;
                header.AnswerRecordCount = answers.Count;
                header.AuthorityRecordCount = authority.Count;
                header.AdditionalRecordCount = additional.Count;
            }
        }

    }
}
