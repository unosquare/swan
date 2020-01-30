using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Swan.Net.Internal
{
    // NOTE TO CONTRIBUTORS: If you decide to use this type
    // in any context other than IPAddressRange, please be aware
    // that consistency checks were kept to the bare minimum
    // for use by IPAddressRange.
    // If you add consistency checks, please ensure
    // that IPAddressRange still works as intended.
    // Add regression tests if needed.
    [Serializable]
    internal struct IPAddressValue : IEquatable<IPAddressValue>, IComparable<IPAddressValue>
    {
        public static readonly IPAddressValue MinValue = new IPAddressValue(ulong.MinValue, ulong.MinValue, false);
        public static readonly IPAddressValue MaxValue = new IPAddressValue(ulong.MaxValue, ulong.MaxValue, false);
        public static readonly IPAddressValue MinIPv4Value = new IPAddressValue(0UL, 0xFFFF00000000UL, true);
        public static readonly IPAddressValue MaxIPv4Value = new IPAddressValue(0UL, 0xFFFFFFFFFFFFUL, true);

        private static readonly IReadOnlyList<ulong> LowBitMasks = BuildLowBitMasks();
        private static readonly IReadOnlyList<ulong> HighBitMasks = BuildHighBitMasks();

        private const long V4Mask0 = 0L;
        private const long V4Mask1 = 0xFFFF00000000L;

        private readonly ulong _n0;
        private readonly ulong _n1;
        private readonly bool _isV4;

        public IPAddressValue(IPAddress address)
        {
            // There are no overloads of IPAddress.NetworkToHostOrder for unsigned types;
            // hence the unchecked casts to signed types.
            static ulong ToHostUInt32(byte[] bytes, int startIndex)
                => unchecked((uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, startIndex)));

            static ulong ToHostUInt64(byte[] bytes, int startIndex)
                => unchecked((ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytes, startIndex)));

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var addressBytes = address.GetAddressBytes();
                _n0 = ToHostUInt64(addressBytes, 0);
                _n1 = ToHostUInt64(addressBytes, 8);
                _isV4 = false;
            }
            else
            {
                _n0 = V4Mask0;
                _n1 = V4Mask1 + ToHostUInt32(address.GetAddressBytes(), 0);
                _isV4 = true;
            }
        }

        private IPAddressValue(ulong n0, ulong n1, bool isV4)
        {
            _n0 = n0;
            _n1 = n1;
            _isV4 = isV4;
        }

        // There are no overloads of IPAddress.HostToNetworkOrder for unsigned types;
        // hence the unchecked casts to signed types.
        public IPAddress ToIPAddress(bool forceV6)
            => new IPAddress(_isV4 && !forceV6
                ? BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((int)(uint)_n1)))
                : BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((long) _n0)))
                    .Concat(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((long) _n1))))
                    .ToArray());

        public override int GetHashCode() => CompositeHashCode.Using(_n0, _n1, _isV4);

        public override bool Equals(object obj)
            => obj is IPAddressValue other && Equals(other);

        public bool Equals(IPAddressValue other)
            => other._n0 == _n0
            && other._n1 == _n1;

        public int CompareTo(IPAddressValue other)
        {
            var result = _n0.CompareTo(other._n0);
            return result == 0 ?_n1.CompareTo(other._n1) : result;
        }

        public bool IsStartOfSubnet(byte prefixLength)
        {
            var maxPrefixLength = _isV4 ? 32 : 128;
            if (prefixLength > maxPrefixLength)
                throw SelfCheck.Failure($"Invalid prefix length {prefixLength} in {nameof(IsStartOfSubnet)}");

            if (_isV4)
                prefixLength += 96;

            var bitsToCheck = 128 - prefixLength;
            return bitsToCheck < 64
                ? (_n1 & LowBitMasks[bitsToCheck]) == 0
                : _n1 == 0 && (_n0 & LowBitMasks[bitsToCheck]) == 0;
        }

        public IPAddressValue GetEndOfSubnet(byte prefixLength)
        {
            var maxPrefixLength = _isV4 ? 32 : 128;
            if (prefixLength > maxPrefixLength)
                throw SelfCheck.Failure($"Invalid prefix length {prefixLength} in {nameof(GetEndOfSubnet)}");

            if (_isV4)
                prefixLength += 96;

            var (n0, n1) = prefixLength > 64
                ? (_n0, (_n1 & HighBitMasks[prefixLength - 64]) | LowBitMasks[128 - prefixLength])
                : ((_n0 & HighBitMasks[prefixLength]) | LowBitMasks[64 - prefixLength], 0xFFFFFFFFFFFFFFFFUL);

            return new IPAddressValue(n0, n1, _isV4);
        }

        private static IReadOnlyList<ulong> BuildLowBitMasks()
        {
            var masks = new ulong[65];
            for (var i = 0; i < 64; i++)
                masks[i + 1] = (masks[i] << 1) | 1;

            return masks;
        }

        private static IReadOnlyList<ulong> BuildHighBitMasks()
        {
            var masks = new ulong[65];
            for (var i = 0; i < 64; i++)
                masks[i + 1] = (masks[i] >> 1) | 0x8000000000000000UL;

            return masks;
        }
    }
}