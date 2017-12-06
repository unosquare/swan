namespace Unosquare.Swan.Networking
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
    using Exceptions;
    using Attributes;

    /// <summary>
    /// DnsClient Request inner class
    /// </summary>
    internal partial class DnsClient
    {
        public class DnsClientRequest : IDnsRequest
        {
            private readonly IDnsRequestResolver _resolver;
            private readonly IDnsRequest _request;

            public DnsClientRequest(IPEndPoint dns, IDnsRequest request = null, IDnsRequestResolver resolver = null)
            {
                Dns = dns;
                _request = request == null ? new DnsRequest() : new DnsRequest(request);
                _resolver = resolver ?? new DnsUdpRequestResolver();
            }

            public int Id
            {
                get => _request.Id;
                set => _request.Id = value;
            }

            public DnsOperationCode OperationCode
            {
                get => _request.OperationCode;
                set => _request.OperationCode = value;
            }

            public bool RecursionDesired
            {
                get => _request.RecursionDesired;
                set => _request.RecursionDesired = value;
            }

            public IList<DnsQuestion> Questions => _request.Questions;

            public int Size => _request.Size;

            public IPEndPoint Dns { get; set; }

            public byte[] ToArray() => _request.ToArray();

            public override string ToString() => _request.ToString();

            /// <summary>
            /// Resolves this request into a response using the provided DNS information. The given
            /// request strategy is used to retrieve the response.
            /// </summary>
            /// <exception cref="DnsQueryException">Throw if a malformed response is received from the server</exception>
            /// <exception cref="IOException">Thrown if a IO error occurs</exception>
            /// <exception cref="SocketException">Thrown if a the reading or writing to the socket fails</exception>
            /// <returns>The response received from server</returns>
            public DnsClientResponse Resolve()
            {
                try
                {
                    var response = _resolver.Request(this);

                    if (response.Id != Id)
                    {
                        throw new DnsQueryException(response, "Mismatching request/response IDs");
                    }

                    if (response.ResponseCode != DnsResponseCode.NoError)
                    {
                        throw new DnsQueryException(response);
                    }

                    return response;
                }
                catch (ArgumentException e)
                {
                    throw new DnsQueryException("Invalid response", e);
                }
            }
        }

        public class DnsRequest : IDnsRequest
        {
            private static readonly Random Random = new Random();

            private readonly IList<DnsQuestion> questions;
            private DnsHeader header;

            public DnsRequest()
            {
                questions = new List<DnsQuestion>();
                header = new DnsHeader
                {
                    OperationCode = DnsOperationCode.Query,
                    Response = false,
                    Id = Random.Next(UInt16.MaxValue)
                };
            }

            public DnsRequest(IDnsRequest request)
            {
                header = new DnsHeader();
                questions = new List<DnsQuestion>(request.Questions);

                header.Response = false;

                Id = request.Id;
                OperationCode = request.OperationCode;
                RecursionDesired = request.RecursionDesired;
            }

            public IList<DnsQuestion> Questions => questions;

            public int Size
            {
                get { return header.Size + questions.Sum(q => q.Size); }
            }

            public int Id
            {
                get => header.Id;
                set => header.Id = value;
            }

            public DnsOperationCode OperationCode
            {
                get => header.OperationCode;
                set => header.OperationCode = value;
            }

            public bool RecursionDesired
            {
                get => header.RecursionDesired;
                set => header.RecursionDesired = value;
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

                return Json.Serialize(this, true);
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
                    tcp.Client.Connect(request.Dns);

                    var stream = tcp.GetStream();
                    var buffer = request.ToArray();
                    var length = BitConverter.GetBytes((ushort) buffer.Length);

                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(length);

                    stream.Write(length, 0, length.Length);
                    stream.Write(buffer, 0, buffer.Length);

                    buffer = new byte[2];
                    Read(stream, buffer);

                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(buffer);

                    buffer = new byte[BitConverter.ToUInt16(buffer, 0)];
                    Read(stream, buffer);

                    var response = DnsResponse.FromArray(buffer);

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
                var length = buffer.Length;
                var offset = 0;
                int size;

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
            private readonly IDnsRequestResolver _fallback;

            public DnsUdpRequestResolver(IDnsRequestResolver fallback)
            {
                _fallback = fallback;
            }

            public DnsUdpRequestResolver()
            {
                _fallback = new DnsNullRequestResolver();
            }

            public DnsClientResponse Request(DnsClientRequest request)
            {
                var udp = new UdpClient();
                var dns = request.Dns;

                try
                {
                    udp.Client.SendTimeout = 7000;
                    udp.Client.ReceiveTimeout = 7000;
                    udp.Client.Connect(dns);
                    udp.Client.Send(request.ToArray());

                    var bufferList = new List<byte>();

                    do
                    {
                        var tempBuffer = new byte[1024];
                        var receiveCount = udp.Client.Receive(tempBuffer);
                        bufferList.AddRange(tempBuffer.Skip(0).Take(receiveCount));
                    }
                    while (udp.Client.Available > 0 || bufferList.Count == 0);

                    var buffer = bufferList.ToArray();
                    var response = DnsResponse.FromArray(buffer);

                    return response.IsTruncated
                        ? _fallback.Request(request)
                        : new DnsClientResponse(request, response, buffer);
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
                throw new DnsQueryException("Request failed");
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
            private ushort questionCount;

            // Answer record count: number of records in the Answer section
            private ushort answerCount;

            // Authority record count: number of records in the Authority section
            private ushort authorityCount;

            // Additional record count: number of records in the Additional section
            private ushort addtionalCount;

            public int Id
            {
                get => id;
                set => id = (ushort) value;
            }

            public int QuestionCount
            {
                get => questionCount;
                set => questionCount = (ushort) value;
            }

            public int AnswerRecordCount
            {
                get => answerCount;
                set => answerCount = (ushort) value;
            }

            public int AuthorityRecordCount
            {
                get => authorityCount;
                set => authorityCount = (ushort) value;
            }

            public int AdditionalRecordCount
            {
                get => addtionalCount;
                set => addtionalCount = (ushort) value;
            }

            public bool Response
            {
                get => Qr == 1;
                set => Qr = Convert.ToByte(value);
            }

            public DnsOperationCode OperationCode
            {
                get => (DnsOperationCode) Opcode;
                set => Opcode = (byte) value;
            }

            public bool AuthorativeServer
            {
                get => Aa == 1;
                set => Aa = Convert.ToByte(value);
            }

            public bool Truncated
            {
                get => Tc == 1;
                set => Tc = Convert.ToByte(value);
            }

            public bool RecursionDesired
            {
                get => Rd == 1;
                set => Rd = Convert.ToByte(value);
            }

            public bool RecursionAvailable
            {
                get => Ra == 1;
                set => Ra = Convert.ToByte(value);
            }

            public DnsResponseCode ResponseCode
            {
                get => (DnsResponseCode) RCode;
                set => RCode = (byte) value;
            }

            public int Size => SIZE;

            // Query/Response Flag
            private byte Qr
            {
                get => Flag0.GetBitValueAt(7);
                set => Flag0 = Flag0.SetBitValueAt(7, 1, value);
            }

            // Operation Code
            private byte Opcode
            {
                get => Flag0.GetBitValueAt(3, 4);
                set => Flag0 = Flag0.SetBitValueAt(3, 4, value);
            }

            // Authorative Answer Flag
            private byte Aa
            {
                get => Flag0.GetBitValueAt(2);
                set => Flag0 = Flag0.SetBitValueAt(2, 1, value);
            }

            // Truncation Flag
            private byte Tc
            {
                get => Flag0.GetBitValueAt(1);
                set => Flag0 = Flag0.SetBitValueAt(1, 1, value);
            }

            // Recursion Desired
            private byte Rd
            {
                get => Flag0.GetBitValueAt(0);
                set => Flag0 = Flag0.SetBitValueAt(0, 1, value);
            }

            // Recursion Available
            private byte Ra
            {
                get => Flag1.GetBitValueAt(7);
                set => Flag1 = Flag1.SetBitValueAt(7, 1, value);
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
                get => Flag1.GetBitValueAt(0, 4);
                set => Flag1 = Flag1.SetBitValueAt(0, 4, value);
            }

            private byte Flag0
            {
                get => flag0;
                set => flag0 = value;
            }

            private byte Flag1
            {
                get => flag1;
                set => flag1 = value;
            }

            public byte[] ToArray() => this.ToBytes();

            public override string ToString()
                => Json.SerializeExcluding(this, true, nameof(Size));
        }

        public class DnsDomain : IComparable<DnsDomain>
        {
            private readonly string[] _labels;

            public DnsDomain(string domain)
                : this(domain.Split('.'))
            {
            }

            public DnsDomain(string[] labels)
            {
                _labels = labels;
            }

            public int Size => _labels.Sum(l => l.Length) + _labels.Length + 1;

            public static DnsDomain FromArray(byte[] message, int offset)
                => FromArray(message, offset, out offset);

            public static DnsDomain FromArray(byte[] message, int offset, out int endOffset)
            {
                var labels = new List<byte[]>();
                var endOffsetAssigned = false;
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

                    if (lengthOrPointer.GetBitValueAt(6, 2) != 0)
                    {
                        throw new ArgumentException("Unexpected bit pattern in label length");
                    }

                    var length = lengthOrPointer;
                    var label = new byte[length];
                    Array.Copy(message, offset, label, 0, length);

                    labels.Add(label);

                    offset += length;
                }

                if (!endOffsetAssigned)
                {
                    endOffset = offset;
                }

                return new DnsDomain(labels.Select(l => l.ToText(Encoding.ASCII)).ToArray());
            }

            public static DnsDomain PointerName(IPAddress ip)
                => new DnsDomain(FormatReverseIP(ip));

            public byte[] ToArray()
            {
                var result = new byte[Size];
                var offset = 0;

                foreach (var l in _labels.Select(label => Encoding.ASCII.GetBytes(label)))
                {
                    result[offset++] = (byte) l.Length;
                    l.CopyTo(result, offset);

                    offset += l.Length;
                }

                result[offset] = 0;

                return result;
            }

            public override string ToString()
                => string.Join(".", _labels);

            public int CompareTo(DnsDomain other)
                => string.Compare(ToString(), other.ToString(), StringComparison.Ordinal);

            public override bool Equals(object obj)
                => obj is DnsDomain && CompareTo((DnsDomain) obj) == 0;

            public override int GetHashCode() => ToString().GetHashCode();

            private static string FormatReverseIP(IPAddress ip)
            {
                var address = ip.GetAddressBytes();

                if (address.Length == 4)
                {
                    return string.Join(".", address.Reverse().Select(b => b.ToString())) + ".in-addr.arpa";
                }

                var nibbles = new byte[address.Length * 2];

                for (int i = 0, j = 0; i < address.Length; i++, j = 2 * i)
                {
                    var b = address[i];

                    nibbles[j] = b.GetBitValueAt(4, 4);
                    nibbles[j + 1] = b.GetBitValueAt(0, 4);
                }

                return string.Join(".", nibbles.Reverse().Select(b => b.ToString("x"))) + ".ip6.arpa";
            }
        }

        public class DnsQuestion : IDnsMessageEntry
        {
            public static IList<DnsQuestion> GetAllFromArray(byte[] message, int offset, int questionCount)
            {
                return GetAllFromArray(message, offset, questionCount, out offset);
            }

            public static IList<DnsQuestion> GetAllFromArray(
                byte[] message, 
                int offset, 
                int questionCount,
                out int endOffset)
            {
                IList<DnsQuestion> questions = new List<DnsQuestion>(questionCount);

                for (var i = 0; i < questionCount; i++)
                {
                    questions.Add(FromArray(message, offset, out offset));
                }

                endOffset = offset;
                return questions;
            }

            public static DnsQuestion FromArray(byte[] message, int offset, out int endOffset)
            {
                var domain = DnsDomain.FromArray(message, offset, out offset);
                var tail = message.ToStruct<Tail>(offset, Tail.SIZE);

                endOffset = offset + Tail.SIZE;

                return new DnsQuestion(domain, tail.Type, tail.Class);
            }

            private readonly DnsDomain _domain;
            private readonly DnsRecordType _type;
            private readonly DnsRecordClass _klass;

            public DnsQuestion(
                DnsDomain domain, 
                DnsRecordType type = DnsRecordType.A,
                DnsRecordClass klass = DnsRecordClass.IN)
            {
                _domain = domain;
                _type = type;
                _klass = klass;
            }

            public DnsDomain Name => _domain;

            public DnsRecordType Type => _type;

            public DnsRecordClass Class => _klass;

            public int Size => _domain.Size + Tail.SIZE;

            public byte[] ToArray()
            {
                return new MemoryStream(Size)
                    .Append(_domain.ToArray())
                    .Append(new Tail {Type = Type, Class = Class}.ToBytes())
                    .ToArray();
            }

            public override string ToString()
                => Json.SerializeOnly(this, true, nameof(Name), nameof(Type), nameof(Class));

            [StructEndianness(Endianness.Big)]
            [StructLayout(LayoutKind.Sequential, Pack = 2)]
            private struct Tail
            {
                public const int SIZE = 4;

                private ushort type;
                private ushort klass;

                public DnsRecordType Type
                {
                    get => (DnsRecordType) type;
                    set => type = (ushort) value;
                }

                public DnsRecordClass Class
                {
                    get => (DnsRecordClass) klass;
                    set => klass = (ushort) value;
                }
            }
        }
    }
}