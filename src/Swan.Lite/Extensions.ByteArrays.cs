using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swan
{
    /// <summary>
    /// Provides various extension methods for byte arrays and streams.
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Converts an array of bytes to its lower-case, hexadecimal representation.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="addPrefix">if set to <c>true</c> add the 0x prefix tot he output.</param>
        /// <returns>
        /// The specified string instance; no actual conversion is performed.
        /// </returns>
        /// <exception cref="ArgumentNullException">bytes.</exception>
        public static string ToLowerHex(this byte[] bytes, bool addPrefix = false)
            => ToHex(bytes, addPrefix, "x2");

        /// <summary>
        /// Converts an array of bytes to its upper-case, hexadecimal representation.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="addPrefix">if set to <c>true</c> [add prefix].</param>
        /// <returns>
        /// The specified string instance; no actual conversion is performed.
        /// </returns>
        /// <exception cref="ArgumentNullException">bytes.</exception>
        public static string ToUpperHex(this byte[] bytes, bool addPrefix = false)
            => ToHex(bytes, addPrefix, "X2");

        /// <summary>
        /// Converts an array of bytes to a sequence of dash-separated, hexadecimal,
        /// uppercase characters.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>
        /// A string of hexadecimal pairs separated by hyphens, where each pair represents
        /// the corresponding element in value; for example, "7F-2C-4A-00".
        /// </returns>
        public static string ToDashedHex(this byte[] bytes) => BitConverter.ToString(bytes);

        /// <summary>
        /// Converts an array of bytes to a base-64 encoded string.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>A <see cref="string" /> converted from an array of bytes.</returns>
        public static string ToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);

        /// <summary>
        /// Converts a set of hexadecimal characters (uppercase or lowercase)
        /// to a byte array. String length must be a multiple of 2 and
        /// any prefix (such as 0x) has to be avoided for this to work properly.
        /// </summary>
        /// <param name="this">The hexadecimal.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters.
        /// </returns>
        /// <exception cref="ArgumentNullException">hex.</exception>
        public static byte[] ConvertHexadecimalToBytes(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
                throw new ArgumentNullException(nameof(@this));

            return Enumerable
                .Range(0, @this.Length / 2)
                .Select(x => Convert.ToByte(@this.Substring(x * 2, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// Gets the bit value at the given offset.
        /// </summary>
        /// <param name="this">The b.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// Bit value at the given offset.
        /// </returns>
        public static byte GetBitValueAt(this byte @this, byte offset, byte length = 1) => (byte)((@this >> offset) & ~(0xff << length));

        /// <summary>
        /// Sets the bit value at the given offset.
        /// </summary>
        /// <param name="this">The b.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="value">The value.</param>
        /// <returns>Bit value at the given offset.</returns>
        public static byte SetBitValueAt(this byte @this, byte offset, byte length, byte value)
        {
            var mask = ~(0xff << length);
            var valueAt = (byte)(value & mask);

            return (byte)((valueAt << offset) | (@this & ~(mask << offset)));
        }

        /// <summary>
        /// Sets the bit value at the given offset.
        /// </summary>
        /// <param name="this">The b.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        /// <returns>Bit value at the given offset.</returns>
        public static byte SetBitValueAt(this byte @this, byte offset, byte value) => @this.SetBitValueAt(offset, 1, value);

        /// <summary>
        /// Splits a byte array delimited by the specified sequence of bytes.
        /// Each individual element in the result will contain the split sequence terminator if it is found to be delimited by it.
        /// For example if you split [1,2,3,4] by a sequence of [2,3] this method will return a list with 2 byte arrays, one containing [1,2,3] and the
        /// second one containing 4. Use the Trim extension methods to remove terminator sequences.
        /// </summary>
        /// <param name="this">The buffer.</param>
        /// <param name="offset">The offset at which to start splitting bytes. Any bytes before this will be discarded.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>
        /// A byte array containing the results the specified sequence of bytes.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// buffer
        /// or
        /// sequence.
        /// </exception>
        public static List<byte[]> Split(this byte[] @this, int offset, params byte[] sequence)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var seqOffset = offset.Clamp(0, @this.Length - 1);

            var result = new List<byte[]>();

            while (seqOffset < @this.Length)
            {
                var separatorStartIndex = @this.GetIndexOf(sequence, seqOffset);

                if (separatorStartIndex >= 0)
                {
                    var item = new byte[separatorStartIndex - seqOffset + sequence.Length];
                    Array.Copy(@this, seqOffset, item, 0, item.Length);
                    result.Add(item);
                    seqOffset += item.Length;
                }
                else
                {
                    var item = new byte[@this.Length - seqOffset];
                    Array.Copy(@this, seqOffset, item, 0, item.Length);
                    result.Add(item);
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Clones the specified buffer, byte by byte.
        /// </summary>
        /// <param name="this">The buffer.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters.
        /// </returns>
        /// <exception cref="ArgumentNullException">this</exception>
        public static byte[] DeepClone(this byte[] @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var result = new byte[@this.Length];
            Array.Copy(@this, result, @this.Length);
            return result;
        }

        /// <summary>
        /// Removes the specified sequence from the start of the buffer if the buffer begins with such sequence.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>
        /// A new trimmed byte array.
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer.</exception>
        public static byte[] TrimStart(this byte[] buffer, params byte[] sequence)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (buffer.StartsWith(sequence) == false)
                return buffer.DeepClone();

            var result = new byte[buffer.Length - sequence.Length];
            Array.Copy(buffer, sequence.Length, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Removes the specified sequence from the end of the buffer if the buffer ends with such sequence.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters.
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer.</exception>
        public static byte[] TrimEnd(this byte[] buffer, params byte[] sequence)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (buffer.EndsWith(sequence) == false)
                return buffer.DeepClone();

            var result = new byte[buffer.Length - sequence.Length];
            Array.Copy(buffer, 0, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Removes the specified sequence from the end and the start of the buffer 
        /// if the buffer ends and/or starts with such sequence.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
        public static byte[] Trim(this byte[] buffer, params byte[] sequence)
        {
            var trimStart = buffer.TrimStart(sequence);
            return trimStart.TrimEnd(sequence);
        }

        /// <summary>
        /// Determines if the specified buffer ends with the given sequence of bytes.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>
        /// True if the specified buffer is ends; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer.</exception>
        public static bool EndsWith(this byte[] buffer, params byte[] sequence)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var startIndex = buffer.Length - sequence.Length;
            return buffer.GetIndexOf(sequence, startIndex) == startIndex;
        }

        /// <summary>
        /// Determines if the specified buffer starts with the given sequence of bytes.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns><c>true</c> if the specified buffer starts; otherwise, <c>false</c>.</returns>
        public static bool StartsWith(this byte[] buffer, params byte[] sequence) => buffer.GetIndexOf(sequence) == 0;

        /// <summary>
        /// Determines whether the buffer contains the specified sequence.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified sequence]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this byte[] buffer, params byte[] sequence) => buffer.GetIndexOf(sequence) >= 0;

        /// <summary>
        /// Determines whether the buffer exactly matches, byte by byte the specified sequence.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>
        ///   <c>true</c> if [is equal to] [the specified sequence]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer.</exception>
        public static bool IsEqualTo(this byte[] buffer, params byte[] sequence)
        {
            if (ReferenceEquals(buffer, sequence))
                return true;

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            return buffer.Length == sequence.Length && buffer.GetIndexOf(sequence) == 0;
        }

        /// <summary>
        /// Returns the first instance of the matched sequence based on the given offset.
        /// If no matches are found then this method returns -1.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The index of the sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// buffer
        /// or
        /// sequence.
        /// </exception>
        public static int GetIndexOf(this byte[] buffer, byte[] sequence, int offset = 0)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (sequence.Length == 0)
                return -1;
            if (sequence.Length > buffer.Length)
                return -1;

            var seqOffset = offset < 0 ? 0 : offset;

            var matchedCount = 0;
            for (var i = seqOffset; i < buffer.Length; i++)
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
        /// <returns>
        /// The same MemoryStream instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// stream
        /// or
        /// buffer.
        /// </exception>
        public static MemoryStream Append(this MemoryStream stream, byte[] buffer)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            stream.Write(buffer, 0, buffer.Length);
            return stream;
        }

        /// <summary>
        /// Appends the Memory Stream with the specified buffer.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// Block of bytes to the current stream using data read from a buffer.
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer.</exception>
        public static MemoryStream Append(this MemoryStream stream, IEnumerable<byte> buffer) => Append(stream, buffer?.ToArray());

        /// <summary>
        /// Appends the Memory Stream with the specified set of buffers.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffers">The buffers.</param>
        /// <returns>
        /// Block of bytes to the current stream using data read from a buffer.
        /// </returns>
        /// <exception cref="ArgumentNullException">buffers.</exception>
        public static MemoryStream Append(this MemoryStream stream, IEnumerable<byte[]> buffers)
        {
            if (buffers == null)
                throw new ArgumentNullException(nameof(buffers));

            foreach (var buffer in buffers)
                Append(stream, buffer);

            return stream;
        }

        /// <summary>
        /// Converts an array of bytes into text with the specified encoding.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>A <see cref="string" /> that contains the results of decoding the specified sequence of bytes.</returns>
        public static string ToText(this IEnumerable<byte> buffer, Encoding encoding) =>
            encoding == null
                ? throw new ArgumentNullException(nameof(encoding))
                : encoding.GetString(buffer.ToArray());

        /// <summary>
        /// Converts an array of bytes into text with UTF8 encoding.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>A <see cref="string" /> that contains the results of decoding the specified sequence of bytes.</returns>
        public static string ToText(this IEnumerable<byte> buffer) => buffer.ToText(Encoding.UTF8);
        
        /// <summary>
        /// Reads the bytes asynchronous.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="length">The length.</param>
        /// <param name="bufferLength">Length of the buffer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters.
        /// </returns>
        /// <exception cref="ArgumentNullException">stream.</exception>
        public static async Task<byte[]> ReadBytesAsync(this Stream stream, long length, int bufferLength, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var dest = new MemoryStream();

            try
            {
                var buff = new byte[bufferLength];
                while (length > 0)
                {
                    if (length < bufferLength)
                        bufferLength = (int)length;

                    var read = await stream.ReadAsync(buff, 0, bufferLength, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                        break;

                    dest.Write(buff, 0, read);
                    length -= read;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // ignored
            }

            return dest.ToArray();
        }

        /// <summary>
        /// Reads the bytes asynchronous.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="length">The length.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters.
        /// </returns>
        /// <exception cref="ArgumentNullException">stream.</exception>
        public static async Task<byte[]> ReadBytesAsync(this Stream stream, int length, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var buff = new byte[length];
            var offset = 0;

            try
            {
                while (length > 0)
                {
                    var read = await stream.ReadAsync(buff, offset, length, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                        break;

                    offset += read;
                    length -= read;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // ignored
            }

            return new ArraySegment<byte>(buff, 0, offset).ToArray();
        }

        private static string ToHex(byte[] bytes, bool addPrefix, string format)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var sb = new StringBuilder(bytes.Length * 2);

            foreach (var item in bytes)
                sb.Append(item.ToString(format, CultureInfo.InvariantCulture));

            return $"{(addPrefix ? "0x" : string.Empty)}{sb}";
        }
    }
}
