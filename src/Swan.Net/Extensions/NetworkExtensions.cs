﻿namespace Swan.Net.Extensions;

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Provides various extension methods for networking-related tasks.
/// </summary>
public static class NetworkExtensions
{
    /// <summary>
    /// Determines whether the IP address is private.
    /// </summary>
    /// <param name="this">The IP address.</param>
    /// <returns>
    /// True if the IP Address is private; otherwise, false.
    /// </returns>
    /// <exception cref="ArgumentNullException">address.</exception>
    public static bool IsPrivateAddress(this IPAddress @this)
    {
        if (@this == null)
            throw new ArgumentNullException(nameof(@this));

        var octets = @this.ToString().Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries).Select(byte.Parse).ToArray();
        var is24Bit = octets[0] == 10;
        var is20Bit = octets[0] == 172 && (octets[1] >= 16 && octets[1] <= 31);
        var is16Bit = octets[0] == 192 && octets[1] == 168;

        return is24Bit || is20Bit || is16Bit;
    }

    /// <summary>
    /// Converts an IPv4 Address to its Unsigned, 32-bit integer representation.
    /// </summary>
    /// <param name="this">The address.</param>
    /// <returns>
    /// A 32-bit unsigned integer converted from four bytes at a specified position in a byte array.
    /// </returns>
    /// <exception cref="ArgumentNullException">address.</exception>
    /// <exception cref="ArgumentException">InterNetwork - address.</exception>
    public static uint ToUInt32(this IPAddress @this)
    {
        if (@this == null)
            throw new ArgumentNullException(nameof(@this));

        if (@this.AddressFamily != AddressFamily.InterNetwork)
            throw new ArgumentException($"Address has to be of family '{nameof(AddressFamily.InterNetwork)}'", nameof(@this));

        var addressBytes = @this.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(addressBytes);

        return BitConverter.ToUInt32(addressBytes, 0);
    }
}
