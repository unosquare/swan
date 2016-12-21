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

                this.message = response.ToArray();
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

            public IList<IDnsResourceRecord> AnswerRecords
            {
                get { return response.AnswerRecords; }
            }

            public IList<IDnsResourceRecord> AuthorityRecords
            {
                get { return new ReadOnlyCollection<IDnsResourceRecord>(response.AuthorityRecords); }
            }

            public IList<IDnsResourceRecord> AdditionalRecords
            {
                get { return new ReadOnlyCollection<IDnsResourceRecord>(response.AdditionalRecords); }
            }

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

            public IList<DnsQuestion> Questions
            {
                get { return new ReadOnlyCollection<DnsQuestion>(response.Questions); }
            }

            public int Size
            {
                get { return message.Length; }
            }

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
                this.header = new DnsHeader();
                this.questions = new List<DnsQuestion>();
                this.answers = new List<IDnsResourceRecord>();
                this.authority = new List<IDnsResourceRecord>();
                this.additional = new List<IDnsResourceRecord>();

                this.header.Response = true;
                this.header.Id = RANDOM.Next(UInt16.MaxValue);
            }

            public DnsResponse(IDnsResponse response)
            {
                this.header = new DnsHeader();
                this.questions = new List<DnsQuestion>(response.Questions);
                this.answers = new List<IDnsResourceRecord>(response.AnswerRecords);
                this.authority = new List<IDnsResourceRecord>(response.AuthorityRecords);
                this.additional = new List<IDnsResourceRecord>(response.AdditionalRecords);

                this.header.Response = true;

                Id = response.Id;
                IsRecursionAvailable = response.IsRecursionAvailable;
                IsAuthorativeServer = response.IsAuthorativeServer;
                OperationCode = response.OperationCode;
                ResponseCode = response.ResponseCode;
            }

            public IList<DnsQuestion> Questions
            {
                get { return questions; }
            }

            public IList<IDnsResourceRecord> AnswerRecords
            {
                get { return answers; }
            }

            public IList<IDnsResourceRecord> AuthorityRecords
            {
                get { return authority; }
            }

            public IList<IDnsResourceRecord> AdditionalRecords
            {
                get { return additional; }
            }

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

                return ObjectStringifier.FromObject(this)
                    .Add(nameof(DnsHeader), header)
                    .Add(nameof(Questions), nameof(AnswerRecords), nameof(AuthorityRecords), nameof(AdditionalRecords))
                    .ToString();
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
