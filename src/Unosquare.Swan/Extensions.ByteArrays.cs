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
        public static string ToDashedHex(this byte[] bytes) => BitConverter.ToString(bytes);

        /// <summary>
        /// Converts an array of bytes to a base-64 encoded string
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);

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
            var mask = ~(0xff << length);
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
        /// Splits a byte array delimited by the specified sequence of bytes.
        /// Each individual element in the result will contain the split sequence terminator if it is found to be delimited by it.
        /// For example if you split [1,2,3,4] by a sequence of [2,3] this method will return a list with 2 byte arrays, one containing [1,2,3] and the
        /// second one containing 4. Use the Trim extension methods to remove terminator sequences.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset at which to start splitting bytes. Any bytes befor this will be discarded.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer
        /// or
        /// sequence
        /// </exception>
        public static List<byte[]> Split(this byte[] buffer, int offset, params byte[] sequence)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            offset = offset.Clamp(0, buffer.Length - 1);

            var result = new List<byte[]>();

            while (offset < buffer.Length)
            {
                var separatorStartIndex = buffer.GetIndexOf(sequence, offset);

                if (separatorStartIndex >= 0)
                {
                    var item = new byte[separatorStartIndex - offset + sequence.Length];
                    Array.Copy(buffer, offset, item, 0, item.Length);
                    result.Add(item);
                    offset += item.Length;
                }
                else
                {
                    var item = new byte[buffer.Length - offset];
                    Array.Copy(buffer, offset, item, 0, item.Length);
                    result.Add(item);
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Colones the specified buffer, byte by byte
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static byte[] DeepClone(this byte[] buffer)
        {
            if (buffer == null)
                return null;
            var result = new byte[buffer.Length];
            Array.Copy(buffer, result, buffer.Length);
            return result;
        }

        /// <summary>
        /// Removes the specified sequence from the start of the buffer if the buffer begins with such sequence
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        public static byte[] TrimStart(this byte[] buffer, params byte[] sequence)
        {
            if (buffer.StartsWith(sequence) == false)
                return buffer.DeepClone();
            var result = new byte[buffer.Length - sequence.Length];
            Array.Copy(buffer, sequence.Length, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Removes the specified sequence from the end of the buffer if the buffer ends with such sequence
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns></returns>
        public static byte[] TrimEnd(this byte[] buffer, params byte[] sequence)
        {
            if (buffer.EndsWith(sequence) == false) return buffer.DeepClone();
            var result = new byte[buffer.Length - sequence.Length];
            Array.Copy(buffer, 0, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Removes the specified sequence from the end and the start of the buffer 
        /// if the buffer ends and/or starts with such sequence
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns></returns>
        public static byte[] Trim(this byte[] buffer, params byte[] sequence)
        {
            var trimStart = buffer.TrimStart(sequence);
            return trimStart.TrimEnd(sequence);
        }

        /// <summary>
        /// Determines if the specified buffer ends with the given sequence of bytes
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns></returns>
        public static bool EndsWith(this byte[] buffer, params byte[] sequence)
        {
            var startIndex = buffer.Length - sequence.Length;
            return buffer.GetIndexOf(sequence, startIndex) == startIndex;
        }

        /// <summary>
        /// Determines if the specified buffer starts with the given sequence of bytes
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns></returns>
        public static bool StartsWith(this byte[] buffer, params byte[] sequence)
        {
            return buffer.GetIndexOf(sequence, 0) == 0;
        }

        /// <summary>
        /// Determines whether the buffer contains the specified sequence
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        public static bool Contains(this byte[] buffer, params byte[] sequence)
        {
            return buffer.GetIndexOf(sequence, 0) >= 0;
        }

        /// <summary>
        /// Determines whether the buffer exactly matches, byte by byte the specified sequence.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        public static bool IsEqualTo(this byte[] buffer, params byte[] sequence)
        {
            if (ReferenceEquals(buffer, sequence)) return true;
            return buffer.Length == sequence.Length && buffer.GetIndexOf(sequence, 0) == 0;
        }

        /// <summary>
        /// Returns the first instance of the matched sequence based on the given offset.
        /// If nomatches are found then this method returns -1
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="offset">The offset.</param>
        public static int GetIndexOf(this byte[] buffer, byte[] sequence, int offset = 0)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            if (sequence.Length == 0) return -1;
            if (sequence.Length > buffer.Length) return -1;
            if (offset < 0) offset = 0;

            var matchedCount = 0;
            for (var i = offset; i < buffer.Length; i++)
            {
                if (buffer[i] == sequence[matchedCount])
                    matchedCount++;
                else
                    matchedCount = 0;

                if (matchedCount == sequence.Length)
                    return i - (matchedCount - 1);
            }

            return -1;
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
        public static string ToText(this byte[] buffer, Encoding encoding) => encoding.GetString(buffer);

        /// <summary>
        /// Converts an array of bytes into text with UTF8 encoding
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static string ToText(this byte[] buffer) => buffer.ToText(Encoding.UTF8);
    }
}
