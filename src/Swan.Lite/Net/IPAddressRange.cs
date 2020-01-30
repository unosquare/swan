using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Swan.Net.Internal;

namespace Swan.Net
{
    // NOTE TO CONTRIBUTORS: When adding a check on a public method parameter,
    // please do not just "throw new ArgumentException(...)".
    // Instead, look at the exception-returning private methods at the bottom of this file
    // and either find one suitable for your case, or add a new one.
    // This way we can keep the exception messages consistent.

    /// <summary>
    /// Represents an inclusive range of IP addresses.
    /// </summary>
    /// <remarks>
    /// <para>This class makes no distinction between IPv4 addresses and the same addresses mapped to IPv6
    /// for the purpose of determining whether it belongs to a range: that is, the <see cref="Contains"/> method
    /// of an instance initialized with IPv4 addresses, or with the same addresses mapped to IPv6,
    /// will return <see langword="true"/> for both an in-range IPv4 address and the same address mapped to IPv6.</para>
    /// <para>The <see cref="IPAddressRange(IPAddress,IPAddress)"/> constructor, however,
    /// does make such distinction: you cannot initialize a range using an IPv4 address and an IPv6 address,
    /// even if the latter is an IPv4 address mapped to IPv6, nor the other way around.</para>
    /// </remarks>
    /// <seealso cref="IEquatable{IPAddressRange}" />
    [Serializable]
    public sealed class IPAddressRange : IEquatable<IPAddressRange>
    {
        /// <summary>
        /// <para>Gets an instance of <see cref="IPAddressRange"/> that contains no addresses.</para>
        /// <para>The <see cref="Contains"/> method of the returned instance will always return <see langword="false"/>.</para>
        /// <para>This property is useful to initialize non-nullable properties 
        /// of type <see cref="IPAddressRange"/>.</para>
        /// </summary>
        public static readonly IPAddressRange None = new IPAddressRange(IPAddressValue.MaxValue, IPAddressValue.MinValue, true, 0);

        /// <summary>
        /// <para>Gets an instance of <see cref="IPAddressRange"/> that contains all possible IP addresses.</para>
        /// <para>The <see cref="Contains"/> method of the returned instance will always return <see langword="true"/>.</para>
        /// </summary>
        public static readonly IPAddressRange All = new IPAddressRange(IPAddressValue.MinValue, IPAddressValue.MaxValue, true, 128);

        /// <summary>
        /// <para>Gets an instance of <see cref="IPAddressRange"/> that contains all IPv4 addresses.</para>
        /// <para>The <see cref="Contains"/> method of the returned instance will return <see langword="true"/>
        /// for all IPv4 addresses, as well as their IPv6 mapped counterparts, and <see langword="false"/>
        /// for all other IPv6 addresses.</para>
        /// </summary>
        public static readonly IPAddressRange AllIPv4 = new IPAddressRange(IPAddressValue.MinIPv4Value, IPAddressValue.MaxIPv4Value, false, 32);

