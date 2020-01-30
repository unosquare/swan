using System.Net;

namespace Swan.Net
{
    /// <summary>
    /// Provides utility methods to work with the <see cref="IPAddress"/> class.
    /// </summary>
    public static class IPAddressUtility
    {
        /// <summary>
        /// <para>Tries to convert the string representation of an IP address
        /// into an instance of <see cref="IPAddress"/></para>
        /// <para>This method works the same way as <see cref="IPAddress.TryParse"/>,
        /// with the exception that it will not recognize a decimal number alone
        /// as an IPv4 address.</para>
        /// </summary>
        /// <param name="str">The string to be converted.</param>
        /// <param name="address">When this method returns <see langword="true"/>,
        /// an instance of <see cref="IPAddress"/> representing the same address
        /// as <paramref name="str"/>.</param>
        /// <returns><see langword="true"/> if the conversion was successful;
        /// otherwise, <see langword="false"/>.</returns>
        public static bool TryParse(string? str, out IPAddress address)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.ipaddress.tryparse
            // "Note that this method accepts as valid an ipString value that can be parsed as an Int64,
            // and then treats that Int64 as the long value of an IP address in network byte order,
            // similar to the way that the IPAddress constructor does.
            // This means that this method returns true if the Int64 is parsed successfully,
            // even if it represents an address that's not a valid IP address.
            // For example, if str is "1", this method returns true even though "1" (or 0.0.0.1)
            // is not a valid IP address and you might expect this method to return false.
            // Fixing this bug would break existing apps, so the current behavior will not be changed.
            // Your code can avoid this behavior by ensuring that it only uses this method
            // to parse IP addresses in dotted-decimal format."
            // ---
            // Thus, if it parses as an Int64, let's just refuse it.
            // One-part IPv4 addresses be darned.
            address = IPAddress.None;
            return !long.TryParse(str, out _) && IPAddress.TryParse(str, out address);
        }
    }
}