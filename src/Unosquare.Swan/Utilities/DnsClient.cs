using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Swan.Formatters;

namespace Unosquare.Swan.Utilities
{

    public enum RecordType
    {
        A = 1,
        NS = 2,
        CNAME = 5,
        SOA = 6,
        WKS = 11,
        PTR = 12,
        MX = 15,
        TXT = 16,
        AAAA = 28,
        SRV = 33,
        ANY = 255,
    }

    public enum RecordClass
    {
        IN = 1,
        ANY = 255,
    }

    public enum OperationCode
    {
        Query = 0,
        IQuery,
        Status,
        // Reserved = 3
        Notify = 4,
        Update,
    }

    public enum ResponseCode
    {
        NoError = 0,
        FormatError,
        ServerFailure,
        NameError,
        NotImplemented,
        Refused,
        YXDomain,
        YXRRSet,
        NXRRSet,
        NotAuth,
        NotZone,
    }

    public class Domain : IComparable<Domain>
    {
        private string[] labels;

        public static Domain FromString(string domain)
        {
            return new Domain(domain);
        }

        public static Domain FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out offset);
        }

        public static Domain FromArray(byte[] message, int offset, out int endOffset)
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

            return new Domain(labels.Select(l => Encoding.ASCII.GetString(l)).ToArray());
        }

        public static Domain PointerName(IPAddress ip)
        {
            return new Domain(FormatReverseIP(ip));
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

        public Domain(string domain) : this(domain.Split('.')) { }

        public Domain(string[] labels)
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

        public int CompareTo(Domain other)
        {
            return ToString().CompareTo(other.ToString());
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is Domain))
            {
                return false;
            }

            return CompareTo(obj as Domain) == 0;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public interface IMessage
    {
        IList<Question> Questions { get; }

        int Size { get; }
        byte[] ToArray();
    }

    public interface IMessageEntry
    {
        Domain Name { get; }
        RecordType Type { get; }
        RecordClass Class { get; }

        int Size { get; }
        byte[] ToArray();
    }

    public interface IResourceRecord : IMessageEntry
    {
        TimeSpan TimeToLive { get; }
        int DataLength { get; }
        byte[] Data { get; }
    }

    public abstract class BaseResourceRecord : IResourceRecord
    {
        private IResourceRecord record;

        public BaseResourceRecord(IResourceRecord record)
        {
            this.record = record;
        }

        public Domain Name
        {
            get { return record.Name; }
        }

        public RecordType Type
        {
            get { return record.Type; }
        }

        public RecordClass Class
        {
            get { return record.Class; }
        }

        public TimeSpan TimeToLive
        {
            get { return record.TimeToLive; }
        }

        public int DataLength
        {
            get { return record.DataLength; }
        }

        public byte[] Data
        {
            get { return record.Data; }
        }

        public int Size
        {
            get { return record.Size; }
        }

        public byte[] ToArray()
        {
            return record.ToArray();
        }

        internal ObjectStringifier Stringify()
        {
            return ObjectStringifier.FromObject(this)
                .Add(nameof(Name), nameof(Type), nameof(Class), nameof(TimeToLive), nameof(DataLength));
        }
    }

    public class ResourceRecord : IResourceRecord
    {
        private Domain domain;
        private RecordType type;
        private RecordClass klass;
        private TimeSpan ttl;
        private byte[] data;

        public static IList<ResourceRecord> GetAllFromArray(byte[] message, int offset, int count)
        {
            return GetAllFromArray(message, offset, count, out offset);
        }

        public static IList<ResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset)
        {
            IList<ResourceRecord> records = new List<ResourceRecord>(count);

            for (int i = 0; i < count; i++)
            {
                records.Add(FromArray(message, offset, out offset));
            }

            endOffset = offset;
            return records;
        }

        public static ResourceRecord FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out offset);
        }

        public static ResourceRecord FromArray(byte[] message, int offset, out int endOffset)
        {
            var domain = Domain.FromArray(message, offset, out offset);
            var tail = message.ToStruct<Tail>(offset, Tail.SIZE);

            byte[] data = new byte[tail.DataLength];

            offset += Tail.SIZE;
            Array.Copy(message, offset, data, 0, data.Length);

            endOffset = offset + data.Length;

            return new ResourceRecord(domain, data, tail.Type, tail.Class, tail.TimeToLive);
        }

        public static ResourceRecord FromQuestion(Question question, byte[] data, TimeSpan ttl = default(TimeSpan))
        {
            return new ResourceRecord(question.Name, data, question.Type, question.Class, ttl);
        }

        public ResourceRecord(Domain domain, byte[] data, RecordType type,
                RecordClass klass = RecordClass.IN, TimeSpan ttl = default(TimeSpan))
        {
            this.domain = domain;
            this.type = type;
            this.klass = klass;
            this.ttl = ttl;
            this.data = data;
        }

        public Domain Name
        {
            get { return domain; }
        }

        public RecordType Type
        {
            get { return type; }
        }

        public RecordClass Class
        {
            get { return klass; }
        }

        public TimeSpan TimeToLive
        {
            get { return ttl; }
        }

        public int DataLength
        {
            get { return data.Length; }
        }

        public byte[] Data
        {
            get { return data; }
        }

        public int Size
        {
            get { return domain.Size + Tail.SIZE + data.Length; }
        }

        public byte[] ToArray()
        {
            var result = new MemoryStream(Size);

            result
                .Append(domain.ToArray())
                .Append((new Tail()
                {
                    Type = Type,
                    Class = Class,
                    TimeToLive = ttl,
                    DataLength = data.Length
                }).ToBytes())
                .Append(data);

            return result.ToArray();
        }

        public override string ToString()
        {
            return ObjectStringifier.FromObject(this)
                .Add(nameof(Name), nameof(Type), nameof(Class), nameof(TimeToLive), nameof(DataLength))
                .ToString();
        }

        [StructEndianness(Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct Tail
        {
            public const int SIZE = 10;

            private ushort type;
            private ushort klass;
            private uint ttl;
            private ushort dataLength;

            public RecordType Type
            {
                get { return (RecordType)type; }
                set { type = (ushort)value; }
            }

            public RecordClass Class
            {
                get { return (RecordClass)klass; }
                set { klass = (ushort)value; }
            }

            public TimeSpan TimeToLive
            {
                get { return TimeSpan.FromSeconds(ttl); }
                set { ttl = (uint)value.TotalSeconds; }
            }

            public int DataLength
            {
                get { return dataLength; }
                set { dataLength = (ushort)value; }
            }
        }
    }

    public class PointerResourceRecord : BaseResourceRecord
    {
        public PointerResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            PointerDomainName = Domain.FromArray(message, dataOffset);
        }

        public PointerResourceRecord(Domain domain, Domain pointer, TimeSpan ttl = default(TimeSpan)) :
            base(new ResourceRecord(domain, pointer.ToArray(), RecordType.PTR, RecordClass.IN, ttl))
        {
            PointerDomainName = pointer;
        }

        public Domain PointerDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("PointerDomainName").ToString();
        }
    }

    public class IPAddressResourceRecord : BaseResourceRecord
    {
        private static IResourceRecord Create(Domain domain, IPAddress ip, TimeSpan ttl)
        {
            byte[] data = ip.GetAddressBytes();
            RecordType type = data.Length == 4 ? RecordType.A : RecordType.AAAA;

            return new ResourceRecord(domain, data, type, RecordClass.IN, ttl);
        }

        public IPAddressResourceRecord(IResourceRecord record)
            : base(record)
        {
            IPAddress = new IPAddress(Data);
        }

        public IPAddressResourceRecord(Domain domain, IPAddress ip, TimeSpan ttl = default(TimeSpan)) :
            base(Create(domain, ip, ttl))
        {
            IPAddress = ip;
        }

        public IPAddress IPAddress
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("IPAddress").ToString();
        }
    }

    public class NameServerResourceRecord : BaseResourceRecord
    {
        public NameServerResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            NSDomainName = Domain.FromArray(message, dataOffset);
        }

        public NameServerResourceRecord(Domain domain, Domain nsDomain, TimeSpan ttl = default(TimeSpan)) :
            base(new ResourceRecord(domain, nsDomain.ToArray(), RecordType.NS, RecordClass.IN, ttl))
        {
            NSDomainName = nsDomain;
        }

        public Domain NSDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("NSDomainName").ToString();
        }
    }

    public class CanonicalNameResourceRecord : BaseResourceRecord
    {
        public CanonicalNameResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            CanonicalDomainName = Domain.FromArray(message, dataOffset);
        }

        public CanonicalNameResourceRecord(Domain domain, Domain cname, TimeSpan ttl = default(TimeSpan)) :
            base(new ResourceRecord(domain, cname.ToArray(), RecordType.CNAME, RecordClass.IN, ttl))
        {
            CanonicalDomainName = cname;
        }

        public Domain CanonicalDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("CanonicalDomainName").ToString();
        }
    }

    public class MailExchangeResourceRecord : BaseResourceRecord
    {
        private const int PREFERENCE_SIZE = 2;

        private static IResourceRecord Create(Domain domain, int preference, Domain exchange, TimeSpan ttl)
        {
            byte[] pref = BitConverter.GetBytes((ushort)preference);
            byte[] data = new byte[pref.Length + exchange.Size];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pref);
            }

            pref.CopyTo(data, 0);
            exchange.ToArray().CopyTo(data, pref.Length);

            return new ResourceRecord(domain, data, RecordType.MX, RecordClass.IN, ttl);
        }

        public MailExchangeResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            byte[] preference = new byte[MailExchangeResourceRecord.PREFERENCE_SIZE];
            Array.Copy(message, dataOffset, preference, 0, preference.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(preference);
            }

            dataOffset += MailExchangeResourceRecord.PREFERENCE_SIZE;

            Preference = BitConverter.ToUInt16(preference, 0);
            ExchangeDomainName = Domain.FromArray(message, dataOffset);
        }

        public MailExchangeResourceRecord(Domain domain, int preference, Domain exchange, TimeSpan ttl = default(TimeSpan)) :
            base(Create(domain, preference, exchange, ttl))
        {
            Preference = preference;
            ExchangeDomainName = exchange;
        }

        public int Preference
        {
            get;
            private set;
        }

        public Domain ExchangeDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("Preference", "ExchangeDomainName").ToString();
        }
    }

    public class StartOfAuthorityResourceRecord : BaseResourceRecord
    {
        private static IResourceRecord Create(Domain domain, Domain master, Domain responsible, long serial,
                TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl)
        {
            var data = new MemoryStream(Options.SIZE + master.Size + responsible.Size);
            Options tail = new Options()
            {
                SerialNumber = serial,
                RefreshInterval = refresh,
                RetryInterval = retry,
                ExpireInterval = expire,
                MinimumTimeToLive = minTtl
            };

            data
                .Append(master.ToArray())
                .Append(responsible.ToArray())
                .Append(tail.ToBytes());

            return new ResourceRecord(domain, data.ToArray(), RecordType.SOA, RecordClass.IN, ttl);
        }

        public StartOfAuthorityResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            MasterDomainName = Domain.FromArray(message, dataOffset, out dataOffset);
            ResponsibleDomainName = Domain.FromArray(message, dataOffset, out dataOffset);

            Options tail = message.ToStruct<Options>(dataOffset, Options.SIZE);

            SerialNumber = tail.SerialNumber;
            RefreshInterval = tail.RefreshInterval;
            RetryInterval = tail.RetryInterval;
            ExpireInterval = tail.ExpireInterval;
            MinimumTimeToLive = tail.MinimumTimeToLive;
        }

        public StartOfAuthorityResourceRecord(Domain domain, Domain master, Domain responsible, long serial,
                TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl = default(TimeSpan)) :
            base(Create(domain, master, responsible, serial, refresh, retry, expire, minTtl, ttl))
        {
            MasterDomainName = master;
            ResponsibleDomainName = responsible;

            SerialNumber = serial;
            RefreshInterval = refresh;
            RetryInterval = retry;
            ExpireInterval = expire;
            MinimumTimeToLive = minTtl;
        }

        public StartOfAuthorityResourceRecord(Domain domain, Domain master, Domain responsible,
                Options options = default(Options), TimeSpan ttl = default(TimeSpan)) :
            this(domain, master, responsible, options.SerialNumber, options.RefreshInterval, options.RetryInterval,
                    options.ExpireInterval, options.MinimumTimeToLive, ttl)
        { }

        public Domain MasterDomainName
        {
            get;
            private set;
        }

        public Domain ResponsibleDomainName
        {
            get;
            private set;
        }

        public long SerialNumber
        {
            get;
            private set;
        }

        public TimeSpan RefreshInterval
        {
            get;
            private set;
        }

        public TimeSpan RetryInterval
        {
            get;
            private set;
        }

        public TimeSpan ExpireInterval
        {
            get;
            private set;
        }

        public TimeSpan MinimumTimeToLive
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("MasterDomainName", "ResponsibleDomainName", "SerialNumber").ToString();
        }

        [StructEndianness(Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Options
        {
            public const int SIZE = 20;

            private uint serialNumber;
            private uint refreshInterval;
            private uint retryInterval;
            private uint expireInterval;
            private uint ttl;

            public long SerialNumber
            {
                get { return serialNumber; }
                set { serialNumber = (uint)value; }
            }

            public TimeSpan RefreshInterval
            {
                get { return TimeSpan.FromSeconds(refreshInterval); }
                set { refreshInterval = (uint)value.TotalSeconds; }
            }

            public TimeSpan RetryInterval
            {
                get { return TimeSpan.FromSeconds(retryInterval); }
                set { retryInterval = (uint)value.TotalSeconds; }
            }

            public TimeSpan ExpireInterval
            {
                get { return TimeSpan.FromSeconds(expireInterval); }
                set { expireInterval = (uint)value.TotalSeconds; }
            }

            public TimeSpan MinimumTimeToLive
            {
                get { return TimeSpan.FromSeconds(ttl); }
                set { ttl = (uint)value.TotalSeconds; }
            }
        }
    }

    public static class ResourceRecordFactory
    {
        public static IList<IResourceRecord> GetAllFromArray(byte[] message, int offset, int count)
        {
            return GetAllFromArray(message, offset, count, out offset);
        }

        public static IList<IResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset)
        {
            IList<IResourceRecord> result = new List<IResourceRecord>(count);

            for (int i = 0; i < count; i++)
            {
                result.Add(FromArray(message, offset, out offset));
            }

            endOffset = offset;
            return result;
        }

        public static IResourceRecord FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out offset);
        }

        public static IResourceRecord FromArray(byte[] message, int offset, out int endOffest)
        {
            ResourceRecord record = ResourceRecord.FromArray(message, offset, out endOffest);
            int dataOffset = endOffest - record.DataLength;

            switch (record.Type)
            {
                case RecordType.A:
                case RecordType.AAAA:
                    return new IPAddressResourceRecord(record);
                case RecordType.NS:
                    return new NameServerResourceRecord(record, message, dataOffset);
                case RecordType.CNAME:
                    return new CanonicalNameResourceRecord(record, message, dataOffset);
                case RecordType.SOA:
                    return new StartOfAuthorityResourceRecord(record, message, dataOffset);
                case RecordType.PTR:
                    return new PointerResourceRecord(record, message, dataOffset);
                case RecordType.MX:
                    return new MailExchangeResourceRecord(record, message, dataOffset);
                default:
                    return record;
            }
        }
    }

    public class Question : IMessageEntry
    {
        public static IList<Question> GetAllFromArray(byte[] message, int offset, int questionCount)
        {
            return GetAllFromArray(message, offset, questionCount, out offset);
        }

        public static IList<Question> GetAllFromArray(byte[] message, int offset, int questionCount, out int endOffset)
        {
            IList<Question> questions = new List<Question>(questionCount);

            for (int i = 0; i < questionCount; i++)
            {
                questions.Add(FromArray(message, offset, out offset));
            }

            endOffset = offset;
            return questions;
        }

        public static Question FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out offset);
        }

        public static Question FromArray(byte[] message, int offset, out int endOffset)
        {
            var domain = Domain.FromArray(message, offset, out offset);
            var tail = message.ToStruct<Tail>(offset, Tail.SIZE);

            endOffset = offset + Tail.SIZE;

            return new Question(domain, tail.Type, tail.Class);
        }

        private Domain domain;
        private RecordType type;
        private RecordClass klass;

        public Question(Domain domain, RecordType type = RecordType.A, RecordClass klass = RecordClass.IN)
        {
            this.domain = domain;
            this.type = type;
            this.klass = klass;
        }

        public Domain Name
        {
            get { return domain; }
        }

        public RecordType Type
        {
            get { return type; }
        }

        public RecordClass Class
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

            public RecordType Type
            {
                get { return (RecordType)type; }
                set { type = (ushort)value; }
            }

            public RecordClass Class
            {
                get { return (RecordClass)klass; }
                set { klass = (ushort)value; }
            }
        }
    }

    public class DnsClient
    {
        private const int DEFAULT_PORT = 53;
        private static readonly Random RANDOM = new Random();

        private IPEndPoint dns;
        private IRequestResolver resolver;

        public DnsClient(IPEndPoint dns, IRequestResolver resolver = null)
        {
            this.dns = dns;
            this.resolver = resolver == null ? new UdpRequestResolver(new TcpRequestResolver()) : resolver;
        }

        public DnsClient(IPAddress ip, int port = DEFAULT_PORT, IRequestResolver resolver = null) :
            this(new IPEndPoint(ip, port), resolver)
        { }

        public DnsClient(string ip, int port = DEFAULT_PORT, IRequestResolver resolver = null) :
            this(IPAddress.Parse(ip), port, resolver)
        { }

        public ClientRequest FromArray(byte[] message)
        {
            Request request = Request.FromArray(message);
            return new ClientRequest(dns, request, resolver);
        }

        public ClientRequest Create(IRequest request = null)
        {
            return new ClientRequest(dns, request, resolver);
        }

        public IList<IPAddress> Lookup(string domain, RecordType type = RecordType.A)
        {
            if (type != RecordType.A && type != RecordType.AAAA)
            {
                throw new ArgumentException("Invalid record type " + type);
            }

            ClientResponse response = Resolve(domain, type);
            IList<IPAddress> ips = response.AnswerRecords
                .Where(r => r.Type == type)
                .Cast<IPAddressResourceRecord>()
                .Select(r => r.IPAddress)
                .ToList();

            if (ips.Count == 0)
            {
                throw new ResponseException(response, "No matching records");
            }

            return ips;
        }

        public string Reverse(string ip)
        {
            return Reverse(IPAddress.Parse(ip));
        }

        public string Reverse(IPAddress ip)
        {
            ClientResponse response = Resolve(Domain.PointerName(ip), RecordType.PTR);
            IResourceRecord ptr = response.AnswerRecords.FirstOrDefault(r => r.Type == RecordType.PTR);

            if (ptr == null)
            {
                throw new ResponseException(response, "No matching records");
            }

            return ((PointerResourceRecord)ptr).PointerDomainName.ToString();
        }

        public ClientResponse Resolve(string domain, RecordType type)
        {
            return Resolve(new Domain(domain), type);
        }

        public ClientResponse Resolve(Domain domain, RecordType type)
        {
            ClientRequest request = Create();
            Question question = new Question(domain, type);

            request.Questions.Add(question);
            request.OperationCode = OperationCode.Query;
            request.RecursionDesired = true;

            return request.Resolve();
        }
    }

    public class ClientRequest : IRequest
    {
        private const int DEFAULT_PORT = 53;

        private IPEndPoint dns;
        private IRequestResolver resolver;
        private IRequest request;

        public ClientRequest(IPEndPoint dns, IRequest request = null, IRequestResolver resolver = null)
        {
            this.dns = dns;
            this.request = request == null ? new Request() : new Request(request);
            this.resolver = resolver == null ? new UdpRequestResolver() : resolver;
        }

        public ClientRequest(IPAddress ip, int port = DEFAULT_PORT, IRequest request = null, IRequestResolver resolver = null) :
            this(new IPEndPoint(ip, port), request, resolver)
        { }

        public ClientRequest(string ip, int port = DEFAULT_PORT, IRequest request = null, IRequestResolver resolver = null) :
            this(IPAddress.Parse(ip), port, request, resolver)
        { }

        public int Id
        {
            get { return request.Id; }
            set { request.Id = value; }
        }

        public OperationCode OperationCode
        {
            get { return request.OperationCode; }
            set { request.OperationCode = value; }
        }

        public bool RecursionDesired
        {
            get { return request.RecursionDesired; }
            set { request.RecursionDesired = value; }
        }

        public IList<Question> Questions
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
        /// <exception cref="ResponseException">Throw if a malformed response is received from the server</exception>
        /// <exception cref="IOException">Thrown if a IO error occurs</exception>
        /// <exception cref="SocketException">Thrown if a the reading or writing to the socket fails</exception>
        /// <returns>The response received from server</returns>
        public ClientResponse Resolve()
        {
            try
            {
                ClientResponse response = resolver.Request(this);

                if (response.Id != this.Id)
                {
                    throw new ResponseException(response, "Mismatching request/response IDs");
                }
                if (response.ResponseCode != ResponseCode.NoError)
                {
                    throw new ResponseException(response);
                }

                return response;
            }
            catch (ArgumentException e)
            {
                throw new ResponseException("Invalid response", e);
            }
        }
    }

    public class ClientResponse : IResponse
    {
        private Response response;
        private byte[] message;

        public static ClientResponse FromArray(ClientRequest request, byte[] message)
        {
            Response response = Response.FromArray(message);
            return new ClientResponse(request, response, message);
        }

        internal ClientResponse(ClientRequest request, Response response, byte[] message)
        {
            Request = request;

            this.message = message;
            this.response = response;
        }

        internal ClientResponse(ClientRequest request, Response response)
        {
            Request = request;

            this.message = response.ToArray();
            this.response = response;
        }

        public ClientRequest Request
        {
            get;
            private set;
        }

        public int Id
        {
            get { return response.Id; }
            set { }
        }

        public IList<IResourceRecord> AnswerRecords
        {
            get { return response.AnswerRecords; }
        }

        public IList<IResourceRecord> AuthorityRecords
        {
            get { return new ReadOnlyCollection<IResourceRecord>(response.AuthorityRecords); }
        }

        public IList<IResourceRecord> AdditionalRecords
        {
            get { return new ReadOnlyCollection<IResourceRecord>(response.AdditionalRecords); }
        }

        public bool RecursionAvailable
        {
            get { return response.RecursionAvailable; }
            set { }
        }

        public bool AuthorativeServer
        {
            get { return response.AuthorativeServer; }
            set { }
        }

        public bool Truncated
        {
            get { return response.Truncated; }
            set { }
        }

        public OperationCode OperationCode
        {
            get { return response.OperationCode; }
            set { }
        }

        public ResponseCode ResponseCode
        {
            get { return response.ResponseCode; }
            set { }
        }

        public IList<Question> Questions
        {
            get { return new ReadOnlyCollection<Question>(response.Questions); }
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

    public interface IRequest : IMessage
    {
        int Id { get; set; }
        OperationCode OperationCode { get; set; }
        bool RecursionDesired { get; set; }
    }

    public class Request : IRequest
    {
        private static readonly Random RANDOM = new Random();

        private IList<Question> questions;
        private Header header;

        public static Request FromArray(byte[] message)
        {
            Header header = Header.FromArray(message);

            if (header.Response || header.QuestionCount == 0 ||
                    header.AdditionalRecordCount + header.AnswerRecordCount + header.AuthorityRecordCount > 0 ||
                    header.ResponseCode != ResponseCode.NoError)
            {

                throw new ArgumentException("Invalid request message");
            }

            return new Request(header, Question.GetAllFromArray(message, header.Size, header.QuestionCount));
        }

        public Request(Header header, IList<Question> questions)
        {
            this.header = header;
            this.questions = questions;
        }

        public Request()
        {
            this.questions = new List<Question>();
            this.header = new Header();

            this.header.OperationCode = OperationCode.Query;
            this.header.Response = false;
            this.header.Id = RANDOM.Next(UInt16.MaxValue);
        }

        public Request(IRequest request)
        {
            this.header = new Header();
            this.questions = new List<Question>(request.Questions);

            this.header.Response = false;

            Id = request.Id;
            OperationCode = request.OperationCode;
            RecursionDesired = request.RecursionDesired;
        }

        public IList<Question> Questions
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

        public OperationCode OperationCode
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
                .Add(nameof(Header), header)
                .Add(nameof(Questions))
                .ToString();
        }

        private void UpdateHeader()
        {
            header.QuestionCount = questions.Count;
        }
    }

    // 12 bytes message header
    [StructEndianness(Endianness.Big)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public const int SIZE = 12;

        public static Header FromArray(byte[] header)
        {
            if (header.Length < SIZE)
            {
                throw new ArgumentException("Header length too small");
            }

            return header.ToStruct<Header>(0, SIZE);
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

        public OperationCode OperationCode
        {
            get { return (OperationCode)Opcode; }
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

        public ResponseCode ResponseCode
        {
            get { return (ResponseCode)RCode; }
            set { RCode = (byte)value; }
        }

        public int Size
        {
            get { return Header.SIZE; }
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

    public interface IResponse : IMessage
    {
        int Id { get; set; }
        IList<IResourceRecord> AnswerRecords { get; }
        IList<IResourceRecord> AuthorityRecords { get; }
        IList<IResourceRecord> AdditionalRecords { get; }
        bool RecursionAvailable { get; set; }
        bool AuthorativeServer { get; set; }
        bool Truncated { get; set; }
        OperationCode OperationCode { get; set; }
        ResponseCode ResponseCode { get; set; }
    }

    public class Response : IResponse
    {
        private static readonly Random RANDOM = new Random();

        private Header header;
        private IList<Question> questions;
        private IList<IResourceRecord> answers;
        private IList<IResourceRecord> authority;
        private IList<IResourceRecord> additional;

        public static Response FromRequest(IRequest request)
        {
            Response response = new Response();

            response.Id = request.Id;

            foreach (Question question in request.Questions)
            {
                response.Questions.Add(question);
            }

            return response;
        }

        public static Response FromArray(byte[] message)
        {
            Header header = Header.FromArray(message);
            int offset = header.Size;

            if (!header.Response || header.QuestionCount == 0)
            {
                throw new ArgumentException("Invalid response message");
            }

            if (header.Truncated)
            {
                return new Response(header,
                    Question.GetAllFromArray(message, offset, header.QuestionCount),
                    new List<IResourceRecord>(),
                    new List<IResourceRecord>(),
                    new List<IResourceRecord>());
            }

            return new Response(header,
                Question.GetAllFromArray(message, offset, header.QuestionCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AnswerRecordCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AuthorityRecordCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out offset));
        }

        public Response(Header header, IList<Question> questions, IList<IResourceRecord> answers,
                IList<IResourceRecord> authority, IList<IResourceRecord> additional)
        {
            this.header = header;
            this.questions = questions;
            this.answers = answers;
            this.authority = authority;
            this.additional = additional;
        }

        public Response()
        {
            this.header = new Header();
            this.questions = new List<Question>();
            this.answers = new List<IResourceRecord>();
            this.authority = new List<IResourceRecord>();
            this.additional = new List<IResourceRecord>();

            this.header.Response = true;
            this.header.Id = RANDOM.Next(UInt16.MaxValue);
        }

        public Response(IResponse response)
        {
            this.header = new Header();
            this.questions = new List<Question>(response.Questions);
            this.answers = new List<IResourceRecord>(response.AnswerRecords);
            this.authority = new List<IResourceRecord>(response.AuthorityRecords);
            this.additional = new List<IResourceRecord>(response.AdditionalRecords);

            this.header.Response = true;

            Id = response.Id;
            RecursionAvailable = response.RecursionAvailable;
            AuthorativeServer = response.AuthorativeServer;
            OperationCode = response.OperationCode;
            ResponseCode = response.ResponseCode;
        }

        public IList<Question> Questions
        {
            get { return questions; }
        }

        public IList<IResourceRecord> AnswerRecords
        {
            get { return answers; }
        }

        public IList<IResourceRecord> AuthorityRecords
        {
            get { return authority; }
        }

        public IList<IResourceRecord> AdditionalRecords
        {
            get { return additional; }
        }

        public int Id
        {
            get { return header.Id; }
            set { header.Id = value; }
        }

        public bool RecursionAvailable
        {
            get { return header.RecursionAvailable; }
            set { header.RecursionAvailable = value; }
        }

        public bool AuthorativeServer
        {
            get { return header.AuthorativeServer; }
            set { header.AuthorativeServer = value; }
        }

        public bool Truncated
        {
            get { return header.Truncated; }
            set { header.Truncated = value; }
        }

        public OperationCode OperationCode
        {
            get { return header.OperationCode; }
            set { header.OperationCode = value; }
        }

        public ResponseCode ResponseCode
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
                .Add(nameof(Header), header)
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

    public interface IRequestResolver
    {
        ClientResponse Request(ClientRequest request);
    }

    public class TcpRequestResolver : IRequestResolver
    {
        public ClientResponse Request(ClientRequest request)
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

                Response response = Response.FromArray(buffer);

                return new ClientResponse(request, response, buffer);
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

    public class UdpRequestResolver : IRequestResolver
    {
        private IRequestResolver fallback;

        public UdpRequestResolver(IRequestResolver fallback)
        {
            this.fallback = fallback;
        }

        public UdpRequestResolver()
        {
            this.fallback = new NullRequestResolver();
        }

        public ClientResponse Request(ClientRequest request)
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
                Response response = Response.FromArray(buffer); //null;

                if (response.Truncated)
                {
                    return fallback.Request(request);
                }

                return new ClientResponse(request, response, buffer);
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

    public class ResponseException : Exception
    {
        private static string Format(IResponse response)
        {
            return string.Format("Invalid response received with code {0}", response.ResponseCode);
        }

        public ResponseException() { }
        public ResponseException(string message) : base(message) { }
        public ResponseException(string message, Exception e) : base(message, e) { }

        public ResponseException(IResponse response) : this(response, Format(response)) { }

        public ResponseException(IResponse response, Exception e)
            : base(Format(response), e)
        {
            Response = response;
        }

        public ResponseException(IResponse response, string message)
            : base(message)
        {
            Response = response;
        }

        public IResponse Response
        {
            get;
            private set;
        }
    }

    public class NullRequestResolver : IRequestResolver
    {
        public ClientResponse Request(ClientRequest request)
        {
            throw new ResponseException("Request failed");
        }
    }
}
