namespace Unosquare.Swan.Utilities
{
    using Formatters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;

    partial class DnsClient
    {
        public class DnsClientRequest : IDnsRequest
        {

            private IPEndPoint dns;
            private IDnsRequestResolver resolver;
            private IDnsRequest request;

            public DnsClientRequest(IPEndPoint dns, IDnsRequest request = null, IDnsRequestResolver resolver = null)
            {
                this.dns = dns;
                this.request = request == null ? new DnsRequest() : new DnsRequest(request);
                this.resolver = resolver == null ? new DnsUdpRequestResolver() : resolver;
            }

            public DnsClientRequest(IPAddress ip, int port = Constants.DnsDefaultPort, IDnsRequest request = null, IDnsRequestResolver resolver = null) :
                this(new IPEndPoint(ip, port), request, resolver)
            { }

            public DnsClientRequest(string ip, int port = Constants.DnsDefaultPort, IDnsRequest request = null, IDnsRequestResolver resolver = null) :
                this(IPAddress.Parse(ip), port, request, resolver)
            { }

            public int Id
            {
                get { return request.Id; }
                set { request.Id = value; }
            }

            public DnsOperationCode OperationCode
            {
                get { return request.OperationCode; }
                set { request.OperationCode = value; }
            }

            public bool RecursionDesired
            {
                get { return request.RecursionDesired; }
                set { request.RecursionDesired = value; }
            }

            public IList<DnsQuestion> Questions
            {
                get { return request.Questions; }
            }

            public int Size
            {
                get { return request.Size; }
            }

            public byte[] ToArray()
            {
                return request.ToArray();
            }

            public override string ToString()
            {
                return request.ToString();
            }

            public IPEndPoint Dns
            {
                get { return dns; }
                set { dns = value; }
            }

            /// <summary>
            /// Resolves this request into a response using the provided DNS information. The given
            /// request strategy is used to retrieve the response.
            /// </summary>
            /// <exception cref="DnsResponseException">Throw if a malformed response is received from the server</exception>
            /// <exception cref="IOException">Thrown if a IO error occurs</exception>
            /// <exception cref="SocketException">Thrown if a the reading or writing to the socket fails</exception>
            /// <returns>The response received from server</returns>
            public DnsClientResponse Resolve()
            {
                try
                {
                    DnsClientResponse response = resolver.Request(this);

                    if (response.Id != this.Id)
                    {
                        throw new DnsResponseException(response, "Mismatching request/response IDs");
                    }
                    if (response.ResponseCode != DnsResponseCode.NoError)
                    {
                        throw new DnsResponseException(response);
                    }

                    return response;
                }
                catch (ArgumentException e)
                {
                    throw new DnsResponseException("Invalid response", e);
                }
            }
        }

        public class DnsRequest : IDnsRequest
        {
            private static readonly Random RANDOM = new Random();

            private IList<DnsQuestion> questions;
            private DnsHeader header;

            public static DnsRequest FromArray(byte[] message)
            {
                DnsHeader header = DnsHeader.FromArray(message);

                if (header.Response || header.QuestionCount == 0 ||
                        header.AdditionalRecordCount + header.AnswerRecordCount + header.AuthorityRecordCount > 0 ||
                        header.ResponseCode != DnsResponseCode.NoError)
                {

                    throw new ArgumentException("Invalid request message");
                }

                return new DnsRequest(header, DnsQuestion.GetAllFromArray(message, header.Size, header.QuestionCount));
            }

            public DnsRequest(DnsHeader header, IList<DnsQuestion> questions)
            {
                this.header = header;
                this.questions = questions;
            }

            public DnsRequest()
            {
                this.questions = new List<DnsQuestion>();
                this.header = new DnsHeader();

                this.header.OperationCode = DnsOperationCode.Query;
                this.header.Response = false;
                this.header.Id = RANDOM.Next(UInt16.MaxValue);
            }

            public DnsRequest(IDnsRequest request)
            {
                this.header = new DnsHeader();
                this.questions = new List<DnsQuestion>(request.Questions);

                this.header.Response = false;

                Id = request.Id;
                OperationCode = request.OperationCode;
                RecursionDesired = request.RecursionDesired;
            }

            public IList<DnsQuestion> Questions
            {
                get { return questions; }
            }

            public int Size
            {
                get { return header.Size + questions.Sum(q => q.Size); }
            }

            public int Id
            {
                get { return header.Id; }
                set { header.Id = value; }
            }

            public DnsOperationCode OperationCode
            {
                get { return header.OperationCode; }
                set { header.OperationCode = value; }
            }

