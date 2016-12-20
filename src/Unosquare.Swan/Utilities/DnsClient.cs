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

    public enum DnsRecordType
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

    public enum DnsRecordClass
    {
        IN = 1,
        ANY = 255,
    }

    public enum DnsOperationCode
    {
        Query = 0,
        IQuery,
        Status,
        // Reserved = 3
        Notify = 4,
        Update,
    }

    public enum DnsResponseCode
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

    public abstract class DnsBaseResourceRecord : IDnsResourceRecord
    {
        private IDnsResourceRecord record;

        public DnsBaseResourceRecord(IDnsResourceRecord record)
        {
            this.record = record;
        }

        public DnsDomain Name
        {
            get { return record.Name; }
        }

        public DnsRecordType Type
        {
            get { return record.Type; }
        }

        public DnsRecordClass Class
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

    public class DnsResourceRecord : IDnsResourceRecord
    {
        private DnsDomain domain;
        private DnsRecordType type;
        private DnsRecordClass klass;
        private TimeSpan ttl;
        private byte[] data;

        public static IList<DnsResourceRecord> GetAllFromArray(byte[] message, int offset, int count)
        {
            return GetAllFromArray(message, offset, count, out offset);
        }

        public static IList<DnsResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset)
        {
            IList<DnsResourceRecord> records = new List<DnsResourceRecord>(count);

            for (int i = 0; i < count; i++)
            {
                records.Add(FromArray(message, offset, out offset));
            }

            endOffset = offset;
            return records;
        }

        public static DnsResourceRecord FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out offset);
        }

        public static DnsResourceRecord FromArray(byte[] message, int offset, out int endOffset)
        {
            var domain = DnsDomain.FromArray(message, offset, out offset);
            var tail = message.ToStruct<Tail>(offset, Tail.SIZE);

            byte[] data = new byte[tail.DataLength];

            offset += Tail.SIZE;
            Array.Copy(message, offset, data, 0, data.Length);

            endOffset = offset + data.Length;

            return new DnsResourceRecord(domain, data, tail.Type, tail.Class, tail.TimeToLive);
        }

        public static DnsResourceRecord FromQuestion(DnsQuestion question, byte[] data, TimeSpan ttl = default(TimeSpan))
        {
            return new DnsResourceRecord(question.Name, data, question.Type, question.Class, ttl);
        }

        public DnsResourceRecord(DnsDomain domain, byte[] data, DnsRecordType type,
                DnsRecordClass klass = DnsRecordClass.IN, TimeSpan ttl = default(TimeSpan))
        {
            this.domain = domain;
            this.type = type;
            this.klass = klass;
            this.ttl = ttl;
            this.data = data;
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

    public class DnsPointerResourceRecord : DnsBaseResourceRecord
    {
        public DnsPointerResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            PointerDomainName = DnsDomain.FromArray(message, dataOffset);
        }

        public DnsPointerResourceRecord(DnsDomain domain, DnsDomain pointer, TimeSpan ttl = default(TimeSpan)) :
            base(new DnsResourceRecord(domain, pointer.ToArray(), DnsRecordType.PTR, DnsRecordClass.IN, ttl))
        {
            PointerDomainName = pointer;
        }

        public DnsDomain PointerDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("PointerDomainName").ToString();
        }
    }

    public class DnsIPAddressResourceRecord : DnsBaseResourceRecord
    {
        private static IDnsResourceRecord Create(DnsDomain domain, IPAddress ip, TimeSpan ttl)
        {
            byte[] data = ip.GetAddressBytes();
            DnsRecordType type = data.Length == 4 ? DnsRecordType.A : DnsRecordType.AAAA;

            return new DnsResourceRecord(domain, data, type, DnsRecordClass.IN, ttl);
        }

        public DnsIPAddressResourceRecord(IDnsResourceRecord record)
            : base(record)
        {
            IPAddress = new IPAddress(Data);
        }

        public DnsIPAddressResourceRecord(DnsDomain domain, IPAddress ip, TimeSpan ttl = default(TimeSpan)) :
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

    public class DnsNameServerResourceRecord : DnsBaseResourceRecord
    {
        public DnsNameServerResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            NSDomainName = DnsDomain.FromArray(message, dataOffset);
        }

        public DnsNameServerResourceRecord(DnsDomain domain, DnsDomain nsDomain, TimeSpan ttl = default(TimeSpan)) :
            base(new DnsResourceRecord(domain, nsDomain.ToArray(), DnsRecordType.NS, DnsRecordClass.IN, ttl))
        {
            NSDomainName = nsDomain;
        }

        public DnsDomain NSDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("NSDomainName").ToString();
        }
    }

    public class DnsCanonicalNameResourceRecord : DnsBaseResourceRecord
    {
        public DnsCanonicalNameResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            CanonicalDomainName = DnsDomain.FromArray(message, dataOffset);
        }

        public DnsCanonicalNameResourceRecord(DnsDomain domain, DnsDomain cname, TimeSpan ttl = default(TimeSpan)) :
            base(new DnsResourceRecord(domain, cname.ToArray(), DnsRecordType.CNAME, DnsRecordClass.IN, ttl))
        {
            CanonicalDomainName = cname;
        }

        public DnsDomain CanonicalDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("CanonicalDomainName").ToString();
        }
    }

    public class DnsMailExchangeResourceRecord : DnsBaseResourceRecord
    {
        private const int PREFERENCE_SIZE = 2;

        private static IDnsResourceRecord Create(DnsDomain domain, int preference, DnsDomain exchange, TimeSpan ttl)
        {
            byte[] pref = BitConverter.GetBytes((ushort)preference);
            byte[] data = new byte[pref.Length + exchange.Size];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pref);
            }

            pref.CopyTo(data, 0);
            exchange.ToArray().CopyTo(data, pref.Length);

            return new DnsResourceRecord(domain, data, DnsRecordType.MX, DnsRecordClass.IN, ttl);
        }

        public DnsMailExchangeResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            byte[] preference = new byte[DnsMailExchangeResourceRecord.PREFERENCE_SIZE];
            Array.Copy(message, dataOffset, preference, 0, preference.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(preference);
            }

            dataOffset += DnsMailExchangeResourceRecord.PREFERENCE_SIZE;

            Preference = BitConverter.ToUInt16(preference, 0);
            ExchangeDomainName = DnsDomain.FromArray(message, dataOffset);
        }

        public DnsMailExchangeResourceRecord(DnsDomain domain, int preference, DnsDomain exchange, TimeSpan ttl = default(TimeSpan)) :
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

        public DnsDomain ExchangeDomainName
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Stringify().Add("Preference", "ExchangeDomainName").ToString();
        }
    }

    public class DnsStartOfAuthorityResourceRecord : DnsBaseResourceRecord
    {
        private static IDnsResourceRecord Create(DnsDomain domain, DnsDomain master, DnsDomain responsible, long serial,
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

            return new DnsResourceRecord(domain, data.ToArray(), DnsRecordType.SOA, DnsRecordClass.IN, ttl);
        }

        public DnsStartOfAuthorityResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            MasterDomainName = DnsDomain.FromArray(message, dataOffset, out dataOffset);
            ResponsibleDomainName = DnsDomain.FromArray(message, dataOffset, out dataOffset);

            Options tail = message.ToStruct<Options>(dataOffset, Options.SIZE);

            SerialNumber = tail.SerialNumber;
            RefreshInterval = tail.RefreshInterval;
            RetryInterval = tail.RetryInterval;
            ExpireInterval = tail.ExpireInterval;
            MinimumTimeToLive = tail.MinimumTimeToLive;
        }

        public DnsStartOfAuthorityResourceRecord(DnsDomain domain, DnsDomain master, DnsDomain responsible, long serial,
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

        public DnsStartOfAuthorityResourceRecord(DnsDomain domain, DnsDomain master, DnsDomain responsible,
                Options options = default(Options), TimeSpan ttl = default(TimeSpan)) :
            this(domain, master, responsible, options.SerialNumber, options.RefreshInterval, options.RetryInterval,
                    options.ExpireInterval, options.MinimumTimeToLive, ttl)
        { }

        public DnsDomain MasterDomainName
        {
            get;
            private set;
        }

        public DnsDomain ResponsibleDomainName
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

    public static class DnsResourceRecordFactory
    {
        public static IList<IDnsResourceRecord> GetAllFromArray(byte[] message, int offset, int count)
        {
            return GetAllFromArray(message, offset, count, out offset);
        }

        public static IList<IDnsResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset)
        {
            IList<IDnsResourceRecord> result = new List<IDnsResourceRecord>(count);

            for (int i = 0; i < count; i++)
            {
                result.Add(FromArray(message, offset, out offset));
            }

            endOffset = offset;
            return result;
        }

        public static IDnsResourceRecord FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out offset);
        }

        public static IDnsResourceRecord FromArray(byte[] message, int offset, out int endOffest)
        {
            DnsResourceRecord record = DnsResourceRecord.FromArray(message, offset, out endOffest);
            int dataOffset = endOffest - record.DataLength;

            switch (record.Type)
            {
                case DnsRecordType.A:
                case DnsRecordType.AAAA:
                    return new DnsIPAddressResourceRecord(record);
                case DnsRecordType.NS:
                    return new DnsNameServerResourceRecord(record, message, dataOffset);
                case DnsRecordType.CNAME:
                    return new DnsCanonicalNameResourceRecord(record, message, dataOffset);
                case DnsRecordType.SOA:
                    return new DnsStartOfAuthorityResourceRecord(record, message, dataOffset);
                case DnsRecordType.PTR:
                    return new DnsPointerResourceRecord(record, message, dataOffset);
                case DnsRecordType.MX:
                    return new DnsMailExchangeResourceRecord(record, message, dataOffset);
                default:
                    return record;
            }
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

    public class DnsClient
    {
        private const int DEFAULT_PORT = 53;
        private static readonly Random RANDOM = new Random();

        private IPEndPoint dns;
        private IDnsRequestResolver resolver;

        public DnsClient(IPEndPoint dns, IDnsRequestResolver resolver = null)
        {
            this.dns = dns;
            this.resolver = resolver == null ? new DnsUdpRequestResolver(new DnsTcpRequestResolver()) : resolver;
        }

        public DnsClient(IPAddress ip, int port = DEFAULT_PORT, IDnsRequestResolver resolver = null) :
            this(new IPEndPoint(ip, port), resolver)
        { }

        public DnsClient(string ip, int port = DEFAULT_PORT, IDnsRequestResolver resolver = null) :
            this(IPAddress.Parse(ip), port, resolver)
        { }

        public DnsClientRequest FromArray(byte[] message)
        {
            DnsRequest request = DnsRequest.FromArray(message);
            return new DnsClientRequest(dns, request, resolver);
        }

        public DnsClientRequest Create(IDnsRequest request = null)
        {
            return new DnsClientRequest(dns, request, resolver);
        }

        public IList<IPAddress> Lookup(string domain, DnsRecordType type = DnsRecordType.A)
        {
            if (type != DnsRecordType.A && type != DnsRecordType.AAAA)
            {
                throw new ArgumentException("Invalid record type " + type);
            }

            DnsClientResponse response = Resolve(domain, type);
            IList<IPAddress> ips = response.AnswerRecords
                .Where(r => r.Type == type)
                .Cast<DnsIPAddressResourceRecord>()
                .Select(r => r.IPAddress)
                .ToList();

            if (ips.Count == 0)
            {
                throw new DnsResponseException(response, "No matching records");
            }

            return ips;
        }

        public string Reverse(string ip)
        {
            return Reverse(IPAddress.Parse(ip));
        }

        public string Reverse(IPAddress ip)
        {
            DnsClientResponse response = Resolve(DnsDomain.PointerName(ip), DnsRecordType.PTR);
            IDnsResourceRecord ptr = response.AnswerRecords.FirstOrDefault(r => r.Type == DnsRecordType.PTR);

            if (ptr == null)
            {
                throw new DnsResponseException(response, "No matching records");
            }

            return ((DnsPointerResourceRecord)ptr).PointerDomainName.ToString();
        }

        public DnsClientResponse Resolve(string domain, DnsRecordType type)
        {
            return Resolve(new DnsDomain(domain), type);
        }

        public DnsClientResponse Resolve(DnsDomain domain, DnsRecordType type)
        {
            DnsClientRequest request = Create();
            DnsQuestion question = new DnsQuestion(domain, type);

            request.Questions.Add(question);
            request.OperationCode = DnsOperationCode.Query;
            request.RecursionDesired = true;

            return request.Resolve();
        }
    }

    public class DnsClientRequest : IDnsRequest
    {
        private const int DEFAULT_PORT = 53;

        private IPEndPoint dns;
        private IDnsRequestResolver resolver;
        private IDnsRequest request;

        public DnsClientRequest(IPEndPoint dns, IDnsRequest request = null, IDnsRequestResolver resolver = null)
        {
            this.dns = dns;
            this.request = request == null ? new DnsRequest() : new DnsRequest(request);
            this.resolver = resolver == null ? new DnsUdpRequestResolver() : resolver;
        }

        public DnsClientRequest(IPAddress ip, int port = DEFAULT_PORT, IDnsRequest request = null, IDnsRequestResolver resolver = null) :
            this(new IPEndPoint(ip, port), request, resolver)
        { }

        public DnsClientRequest(string ip, int port = DEFAULT_PORT, IDnsRequest request = null, IDnsRequestResolver resolver = null) :
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

    public class DnsClientResponse : IDnsResponse
    {
        private DnsResponse response;
        private byte[] message;

        public static DnsClientResponse FromArray(DnsClientRequest request, byte[] message)
        {
            DnsResponse response = DnsResponse.FromArray(message);
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

    public interface IDnsRequest : IDnsMessage
    {
        int Id { get; set; }
        DnsOperationCode OperationCode { get; set; }
        bool RecursionDesired { get; set; }
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

    public interface IDnsResponse : IDnsMessage
    {
        int Id { get; set; }
        IList<IDnsResourceRecord> AnswerRecords { get; }
        IList<IDnsResourceRecord> AuthorityRecords { get; }
        IList<IDnsResourceRecord> AdditionalRecords { get; }
        bool RecursionAvailable { get; set; }
        bool AuthorativeServer { get; set; }
        bool Truncated { get; set; }
        DnsOperationCode OperationCode { get; set; }
        DnsResponseCode ResponseCode { get; set; }
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
            DnsResponse response = new DnsResponse();

            response.Id = request.Id;

            foreach (DnsQuestion question in request.Questions)
            {
                response.Questions.Add(question);
            }

            return response;
        }

        public static DnsResponse FromArray(byte[] message)
        {
            DnsHeader header = DnsHeader.FromArray(message);
            int offset = header.Size;

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
            RecursionAvailable = response.RecursionAvailable;
            AuthorativeServer = response.AuthorativeServer;
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

    public interface IDnsRequestResolver
    {
        DnsClientResponse Request(DnsClientRequest request);
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

    public class DnsResponseException : Exception
    {
        private static string Format(IDnsResponse response)
        {
            return string.Format("Invalid response received with code {0}", response.ResponseCode);
        }

        public DnsResponseException() { }
        public DnsResponseException(string message) : base(message) { }
        public DnsResponseException(string message, Exception e) : base(message, e) { }

        public DnsResponseException(IDnsResponse response) : this(response, Format(response)) { }

        public DnsResponseException(IDnsResponse response, Exception e)
            : base(Format(response), e)
        {
            Response = response;
        }

        public DnsResponseException(IDnsResponse response, string message)
            : base(message)
        {
            Response = response;
        }

        public IDnsResponse Response
        {
            get;
            private set;
        }
    }

    public class DnsNullRequestResolver : IDnsRequestResolver
    {
        public DnsClientResponse Request(DnsClientRequest request)
        {
            throw new DnsResponseException("Request failed");
        }
    }
}
