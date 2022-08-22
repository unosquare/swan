namespace Swan.Net.Dns;

using Swan.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

/// <summary>
/// DnsClient public methods.
/// </summary>
internal partial class DnsClient
{
    public abstract class DnsResourceRecordBase : IDnsResourceRecord
    {
        private readonly IDnsResourceRecord _record;

        protected DnsResourceRecordBase(IDnsResourceRecord record)
        {
            _record = record;
        }

        public DnsDomain Name => _record.Name;

        public DnsRecordType Type => _record.Type;

        public DnsRecordClass Class => _record.Class;

        public TimeSpan TimeToLive => _record.TimeToLive;

        public int DataLength => _record.DataLength;

        [JsonIgnore]
        public byte[] Data => _record.Data;

        [JsonIgnore]
        public int Size => _record.Size;

        public byte[] ToArray() => _record.ToArray();

        public override string ToString()
            => this.JsonSerialize(true);
    }

    public class DnsResourceRecord : IDnsResourceRecord
    {
        public DnsResourceRecord(
            DnsDomain domain,
            byte[] data,
            DnsRecordType type,
            DnsRecordClass klass = DnsRecordClass.IN,
            TimeSpan ttl = default)
        {
            Name = domain;
            Type = type;
            Class = klass;
            TimeToLive = ttl;
            Data = data;
        }

        public DnsDomain Name { get; }

        public DnsRecordType Type { get; }

        public DnsRecordClass Class { get; }

        public TimeSpan TimeToLive { get; }

        public int DataLength => Data.Length;

        [JsonIgnore]
        public byte[] Data { get; }

        [JsonIgnore]
        public int Size => Name.Size + Tail.SIZE + Data.Length;

        public static DnsResourceRecord FromArray(byte[] message, int offset, out int endOffset)
        {
            var domain = DnsDomain.FromArray(message, offset, out offset);
            var tail = message.ToStruct<Tail>(offset, Tail.SIZE);

            var data = new byte[tail.DataLength];

            offset += Tail.SIZE;
            Array.Copy(message, offset, data, 0, data.Length);

            endOffset = offset + data.Length;

            return new DnsResourceRecord(domain, data, tail.Type, tail.Class, tail.TimeToLive);
        }

        public byte[] ToArray()
        {
            using var stream = new MemoryStream(Size);
            return stream
                .Append(Name.ToArray())
                .Append(new Tail()
                {
                    Type = Type,
                    Class = Class,
                    TimeToLive = TimeToLive,
                    DataLength = Data.Length,
                }.ToBytes())
                .Append(Data)
                .ToArray();
        }

        public override string ToString() =>
            this.JsonSerialize(true);

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
                get => (DnsRecordType)type;
                init => type = (ushort)value;
            }

            public DnsRecordClass Class
            {
                get => (DnsRecordClass)klass;
                init => klass = (ushort)value;
            }

            public TimeSpan TimeToLive
            {
                get => TimeSpan.FromSeconds(ttl);
                init => ttl = (uint)value.TotalSeconds;
            }

