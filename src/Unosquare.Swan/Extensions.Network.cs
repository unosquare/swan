namespace Unosquare.Swan
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    public partial class Extensions
    {
        /// <summary>
        /// Determines whether the IP address is private
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <returns>True if the IP Address is private; otherwise, false</returns>
        public static bool IsPrivateAddress(this IPAddress address)
        {
            var octets = address.ToString().Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).Select(byte.Parse).ToArray();
            var is24Bit = octets[0] == 10;
            var is20Bit = octets[0] == 172 && (octets[1] >= 16 && octets[1] <= 31);
            var is16Bit = octets[0] == 192 && octets[1] == 168;

            return is24Bit || is20Bit || is16Bit;
        }

        /// <summary>
        /// Converts an IPv4 Address to its Unsigned, 32-bit integer representation.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>method returns an unit</returns>
        public static uint ToUInt32(this IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException($"Address has to be of family '{nameof(AddressFamily.InterNetwork)}'", nameof(address));

            var addressBytes = address.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(addressBytes);

            return BitConverter.ToUInt32(addressBytes, 0);
        }
    }
}