        private readonly IPAddressValue _start;
        private readonly IPAddressValue _end;
        private readonly bool _isV6;
        private readonly byte _prefixLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class,
        /// representing a single IP address.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
        public IPAddressRange(IPAddress address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            _start = _end = new IPAddressValue(address);
            _isV6 = address.AddressFamily == AddressFamily.InterNetworkV6;
            _prefixLength = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class,
        /// representing a range of IP addresses between <paramref name="start"/>
        /// and <paramref name="end"/>, extremes included.
        /// </summary>
        /// <param name="start">The starting address of the range.</param>
        /// <param name="end">The ending address of the range.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="start"/> is <see langword="null"/>.</para>
        /// <para>- or -</para>
        /// <para><paramref name="end"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="end"/> has a different <see cref="IPAddress.AddressFamily">AddressFamily</see>
        /// from <paramref name="start"/>.</para>
        /// <para>- or -</para>
        /// <para><paramref name="end"/> is a lower address than <paramref name="start"/>,
        /// i.e. the binary representation of <paramref name="end"/> in network byte order
        /// is a lower number than the same representation of <paramref name="start"/>.</para>
        /// </exception>
        public IPAddressRange(IPAddress start, IPAddress end)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (end == null)
                throw new ArgumentNullException(nameof(end));

            var startFamily = start.AddressFamily;
            _isV6 = startFamily == AddressFamily.InterNetworkV6;
            if (end.AddressFamily != startFamily)
                throw MismatchedEndFamily(nameof(end));

            _start = new IPAddressValue(start);
            _end = new IPAddressValue(end);
            if (_end.CompareTo(_start) < 0)
                throw EndLowerThanStart(nameof(end));

            _prefixLength = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressRange"/> class,
        /// representing a CIDR subnet.
        /// </summary>
        /// <param name="baseAddress">The base address of the subnet.</param>
        /// <param name="prefixLength">The prefix length of the subnet.</param>
        /// <exception cref="ArgumentNullException"><paramref name="baseAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <para><paramref name="prefixLength"/> is zero.</para>
        /// <para>- or -</para>
        /// <para><paramref name="prefixLength"/> is greater than the number of bits in
        /// the binary representation of <paramref name="baseAddress"/> (32 for IPv4 addresses,
        /// 128 for IPv6 addresses.)</para>
        /// <para>- or -</para>
        /// <para><paramref name="baseAddress"/> cannot be the base address of a subnet with a prefix length
        /// equal to <paramref name="prefixLength"/>, because the remaining bits after the prefix
        /// are not all zeros.</para>
        /// </exception>
        public IPAddressRange(IPAddress baseAddress, byte prefixLength)
        {
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));

            byte maxPrefixLength;
            if (baseAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                _isV6 = true;
                maxPrefixLength = 128;
            }
            else
            {
                _isV6 = false;
                maxPrefixLength = 32;
            }

            if (prefixLength < 1 || prefixLength > maxPrefixLength)
                throw InvalidPrefixLength(nameof(prefixLength));

            _start = new IPAddressValue(baseAddress);
            if (!_start.IsStartOfSubnet(prefixLength))
                throw InvalidSubnetBaseAddress(nameof(baseAddress));

            _end = _start.GetEndOfSubnet(prefixLength);
            _prefixLength = prefixLength;
        }

        private IPAddressRange(IPAddressValue start, IPAddressValue end, bool isV6, byte prefixLength)
        {
            _start = start;
            _end = end;
            _isV6 = isV6;
            _prefixLength = prefixLength;
        }

        /// <summary>
        /// Gets the address family of the IP address range.
        /// </summary>
        /// <remarks>
        /// <para>Regardless of the value of this property, IPv4 addresses
        /// and their IPv6 mapped counterparts will be considered the same
        /// for the purposes of the <see cref="Contains"/> method.</para>
        /// </remarks>
        public AddressFamily AddressFamily => _isV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;

        /// <summary>
        /// Gets a value indicating whether this instance represents a CIDR subnet.
        /// </summary>
        /// <remarks>
        /// <para>This property is <see langword="true"/> only for instances
        /// initialized via the <see cref="IPAddressRange(IPAddress,byte)"/> constructor.
        /// Instances constructed by specifying a range will have this property
        /// set to <see langword="false"/> even when they actually represent a subnet.</para>
        /// <para>For example, the instance returned by <c>IPAddressRange.Parse("192.168.0.0-192.168.0.255")</c>
        /// will have this property set to <see langword="false"/>; for this property to be <see langword="true"/>,
        /// the string passed to <see cref="Parse"/> should instead be <c>"192.168.0.0/24"</c>
        /// (a CIDR subnet specification) or "192.168.0.0/255.255.255.0" (a base address / netmask pair,
        /// only accepted by <see cref="Parse"/> and <see cref="TryParse"/> for IPv4 addresses.)</para>
        /// </remarks>
        public bool IsSubnet => _prefixLength > 0;

        /// <summary>
        /// Gets an instance of <see cref="IPAddress"/> representing
        /// the first address in the range.
        /// </summary>
        public IPAddress Start => _start.ToIPAddress(_isV6);

        /// <summary>
        /// Gets an instance of <see cref="IPAddress"/> representing
        /// the last address in the range.
        /// </summary>
        public IPAddress End => _end.ToIPAddress(_isV6);

