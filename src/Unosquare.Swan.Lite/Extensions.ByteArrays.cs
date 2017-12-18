namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides various extension methods for byte arrays and streams
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Converts an array of bytes to its lower-case, hexadecimal representation
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="addPrefix">if set to <c>true</c> add the 0x prefix tot he output.</param>
        /// <returns>
        /// The specified string instance; no actual conversion is performed
        /// </returns>
        /// <exception cref="ArgumentNullException">bytes</exception>
        public static string ToLowerHex(this byte[] bytes, bool addPrefix = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

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
        /// <returns>
        /// The specified string instance; no actual conversion is performed
        /// </returns>
        /// <exception cref="ArgumentNullException">bytes</exception>
        public static string ToUpperHex(this byte[] bytes, bool addPrefix = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

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
        /// <returns>
        /// A string of hexadecimal pairs separated by hyphens, where each pair represents
        /// the corresponding element in value; for example, "7F-2C-4A-00"
        /// </returns>
        public static string ToDashedHex(this byte[] bytes) => BitConverter.ToString(bytes);

        /// <summary>
        /// Converts an array of bytes to a base-64 encoded string
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>A <see cref="System.String" /> converted from an array of bytes</returns>
        public static string ToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);

        /// <summary>
        /// Converts a set of hexadecimal characters (uppercase or lowercase)
        /// to a byte array. String length must be a multiple of 2 and
        /// any prefix (such as 0x) has to be avoided for this to work properly
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters
        /// </returns>
        /// <exception cref="ArgumentNullException">hex</exception>
        public static byte[] ConvertHexadecimalToBytes(this string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentNullException(nameof(hex));

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
        /// <returns>
        /// Bit value at the given offset
        /// </returns>
        public static byte GetBitValueAt(this byte b, byte offset, byte length = 1)
        {
            return (byte)((b >> offset) & ~(0xff << length));
        }
        
        /// <summary>
        /// Sets the bit value at the given offset.
        /// </summary>
        /// <param name="b">The b.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="value">The value.</param>
        /// <returns>Bit value at the given offset</returns>
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
        /// <returns>Bit value at the given offset</returns>
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
        /// <returns>
        /// A byte array containing the results the specified sequence of bytes
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer
        /// or
        /// sequence
        /// </exception>
        public static List<byte[]> Split(this byte[] buffer, int offset, params byte[] sequence)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

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
        /// Clones the specified buffer, byte by byte
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters</returns>
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
        /// <returns>
        /// A new trimmed byte array
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer</exception>
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
        /// Removes the specified sequence from the end of the buffer if the buffer ends with such sequence
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer</exception>
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
        /// if the buffer ends and/or starts with such sequence
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters</returns>
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
        /// <returns>
        /// True if the specified buffer is ends; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer</exception>
        public static bool EndsWith(this byte[] buffer, params byte[] sequence)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var startIndex = buffer.Length - sequence.Length;
            return buffer.GetIndexOf(sequence, startIndex) == startIndex;
        }

        /// <summary>
        /// Determines if the specified buffer starts with the given sequence of bytes
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <returns><c>true</c> if the specified buffer starts; otherwise, <c>false</c>.</returns>
        public static bool StartsWith(this byte[] buffer, params byte[] sequence) => buffer.GetIndexOf(sequence) == 0;

        /// <summary>
        /// Determines whether the buffer contains the specified sequence
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
        /// <exception cref="ArgumentNullException">buffer</exception>
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
        /// If nomatches are found then this method returns -1
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The index of the sequence</returns>
        /// <exception cref="ArgumentNullException">
        /// buffer
        /// or
        /// sequence
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
        /// <returns>
        /// The same MemoryStream instance
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// stream
        /// or
        /// buffer
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
        /// Block of bytes to the current stream using data read from a buffer
        /// </returns>
        /// <exception cref="ArgumentNullException">buffer</exception>
        public static MemoryStream Append(this MemoryStream stream, IEnumerable<byte> buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            return Append(stream, buffer.ToArray());
        }

        /// <summary>
        /// Appends the Memory Stream with the specified set of buffers.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="buffers">The buffers.</param>
        /// <returns>
        /// Block of bytes to the current stream using data read from a buffer
        /// </returns>
        /// <exception cref="ArgumentNullException">buffers</exception>
        public static MemoryStream Append(this MemoryStream stream, IEnumerable<byte[]> buffers)
        {
            if (buffers == null)
                throw new ArgumentNullException(nameof(buffers));

            foreach (var buffer in buffers)
                Append(stream, buffer);

            return stream;
        }

        /// <summary>
        /// Converts an array of bytes into text with the specified encoding
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>A <see cref="System.String" /> that contains the results of decoding the specified sequence of bytes</returns>
        public static string ToText(this byte[] buffer, Encoding encoding) => encoding.GetString(buffer);

        /// <summary>
        /// Converts an array of bytes into text with UTF8 encoding
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>A <see cref="System.String" /> that contains the results of decoding the specified sequence of bytes</returns>
        public static string ToText(this byte[] buffer) => buffer.ToText(Encoding.UTF8);

        /// <summary>
        /// Retrieves a sub-array from the specified <paramref name="array"/>. A sub-array starts at
        /// the specified element position in <paramref name="array"/>.
        /// </summary>
        /// <returns>
        /// An array of T that receives a sub-array, or an empty array of T if any problems with
        /// the parameters.
        /// </returns>
        /// <param name="array">
        /// An array of T from which to retrieve a sub-array.
        /// </param>
        /// <param name="startIndex">
        /// An <see cref="int"/> that represents the zero-based starting position of
        /// a sub-array in <paramref name="array"/>.
        /// </param>
        /// <param name="length">
        /// An <see cref="int"/> that represents the number of elements to retrieve.
        /// </param>
        /// <typeparam name="T">
        /// The type of elements in <paramref name="array"/>.
        /// </typeparam>
        public static T[] SubArray<T>(this T[] array, int startIndex, int length)
        {
            int len;
            if (array == null || (len = array.Length) == 0)
                return new T[0];

            if (startIndex < 0 || length <= 0 || startIndex + length > len)
                return new T[0];

            if (startIndex == 0 && length == len)
                return array;

            var subArray = new T[length];
            Array.Copy(array, startIndex, subArray, 0, length);

            return subArray;
        }

        /// <summary>
        /// Retrieves a sub-array from the specified <paramref name="array"/>. A sub-array starts at
        /// the specified element position in <paramref name="array"/>.
        /// </summary>
        /// <returns>
        /// An array of T that receives a sub-array, or an empty array of T if any problems with
        /// the parameters.
        /// </returns>
        /// <param name="array">
        /// An array of T from which to retrieve a sub-array.
        /// </param>
        /// <param name="startIndex">
        /// A <see cref="long"/> that represents the zero-based starting position of
        /// a sub-array in <paramref name="array"/>.
        /// </param>
        /// <param name="length">
        /// A <see cref="long"/> that represents the number of elements to retrieve.
        /// </param>
        /// <typeparam name="T">
        /// The type of elements in <paramref name="array"/>.
        /// </typeparam>
        public static T[] SubArray<T>(this T[] array, long startIndex, long length)
        {
            return array.SubArray((int)startIndex, (int)length);
        }

        /// <summary>
        /// Reads the bytes asynchronous.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="length">The length.</param>
        /// <param name="bufferLength">Length of the buffer.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters
        /// </returns>
        /// <exception cref="ArgumentNullException">stream</exception>
        public static async Task<byte[]> ReadBytesAsync(this Stream stream, long length, int bufferLength, CancellationToken ct = default(CancellationToken))
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var dest = new MemoryStream())
            {
                try
                {
                    var buff = new byte[bufferLength];
                    while (length > 0)
                    {
                        if (length < bufferLength)
                            bufferLength = (int)length;

                        var nread = await stream.ReadAsync(buff, 0, bufferLength, ct);
                        if (nread == 0)
                            break;

                        dest.Write(buff, 0, nread);
                        length -= nread;
                    }
                }
                catch
                {
                    // ignored
                }

                return dest.ToArray();
            }
        }

        /// <summary>
        /// Reads the bytes asynchronous.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="length">The length.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A byte array containing the results of encoding the specified set of characters
        /// </returns>
        /// <exception cref="ArgumentNullException">stream</exception>
        public static async Task<byte[]> ReadBytesAsync(this Stream stream, int length, CancellationToken ct = default(CancellationToken))
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var buff = new byte[length];
            var offset = 0;
            try
            {
                while (length > 0)
                {
                    var nread = await stream.ReadAsync(buff, offset, length, ct);
                    if (nread == 0)
                        break;

                    offset += nread;
                    length -= nread;
                }
            }
            catch
            {
                // ignored
            }

            return buff.SubArray(0, offset);
        }

        /// <summary>
        /// Converts an array of sbytes to an array of bytes
        /// </summary>
        /// <param name="sbyteArray">The sbyte array.</param>
        /// <returns>
        /// The byte array from conversion
        /// </returns>
        /// <exception cref="ArgumentNullException">sbyteArray</exception>
        public static byte[] ToByteArray(this sbyte[] sbyteArray)
        {
            if (sbyteArray == null)
                throw new ArgumentNullException(nameof(sbyteArray));

            var byteArray = new byte[sbyteArray.Length];
            for (var index = 0; index < sbyteArray.Length; index++)
                byteArray[index] = (byte)sbyteArray[index];
            return byteArray;
        }

        /// <summary>
        /// Receives a byte array and returns it transformed in an sbyte array
        /// </summary>
        /// <param name="byteArray">The byte array.</param>
        /// <returns>
        /// The sbyte array from conversion
        /// </returns>
        /// <exception cref="ArgumentNullException">byteArray</exception>
        public static sbyte[] ToSByteArray(this byte[] byteArray)
        {
            if (byteArray == null)
                throw new ArgumentNullException(nameof(byteArray));

            var sbyteArray = new sbyte[byteArray.Length];
            for (var index = 0; index < byteArray.Length; index++)
                sbyteArray[index] = (sbyte)byteArray[index];
            return sbyteArray;
        }

        /// <summary>
        /// Gets the sbytes from a string.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="s">The s.</param>
        /// <returns>The sbyte array from string</returns>
        public static sbyte[] GetSBytes(this Encoding encoding, string s)
            => encoding.GetBytes(s).ToSByteArray();

        /// <summary>
        /// Gets the string from a sbyte array.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="data">The data.</param>
        /// <returns>The string</returns>
        public static string GetString(this Encoding encoding, sbyte[] data)
            => encoding.GetString(data.ToByteArray());

        /// <summary>
        /// Reads a number of characters from the current source Stream and writes the data to the target array at the
        /// specified index.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="target">The target.</param>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        /// <returns>
        /// The number of bytes read
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// sourceStream
        /// or
        /// target
        /// </exception>
        public static int ReadInput(this Stream sourceStream, ref sbyte[] target, int start, int count)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            // Returns 0 bytes if not enough space in target
            if (target.Length == 0)
                return 0;

            var receiver = new byte[target.Length];
            var bytesRead = 0;
            var startIndex = start;
            var bytesToRead = count;
            while (bytesToRead > 0)
            {
                var n = sourceStream.Read(receiver, startIndex, bytesToRead);
                if (n == 0)
                    break;
                bytesRead += n;
                startIndex += n;
                bytesToRead -= n;
            }

            // Returns -1 if EOF
            if (bytesRead == 0)
                return -1;

            for (var i = start; i < start + bytesRead; i++)
                target[i] = (sbyte)receiver[i];

            return bytesRead;
        }
    }
}