            public bool RecursionDesired
            {
                get { return header.RecursionDesired; }
                set { header.RecursionDesired = value; }
            }

            public byte[] ToArray()
            {
                UpdateHeader();
                var result = new MemoryStream(Size);

                result
                    .Append(header.ToArray())
                    .Append(questions.Select(q => q.ToArray()));

                return result.ToArray();
            }

            public override string ToString()
            {
                UpdateHeader();

                return ObjectStringifier.FromObject(this)
                    .Add(nameof(DnsHeader), header)
                    .Add(nameof(Questions))
                    .ToString();
            }

            private void UpdateHeader()
            {
                header.QuestionCount = questions.Count;
            }
        }

        public class DnsTcpRequestResolver : IDnsRequestResolver
        {
            public DnsClientResponse Request(DnsClientRequest request)
            {
                var tcp = new TcpClient();

                try
                {
                    tcp.ConnectAsync(request.Dns.Address, request.Dns.Port).RunSynchronously();

                    var stream = tcp.GetStream();
                    byte[] buffer = request.ToArray();
                    byte[] length = BitConverter.GetBytes((ushort)buffer.Length);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(length);
                    }

                    stream.Write(length, 0, length.Length);
                    stream.Write(buffer, 0, buffer.Length);

                    buffer = new byte[2];
                    Read(stream, buffer);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(buffer);
                    }

                    buffer = new byte[BitConverter.ToUInt16(buffer, 0)];
                    Read(stream, buffer);

                    DnsResponse response = DnsResponse.FromArray(buffer);

                    return new DnsClientResponse(request, response, buffer);
                }
                finally
                {
#if NET452
                tcp.Close();
#else
                    tcp.Dispose();
#endif
                }
            }