        /// <summary>
        /// Tries to convert the string representation of a range of IP addresses
        /// to an instance of <see cref="IPAddressRange"/>.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <param name="result">When this method returns <see langword="true"/>,
        /// an instance of <see cref="IPAddressRange"/> representing the same range of
        /// IP addresses represented by <paramref name="str"/>.</param>
        /// <returns><see langword="true"/> if the conversion was successful;
        /// otherwise, <see langword="false"/>.</returns>
        /// <remarks>See the "Remarks" section of <see cref="Parse"/>
        /// for an overview of the formats accepted for <paramref name="str"/>.</remarks>
        /// <seealso cref="Parse"/>
        public static bool TryParse(string str, out IPAddressRange result)
            => TryParseInternal(nameof(str), str, out result) == null;

        /// <summary>
        /// Converts the string representation of a range of IP addresses
        /// to an instance of <see cref="IPAddressRange"/>.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>An instance of <see cref="IPAddressRange"/> representing the same range of
        /// IP addresses represented by <paramref name="str"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="str"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException"><paramref name="str"/> is in none of the supported formats.</exception>
        /// <remarks>
        /// <para>This method supports the following formats for <paramref name="str"/>:</para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Format</term>
        ///     <term>Description</term>
        ///     <term>Examples</term>
        ///   </listheader>
        ///   <item>
        ///     <term>Single address</term>
        ///     <term>A single IP address.</term>
        ///     <term>
        ///       <para><c>192.168.23.199</c></para>
        ///       <para><c>2001:db8:a0b:12f0::1</c></para>
        ///     </term>
        ///   </item>
        ///   <item>
        ///     <term>Range of addresses</term>
        ///     <term>Start and end address, separated by a hyphen (<c>-</c>).</term>
        ///     <term>
        ///       <para><c>192.168.0.100-192.168.11.255</c></para>
        ///       <para><c>2001:db8:a0b:12f0::-2001:db8:a0b:12f0::ffff</c></para>
        ///     </term>
        ///   </item>
        ///   <item>
        ///     <term>CIDR subnet</term>
        ///     <term>Base address and prefix length, separated by a slash (<c>/</c>).</term>
        ///     <term>
        ///       <para><c>169.254.0.0/16</c></para>
        ///       <para><c>192.168.123.0/24</c></para>
        ///       <para><c>2001:db8:a0b:12f0::/64</c></para>
        ///     </term>
        ///   </item>
        ///   <item>
        ///     <term>"Legacy" subnet</term>
        ///     <term>
        ///       <para>Base address and netmask, separated by a slash (<c>/</c>).</para>
        ///       <para>Only accepted for IPv4 addresses.</para>
        ///     </term>
        ///     <term>
        ///       <para><c>169.254.0.0/255.255.0.0</c></para>
        ///       <para><c>192.168.123.0/255.255.255.0</c></para>
        ///     </term>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <seealso cref="TryParse"/>
        public static IPAddressRange Parse(string str)
        {
            var exception = TryParseInternal(nameof(str), str, out var result);
            if (exception != null)
                throw exception;

            return result;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>The result of this method will be a string that,
        /// if passed to the <see cref="Parse"/> or <see cref="TryParse"/> method,
        /// will result in an instance identical to this one.</para>
        /// <para>If this instance has been created by means of the <see cref="Parse"/>
        /// or <see cref="TryParse"/> method, the returned string will not
        /// necessarily be identical to the parsed string. The possible differences
        /// include the following:</para>
        /// <list type="bullet">
        ///   <item>ranges consisting of just one IP address will result in a
        ///   string representing that single address;</item>
        ///   <item>addresses in the returned string are passed to the
        ///   <see cref="IPAddress.ToString"/> method, resulting in standardized
        ///   representations that may be different from the originally parsed
        ///   strings;</item>
        ///   <item>the returned string will contain no blank characters;</item>
        ///   <item>address ranges parsed as <c>address/netmask</c> will be
        ///   rendered as CIDR subnets: for example,
        ///   <c>IPAddressRange.Parse("192.168.19.0/255.255.255.0").ToString()</c>
        ///   will return <c>"192.168.19.0/24"</c>.</item>
        /// </list>
        /// </remarks>
        public override string ToString()
            => _prefixLength > 0
                ? $"{Start}/{_prefixLength}"
                : _start.CompareTo(_end) == 0
                    ? Start.ToString()
                    : $"{Start}-{End}";

        /// <summary>
        /// Determines whether the given <paramref name="address"/>
        /// sa contained in this range.
        /// </summary>
        /// <param name="address">The IP address to check.</param>
        /// <returns><see langword="true"/> if <paramref name="address"/>
        /// is between <see cref="Start"/> and <see cref="End"/>, inclusive;
        /// otherwise, <see lamgword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>This method treats IPv4 addresses and their IPv6-mapped counterparts
        /// the same; that is, given a range obtained by parsing the string <c>192.168.1.0/24</c>,
        /// <c>Contains(IPAddress.Parse("192.168.1.55"))</c> will return <see langword="true"/>,
        /// as will <c>Contains(IPAddress.Parse("192.168.1.55").MapToIPv6())</c>. This is true
        /// as well if a range is initialized with IPv6 addresses.</para>
        /// </remarks>
        public bool Contains(IPAddress address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            var addressValue = new IPAddressValue(address);
            return addressValue.CompareTo(_start) >= 0
                && addressValue.CompareTo(_end) <= 0;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is IPAddressRange other && Equals(other);

        /// <inheritdoc/>
        public bool Equals(IPAddressRange? other)
            => other != null
            && other._start.Equals(_start)
            && other._end.Equals(_end)
            && other._isV6 == _isV6
            && other._prefixLength == _prefixLength;

        /// <inheritdoc />
        public override int GetHashCode() => CompositeHashCode.Using(_start, _end, _isV6, _prefixLength);

        private static bool TryNetmaskToCidrPrefixLength(byte[] bytes, out byte result)
        {
            result = 0;
            var length = bytes.Length;
            var prefixFound = false;
            for (var i = 0; i < length; i++)
            {
                if (prefixFound)
                {
                    if (bytes[i] != 0)
                        return false;
                }
                else
                {
                    switch (bytes[i])
                    {
                        case 0x00:
                            if (result == 0)
                                return false;

                            prefixFound = true;
                            break;
                        case 0x80:
                            result += 1;
                            prefixFound = true;
                            break;
                        case 0xC0:
                            result += 2;
                            prefixFound = true;
                            break;
                        case 0xE0:
                            result += 3;
                            prefixFound = true;
                            break;
                        case 0xF0:
                            result += 4;
                            prefixFound = true;
                            break;
                        case 0xF8:
                            result += 5;
                            prefixFound = true;
                            break;
                        case 0xFC:
                            result += 6;
                            prefixFound = true;
                            break;
                        case 0xFE:
                            result += 7;
                            prefixFound = true;
                            break;
                        case 0xFF:
                            result += 8;
                            break;
                        default:
                            return false;
                    }
                }
            }

            return true;
        }

        private static Exception? TryParseInternal(string paramName, string? str, out IPAddressRange result)
        {
            result = None;

            if (str == null)
                return new ArgumentNullException(paramName);

            // Try CIDR format (e.g. 192.168.99.0/24) and address/netmask format (192.168.99.0/255.255.255.0)
            var separatorPos = str.IndexOf('/');
            if (separatorPos >= 0)
                return TryParseCidrOrAddressNetmaskFormat(str, separatorPos, out result);

            // Try range format (e.g. 192.168.99.100-192.168.99.199)
            separatorPos = str.IndexOf('-');
            if (separatorPos >= 0)
                return TryParseStartEndFormat(str, separatorPos, out result);

            // Try single address format (e.g. 192.168.99.123)
            return TryParseSingleAddressFormat(str, out result);
        }

        private static Exception? TryParseCidrOrAddressNetmaskFormat(string str, int separatorPos, out IPAddressRange result)
        {
            result = None;

            var s = str.Substring(0, separatorPos).Trim();
            if (!IPAddressUtility.TryParse(s, out var address))
                return InvalidIPAddress();

            var addressValue = new IPAddressValue(address);

            s = str.Substring(separatorPos + 1).Trim();
            if (byte.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var prefixLength))
            {
                var maxPrefixLength = address.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;
                if (prefixLength < 1 || prefixLength > maxPrefixLength)
                    return InvalidPrefixLength();

                if (!addressValue.IsStartOfSubnet(prefixLength))
                    return InvalidSubnetBaseAddress();

                result = new IPAddressRange(addressValue, addressValue.GetEndOfSubnet(prefixLength), address.AddressFamily == AddressFamily.InterNetworkV6, prefixLength);
                return null;
            }

            // Only accept a netmask for IPv4
            if (address.AddressFamily != AddressFamily.InterNetwork)
                return InvalidPrefixLength();

            if (!IPAddressUtility.TryParse(s, out var netmask))
                return InvalidPrefixLengthOrNetmask();

            var addressFamily = address.AddressFamily;
            if (netmask.AddressFamily != addressFamily)
                return MismatchedNetmaskAddressFamily();

            var netmaskBytes = netmask.GetAddressBytes();
            if (!TryNetmaskToCidrPrefixLength(netmaskBytes, out prefixLength))
                return InvalidNetmask();

            if (!addressValue.IsStartOfSubnet(prefixLength))
                return InvalidSubnetBaseAddress();

            result = new IPAddressRange(addressValue, addressValue.GetEndOfSubnet(prefixLength), false, prefixLength);
            return null;
        }

        private static Exception? TryParseStartEndFormat(string str, int separatorPos, out IPAddressRange result)
        {
            result = None;

            var s = str.Substring(0, separatorPos).Trim();
            if (!IPAddressUtility.TryParse(s, out var startAddress))
                return InvalidStartAddress();

            s = str.Substring(separatorPos + 1).Trim();
            if (!IPAddressUtility.TryParse(s, out var endAddress))
                return InvalidEndAddress();

            var addressFamily = startAddress.AddressFamily;
            if (endAddress.AddressFamily != addressFamily)
                return MismatchedStartEndFamily();

            var start = new IPAddressValue(startAddress);
            var end = new IPAddressValue(endAddress);
            if (end.CompareTo(start) < 0)
                return EndLowerThanStart();

            result = new IPAddressRange(start, end, addressFamily == AddressFamily.InterNetworkV6, 0);
            return null;
        }

        private static Exception? TryParseSingleAddressFormat(string str, out IPAddressRange result)
        {
            result = None;

            if (!IPAddressUtility.TryParse(str, out var address))
                return InvalidIPAddress();

            var addressValue = new IPAddressValue(address);
            result = new IPAddressRange(addressValue, addressValue, address.AddressFamily == AddressFamily.InterNetworkV6, 0);
            return null;
        }

        private static Exception InvalidIPAddress() => new FormatException("An invalid IP address was specified.");

        private static Exception InvalidPrefixLengthOrNetmask() => new FormatException("An invalid prefix length or netmask was specified.");

        private static Exception MismatchedNetmaskAddressFamily() => new FormatException("Address and netmask are different types of addresses.");

        private static Exception InvalidPrefixLength() => new FormatException("An invalid prefix length was specified.");

        private static Exception InvalidPrefixLength(string paramName) => new ArgumentException("The prefix length is invalid.", paramName);

        private static Exception InvalidNetmask() => new FormatException("An invalid netmask was specified.");

        private static Exception InvalidSubnetBaseAddress() => new FormatException("The specified address is not the base address of the specified subnet.");

        private static Exception InvalidSubnetBaseAddress(string paramName) => new ArgumentException("The specified address is not the base address of the specified subnet.", paramName);

        private static Exception InvalidStartAddress() => new FormatException("An invalid start address was specified for a range.");

        private static Exception InvalidEndAddress() => new FormatException("An invalid end address was specified for a range.");

        private static Exception MismatchedStartEndFamily() => new FormatException("Start and end are different types of addresses.");

        private static Exception MismatchedEndFamily(string paramName) => new ArgumentException("The end address of a range must be of the same family as the start address.", paramName);

        private static Exception EndLowerThanStart() => new FormatException("An end address was specified for a range that is lower than the start address.");

        private static Exception EndLowerThanStart(string paramName) => new ArgumentException("The end address of a range cannot be lower than the start address.", paramName);
    }
}