namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    partial class Extensions
    {
        /// <summary>
        /// Converts an array of bytes to its lower-case, hexadecimal representation
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="addPrefix">if set to <c>true</c> add the 0x prefix tot he output.</param>
        /// <returns></returns>
        public static string ToLowerHex(this byte[] bytes, bool addPrefix = false)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var item in bytes)
                sb.Append(item.ToString("x2", CultureInfo.InvariantCulture));

            return $"{(addPrefix ? "0x" : string.Empty)}{sb}";
        }

        /// <summary>
        /// Converts an array of bytes to its upper-case, hexadecimal representation
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="addPrefix">if set to <c>true</c> [add prefix].</param>
        /// <returns></returns>
        public static string ToUpperHex(this byte[] bytes, bool addPrefix = false)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var item in bytes)
                sb.Append(item.ToString("X2", CultureInfo.InvariantCulture));

            return $"{(addPrefix ? "0x" : string.Empty)}{sb}";
        }

        /// <summary>
        /// Converts an array of bytes to a sequence of dash-separated, hexadecimal,
        /// uppercase characters
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToDashedHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }

        /// <summary>
        /// Converts an array of bytes to a base-64 encoded string
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts a set of hexadecimal characters (uppercase or lowercase)
        /// to a byte array. String length must be a multiple of 2 and 
        /// any prefix (such as 0x) has to be avoided for this to work properly
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        public static byte[] ConvertHexadecimalToBytes(this string hex)
        {
            return Enumerable
                .Range(0, hex.Length / 2)
                .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// Gets the bit value at the given offset.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static byte GetBitValueAt(this byte b, byte offset, byte length)
        {
            return (byte)((b >> offset) & ~(0xff << length));
        }

        /// <summary>
        /// Gets the bit value at the given offset.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static byte GetBitValueAt(this byte b, byte offset)
        {
            return b.GetBitValueAt(offset, 1);
        }

        /// <summary>
        /// Sets the bit value at the given offset.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static byte SetBitValueAt(this byte b, byte offset, byte length, byte value)
        {
            int mask = ~(0xff << length);
            value = (byte)(value & mask);

            return (byte)((value << offset) | (b & ~(mask << offset)));
        }

        /// <summary>
        /// Sets the bit value at the given offset.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static byte SetBitValueAt(this byte b, byte offset, byte value)
        {
            return b.SetBitValueAt(offset, 1, value);
        }

        /// <summary>
        /// Appends the Memory Stream with the specified buffer.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">The buffer.</param>
        public static MemoryStream Append(this MemoryStream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
            return stream;
        }

        /// <summary>
        /// Appends the Memory Stream with the specified buffer.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static MemoryStream Append(this MemoryStream stream, IEnumerable<byte> buffer)
        {
            return Append(stream, buffer.ToArray());
        }

        /// <summary>
        /// Appends the Memory Stream with the specified set of buffers.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffers">The buffers.</param>
        /// <returns></returns>
        public static MemoryStream Append(this MemoryStream stream, IEnumerable<byte[]> buffers)
        {
            foreach (var buffer in buffers)
                Append(stream, buffer);

            return stream;
        }

        /// <summary>
        /// Converts an array of bytes into text with the specified encoding
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static string ToText(this byte[] buffer, Encoding encoding)
        {
            return encoding.GetString(buffer);
        }

        /// <summary>
        /// Converts an array of bytes into text with UTF8 encoding
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static string ToText(this byte[] buffer)
        {
            return buffer.ToText(Encoding.UTF8);
        }

    }
}