            private static void Read(Stream stream, byte[] buffer)
            {
                int length = buffer.Length;
                int offset = 0;
                int size = 0;

                while (length > 0 && (size = stream.Read(buffer, offset, length)) > 0)
                {
                    offset += size;
                    length -= size;
                }

                if (length > 0)
                {
                    throw new IOException("Unexpected end of stream");
                }
            }
        }

        public class DnsUdpRequestResolver : IDnsRequestResolver
        {
            private IDnsRequestResolver fallback;

            public DnsUdpRequestResolver(IDnsRequestResolver fallback)
            {
                this.fallback = fallback;
            }

            public DnsUdpRequestResolver()
            {
                this.fallback = new DnsNullRequestResolver();
            }

            public DnsClientResponse Request(DnsClientRequest request)
            {
                UdpClient udp = new UdpClient();
                IPEndPoint dns = request.Dns;

                try
                {
                    udp.Client.SendTimeout = 5000;
                    udp.Client.ReceiveTimeout = 5000;
                    udp.Client.Connect(dns);

                    var bytesWritten = udp.SendAsync(request.ToArray(), request.Size, dns).Result;

                    byte[] buffer = udp.ReceiveAsync().Result.Buffer;
                    DnsResponse response = DnsResponse.FromArray(buffer); //null;

                    if (response.Truncated)
                    {
                        return fallback.Request(request);
                    }

                    return new DnsClientResponse(request, response, buffer);
                }
                finally
                {
#if NET452
                udp.Close();
#else
                    udp.Dispose();
#endif
                }
            }
        }

        public class DnsNullRequestResolver : IDnsRequestResolver
        {
            public DnsClientResponse Request(DnsClientRequest request)
            {
                throw new DnsResponseException("Request failed");
            }
        }

        // 12 bytes message header
        [StructEndianness(Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DnsHeader
        {
            public const int SIZE = 12;

            public static DnsHeader FromArray(byte[] header)
            {
                if (header.Length < SIZE)
                {
                    throw new ArgumentException("Header length too small");
                }

                return header.ToStruct<DnsHeader>(0, SIZE);
            }

            private ushort id;

            private byte flag0;
            private byte flag1;

            // Question count: number of questions in the Question section
            private ushort qdCount;

            // Answer record count: number of records in the Answer section
            private ushort anCount;

            // Authority record count: number of records in the Authority section
            private ushort nsCount;

            // Additional record count: number of records in the Additional section
            private ushort arCount;

            public int Id
            {
                get { return id; }
                set { id = (ushort)value; }
            }

            public int QuestionCount
            {
                get { return qdCount; }
                set { qdCount = (ushort)value; }
            }

            public int AnswerRecordCount
            {
                get { return anCount; }
                set { anCount = (ushort)value; }
            }

            public int AuthorityRecordCount
            {
                get { return nsCount; }
                set { nsCount = (ushort)value; }
            }

            public int AdditionalRecordCount
            {
                get { return arCount; }
                set { arCount = (ushort)value; }
            }

            public bool Response
            {
                get { return Qr == 1; }
                set { Qr = Convert.ToByte(value); }
            }

            public DnsOperationCode OperationCode
            {
                get { return (DnsOperationCode)Opcode; }
                set { Opcode = (byte)value; }
            }

            public bool AuthorativeServer
            {
                get { return Aa == 1; }
                set { Aa = Convert.ToByte(value); }
            }

            public bool Truncated
            {
                get { return Tc == 1; }
                set { Tc = Convert.ToByte(value); }
            }

            public bool RecursionDesired
            {
                get { return Rd == 1; }
                set { Rd = Convert.ToByte(value); }
            }

            public bool RecursionAvailable
            {
                get { return Ra == 1; }
                set { Ra = Convert.ToByte(value); }
            }

            public DnsResponseCode ResponseCode
            {
                get { return (DnsResponseCode)RCode; }
                set { RCode = (byte)value; }
            }

            public int Size
            {
                get { return DnsHeader.SIZE; }
            }

            public byte[] ToArray()
            {
                return this.ToBytes();
            }

            public override string ToString()
            {
                return ObjectStringifier.FromObject(this)
                    .AddAll()
                    .Remove(nameof(Size))
                    .ToString();
            }

            // Query/Response Flag
            private byte Qr
            {
                get { return Flag0.GetBitValueAt(7, 1); }
                set { Flag0 = Flag0.SetBitValueAt(7, 1, value); }
            }

            // Operation Code
            private byte Opcode
            {
                get { return Flag0.GetBitValueAt(3, 4); }
                set { Flag0 = Flag0.SetBitValueAt(3, 4, value); }
            }

            // Authorative Answer Flag
            private byte Aa
            {
                get { return Flag0.GetBitValueAt(2, 1); }
                set { Flag0 = Flag0.SetBitValueAt(2, 1, value); }
            }

            // Truncation Flag
            private byte Tc
            {
                get { return Flag0.GetBitValueAt(1, 1); }
                set { Flag0 = Flag0.SetBitValueAt(1, 1, value); }
            }

            // Recursion Desired
            private byte Rd
            {
                get { return Flag0.GetBitValueAt(0, 1); }
                set { Flag0 = Flag0.SetBitValueAt(0, 1, value); }
            }

            // Recursion Available
            private byte Ra
            {
                get { return Flag1.GetBitValueAt(7, 1); }
                set { Flag1 = Flag1.SetBitValueAt(7, 1, value); }
            }

            // Zero (Reserved)
            private byte Z
            {
                get { return Flag1.GetBitValueAt(4, 3); }
                set { }
            }

            // Response Code
            private byte RCode
            {
                get { return Flag1.GetBitValueAt(0, 4); }
                set { Flag1 = Flag1.SetBitValueAt(0, 4, value); }
            }

            private byte Flag0
            {
                get { return flag0; }
                set { flag0 = value; }
            }

            private byte Flag1
            {
                get { return flag1; }
                set { flag1 = value; }
            }
        }

        public class DnsDomain : IComparable<DnsDomain>
        {
            private string[] labels;

            public static DnsDomain FromString(string domain)
            {
                return new DnsDomain(domain);
            }

            public static DnsDomain FromArray(byte[] message, int offset)
            {
                return FromArray(message, offset, out offset);
            }

            public static DnsDomain FromArray(byte[] message, int offset, out int endOffset)
            {
                IList<byte[]> labels = new List<byte[]>();
                bool endOffsetAssigned = false;
                endOffset = 0;
                byte lengthOrPointer;

                while ((lengthOrPointer = message[offset++]) > 0)
                {
                    // Two heighest bits are set (pointer)
                    if (lengthOrPointer.GetBitValueAt(6, 2) == 3)
                    {
                        if (!endOffsetAssigned)
                        {
                            endOffsetAssigned = true;
                            endOffset = offset + 1;
                        }

                        ushort pointer = lengthOrPointer.GetBitValueAt(0, 6);
                        offset = (pointer << 8) | message[offset];

                        continue;
                    }
                    else if (lengthOrPointer.GetBitValueAt(6, 2) != 0)
                    {
                        throw new ArgumentException("Unexpected bit pattern in label length");
                    }

                    byte length = lengthOrPointer;
                    byte[] label = new byte[length];
                    Array.Copy(message, offset, label, 0, length);

                    labels.Add(label);

                    offset += length;
                }

                if (!endOffsetAssigned)
                {
                    endOffset = offset;
                }

                return new DnsDomain(labels.Select(l => Encoding.ASCII.GetString(l)).ToArray());
            }

            public static DnsDomain PointerName(IPAddress ip)
            {
                return new DnsDomain(FormatReverseIP(ip));
            }

            private static string FormatReverseIP(IPAddress ip)
            {
                byte[] address = ip.GetAddressBytes();

                if (address.Length == 4)
                {
                    return string.Join(".", address.Reverse().Select(b => b.ToString())) + ".in-addr.arpa";
                }

                byte[] nibbles = new byte[address.Length * 2];

                for (int i = 0, j = 0; i < address.Length; i++, j = 2 * i)
                {
                    byte b = address[i];

                    nibbles[j] = b.GetBitValueAt(4, 4);
                    nibbles[j + 1] = b.GetBitValueAt(0, 4);
                }

                return string.Join(".", nibbles.Reverse().Select(b => b.ToString("x"))) + ".ip6.arpa";
            }

            public DnsDomain(string domain) : this(domain.Split('.')) { }

            public DnsDomain(string[] labels)
            {
                this.labels = labels;
            }

            public int Size
            {
                get { return labels.Sum(l => l.Length) + labels.Length + 1; }
            }

            public byte[] ToArray()
            {
                byte[] result = new byte[Size];
                int offset = 0;

                foreach (string label in labels)
                {
                    byte[] l = Encoding.ASCII.GetBytes(label);

                    result[offset++] = (byte)l.Length;
                    l.CopyTo(result, offset);

                    offset += l.Length;
                }

                result[offset] = 0;

                return result;
            }

            public override string ToString()
            {
                return string.Join(".", labels);
            }

            public int CompareTo(DnsDomain other)
            {
                return ToString().CompareTo(other.ToString());
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (!(obj is DnsDomain))
                {
                    return false;
                }

                return CompareTo(obj as DnsDomain) == 0;
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }
        }

        public class DnsQuestion : IDnsMessageEntry
        {
            public static IList<DnsQuestion> GetAllFromArray(byte[] message, int offset, int questionCount)
            {
                return GetAllFromArray(message, offset, questionCount, out offset);
            }

            public static IList<DnsQuestion> GetAllFromArray(byte[] message, int offset, int questionCount, out int endOffset)
            {
                IList<DnsQuestion> questions = new List<DnsQuestion>(questionCount);

                for (int i = 0; i < questionCount; i++)
                {
                    questions.Add(FromArray(message, offset, out offset));
                }

                endOffset = offset;
                return questions;
            }

            public static DnsQuestion FromArray(byte[] message, int offset)
            {
                return FromArray(message, offset, out offset);
            }

            public static DnsQuestion FromArray(byte[] message, int offset, out int endOffset)
            {
                var domain = DnsDomain.FromArray(message, offset, out offset);
                var tail = message.ToStruct<Tail>(offset, Tail.SIZE);

                endOffset = offset + Tail.SIZE;

                return new DnsQuestion(domain, tail.Type, tail.Class);
            }

            private DnsDomain domain;
            private DnsRecordType type;
            private DnsRecordClass klass;

            public DnsQuestion(DnsDomain domain, DnsRecordType type = DnsRecordType.A, DnsRecordClass klass = DnsRecordClass.IN)
            {
                this.domain = domain;
                this.type = type;
                this.klass = klass;
            }

            public DnsDomain Name
            {
                get { return domain; }
            }

            public DnsRecordType Type
            {
                get { return type; }
            }

            public DnsRecordClass Class
            {
                get { return klass; }
            }

            public int Size
            {
                get { return domain.Size + Tail.SIZE; }
            }

            public byte[] ToArray()
            {
                var result = new MemoryStream(Size);

                result
                    .Append(domain.ToArray())
                    .Append((new Tail { Type = Type, Class = Class }).ToBytes());

                return result.ToArray();
            }

            public override string ToString()
            {
                return ObjectStringifier.FromObject(this)
                    .Add(nameof(Name), nameof(Type), nameof(Class))
                    .ToString();
            }

            [StructEndianness(Endianness.Big)]
            [StructLayout(LayoutKind.Sequential, Pack = 2)]
            private struct Tail
            {
                public const int SIZE = 4;

                private ushort type;
                private ushort klass;

                public DnsRecordType Type
                {
                    get { return (DnsRecordType)type; }
                    set { type = (ushort)value; }
                }

                public DnsRecordClass Class
                {
                    get { return (DnsRecordClass)klass; }
                    set { klass = (ushort)value; }
                }
            }
        }


    }
}