            public int DataLength
            {
                get => dataLength;
                init => dataLength = (ushort)value;
            }
        }
    }

    public class DnsPointerResourceRecord : DnsResourceRecordBase
    {
        public DnsPointerResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            PointerDomainName = DnsDomain.FromArray(message, dataOffset);
        }

        public DnsDomain PointerDomainName { get; }
    }

    public class DnsIPAddressResourceRecord : DnsResourceRecordBase
    {
        public DnsIPAddressResourceRecord(IDnsResourceRecord record)
            : base(record)
        {
            IPAddress = new IPAddress(Data);
        }

        public IPAddress IPAddress { get; }
    }

    public class DnsNameServerResourceRecord : DnsResourceRecordBase
    {
        public DnsNameServerResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            NSDomainName = DnsDomain.FromArray(message, dataOffset);
        }

        public DnsDomain NSDomainName { get; }
    }

    public class DnsCanonicalNameResourceRecord : DnsResourceRecordBase
    {
        public DnsCanonicalNameResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            CanonicalDomainName = DnsDomain.FromArray(message, dataOffset);
        }

        public DnsDomain CanonicalDomainName { get; }
    }

    public class DnsMailExchangeResourceRecord : DnsResourceRecordBase
    {
        private const int PreferenceSize = 2;

        public DnsMailExchangeResourceRecord(
            IDnsResourceRecord record,
            byte[] message,
            int dataOffset)
            : base(record)
        {
            var preference = new byte[PreferenceSize];
            Array.Copy(message, dataOffset, preference, 0, preference.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(preference);
            }

            dataOffset += PreferenceSize;

            Preference = BitConverter.ToUInt16(preference, 0);
            ExchangeDomainName = DnsDomain.FromArray(message, dataOffset);
        }

        public int Preference { get; }

        public DnsDomain ExchangeDomainName { get; }
    }

    public class DnsStartOfAuthorityResourceRecord : DnsResourceRecordBase
    {
        public DnsStartOfAuthorityResourceRecord(IDnsResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            MasterDomainName = DnsDomain.FromArray(message, dataOffset, out dataOffset);
            ResponsibleDomainName = DnsDomain.FromArray(message, dataOffset, out dataOffset);

            var tail = message.ToStruct<Options>(dataOffset, Options.SIZE);

            SerialNumber = tail.SerialNumber;
            RefreshInterval = tail.RefreshInterval;
            RetryInterval = tail.RetryInterval;
            ExpireInterval = tail.ExpireInterval;
            MinimumTimeToLive = tail.MinimumTimeToLive;
        }

        public DnsStartOfAuthorityResourceRecord(
            DnsDomain domain,
            DnsDomain master,
            DnsDomain responsible,
            long serial,
            TimeSpan refresh,
            TimeSpan retry,
            TimeSpan expire,
            TimeSpan minTtl,
            TimeSpan ttl = default)
            : base(Create(domain, master, responsible, serial, refresh, retry, expire, minTtl, ttl))
        {
            MasterDomainName = master;
            ResponsibleDomainName = responsible;

            SerialNumber = serial;
            RefreshInterval = refresh;
            RetryInterval = retry;
            ExpireInterval = expire;
            MinimumTimeToLive = minTtl;
        }

        public DnsDomain MasterDomainName { get; }

        public DnsDomain ResponsibleDomainName { get; }

        public long SerialNumber { get; }

        [JsonIgnore]
        public TimeSpan RefreshInterval { get; }

        [JsonIgnore]
        public TimeSpan RetryInterval { get; }

        [JsonIgnore]
        public TimeSpan ExpireInterval { get; }

        [JsonIgnore]
        public TimeSpan MinimumTimeToLive { get; }

        private static IDnsResourceRecord Create(
            DnsDomain domain,
            DnsDomain master,
            DnsDomain responsible,
            long serial,
            TimeSpan refresh,
            TimeSpan retry,
            TimeSpan expire,
            TimeSpan minTtl,
            TimeSpan ttl)
        {
            using var data = new MemoryStream(Options.SIZE + master.Size + responsible.Size);
            var tail = new Options
            {
                SerialNumber = serial,
                RefreshInterval = refresh,
                RetryInterval = retry,
                ExpireInterval = expire,
                MinimumTimeToLive = minTtl,
            };

            data.Append(master.ToArray()).Append(responsible.ToArray()).Append(tail.ToBytes());

            return new DnsResourceRecord(domain, data.ToArray(), DnsRecordType.SOA, DnsRecordClass.IN, ttl);
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
                get => serialNumber;
                set => serialNumber = (uint)value;
            }

            public TimeSpan RefreshInterval
            {
                get => TimeSpan.FromSeconds(refreshInterval);
                set => refreshInterval = (uint)value.TotalSeconds;
            }

            public TimeSpan RetryInterval
            {
                get => TimeSpan.FromSeconds(retryInterval);
                set => retryInterval = (uint)value.TotalSeconds;
            }

            public TimeSpan ExpireInterval
            {
                get => TimeSpan.FromSeconds(expireInterval);
                set => expireInterval = (uint)value.TotalSeconds;
            }

            public TimeSpan MinimumTimeToLive
            {
                get => TimeSpan.FromSeconds(ttl);
                set => ttl = (uint)value.TotalSeconds;
            }
        }
    }

    private static class DnsResourceRecordFactory
    {
        public static IList<IDnsResourceRecord> GetAllFromArray(
            byte[] message,
            int offset,
            int count,
            out int endOffset)
        {
            var result = new List<IDnsResourceRecord>(count);

            for (var i = 0; i < count; i++)
            {
                result.Add(GetFromArray(message, offset, out offset));
            }

            endOffset = offset;
            return result;
        }

        private static IDnsResourceRecord GetFromArray(byte[] message, int offset, out int endOffset)
        {
            var record = DnsResourceRecord.FromArray(message, offset, out endOffset);
            var dataOffset = endOffset - record.DataLength;

            return record.Type switch
            {
                DnsRecordType.A => new DnsIPAddressResourceRecord(record),
                DnsRecordType.AAAA => new DnsIPAddressResourceRecord(record),
                DnsRecordType.NS => new DnsNameServerResourceRecord(record, message, dataOffset),
                DnsRecordType.CNAME => new DnsCanonicalNameResourceRecord(record, message, dataOffset),
                DnsRecordType.SOA => new DnsStartOfAuthorityResourceRecord(record, message, dataOffset),
                DnsRecordType.PTR => new DnsPointerResourceRecord(record, message, dataOffset),
                DnsRecordType.MX => new DnsMailExchangeResourceRecord(record, message, dataOffset),
                _ => record
            };
        }
    }
}
