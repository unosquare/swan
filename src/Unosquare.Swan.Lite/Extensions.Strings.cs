namespace Unosquare.Swan
{
    using Formatters;
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// String related extension methods.
    /// </summary>
    public static class StringExtensions
    {
        #region Private Declarations

        private const RegexOptions StandardRegexOptions =
            RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant;

        private static readonly string[] ByteSuffixes = {"B", "KB", "MB", "GB", "TB"};

        private static readonly Lazy<MD5> Md5Hasher = new Lazy<MD5>(MD5.Create, true);
        private static readonly Lazy<SHA1> SHA1Hasher = new Lazy<SHA1>(SHA1.Create, true);
        private static readonly Lazy<SHA256> SHA256Hasher = new Lazy<SHA256>(SHA256.Create, true);
        private static readonly Lazy<SHA512> SHA512Hasher = new Lazy<SHA512>(SHA512.Create, true);

        private static readonly Lazy<Regex> SplitLinesRegex =
            new Lazy<Regex>(
                () => new Regex("\r\n|\r|\n", StandardRegexOptions));

        private static readonly Lazy<Regex> UnderscoreRegex =
            new Lazy<Regex>(
                () => new Regex(@"_", StandardRegexOptions));

        private static readonly Lazy<Regex> CamelCaseRegEx =
            new Lazy<Regex>(
                () =>
                    new Regex(@"[a-z][A-Z]",
                        StandardRegexOptions));

        private static readonly Lazy<MatchEvaluator> SplitCamelCaseString = new Lazy<MatchEvaluator>(() => m =>
        {
            var x = m.ToString();
            return x[0] + " " + x.Substring(1, x.Length - 1);
        });

        private static readonly Lazy<string[]> InvalidFilenameChars =
            new Lazy<string[]>(() => Path.GetInvalidFileNameChars().Select(c => c.ToString()).ToArray());

        #endregion

        /// <summary>
        /// Computes the MD5 hash of the given stream.
        /// Do not use for large streams as this reads ALL bytes at once.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
        /// <returns>
        /// The computed hash code.
        /// </returns>
        /// <exception cref="ArgumentNullException">stream.</exception>
        [Obsolete("Use a better hasher.")]
        public static byte[] ComputeMD5(this Stream stream, bool createHasher = false)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

#if NET461
            var md5 = MD5.Create();
            const int bufferSize = 4096;

            var readAheadBuffer = new byte[bufferSize];
            var readAheadBytesRead = stream.Read(readAheadBuffer, 0, readAheadBuffer.Length);

            do
            {
                var bytesRead = readAheadBytesRead;
                var buffer = readAheadBuffer;

                readAheadBuffer = new byte[bufferSize];
                readAheadBytesRead = stream.Read(readAheadBuffer, 0, readAheadBuffer.Length);

                if (readAheadBytesRead == 0)
                    md5.TransformFinalBlock(buffer, 0, bytesRead);
                else
                    md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
            }
            while (readAheadBytesRead != 0);

            return md5.Hash;
#else
            using (var ms = new MemoryStream())
            {
                stream.Position = 0;
                stream.CopyTo(ms);

                return (createHasher ? MD5.Create() : Md5Hasher.Value).ComputeHash(ms.ToArray());
            }
#endif
        }

        /// <summary>
        /// Computes the MD5 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
        /// <returns>The computed hash code.</returns>
        [Obsolete("Use a better hasher.")]
        public static byte[] ComputeMD5(this string value, bool createHasher = false) =>
            Encoding.UTF8.GetBytes(value).ComputeMD5(createHasher);

        /// <summary>
        /// Computes the MD5 hash of the given byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
        /// <returns>The computed hash code.</returns>
        [Obsolete("Use a better hasher.")]
        public static byte[] ComputeMD5(this byte[] data, bool createHasher = false) =>
            (createHasher ? MD5.Create() : Md5Hasher.Value).ComputeHash(data);

        /// <summary>
        /// Computes the SHA-1 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
        /// <returns>
        /// The computes a Hash-based Message Authentication Code (HMAC) 
        /// using the SHA1 hash function.
        /// </returns>
        [Obsolete("Use a better hasher.")]
        public static byte[] ComputeSha1(this string value, bool createHasher = false)
        {
            var inputBytes = Encoding.UTF8.GetBytes(value);
            return (createHasher ? SHA1.Create() : SHA1Hasher.Value).ComputeHash(inputBytes);
        }

        /// <summary>
        /// Computes the SHA-256 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
        /// <returns>
        /// The computes a Hash-based Message Authentication Code (HMAC) 
        /// by using the SHA256 hash function.
        /// </returns>
        public static byte[] ComputeSha256(this string value, bool createHasher = false)
        {
            var inputBytes = Encoding.UTF8.GetBytes(value);
            return (createHasher ? SHA256.Create() : SHA256Hasher.Value).ComputeHash(inputBytes);
        }

        /// <summary>
        /// Computes the SHA-512 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="value">The input string.</param>
        /// <param name="createHasher">if set to <c>true</c> [create hasher].</param>
        /// <returns>
        /// The computes a Hash-based Message Authentication Code (HMAC) 
        /// using the SHA512 hash function.
        /// </returns>
        public static byte[] ComputeSha512(this string value, bool createHasher = false)
        {
            var inputBytes = Encoding.UTF8.GetBytes(value);
            return (createHasher ? SHA512.Create() : SHA512Hasher.Value).ComputeHash(inputBytes);
        }

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <param name="obj">The item.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string ToStringInvariant(this object obj)
        {
            if (obj == null)
                return string.Empty;

            var itemType = obj.GetType();

            if (itemType == typeof(string))
                return obj as string;

            return Definitions.BasicTypesInfo.Value.ContainsKey(itemType)
                ? Definitions.BasicTypesInfo.Value[itemType].ToStringInvariant(obj)
                : obj.ToString();
        }

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <typeparam name="T">The type to get the string.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string ToStringInvariant<T>(this T item)
        {
            if (typeof(string) == typeof(T))
                return Equals(item, default(T)) ? string.Empty : item as string;

            return ToStringInvariant(item as object);
        }

        /// <summary>
        /// Removes the control characters from a string except for those specified.
        /// </summary>
        /// <param name="value">The input.</param>
        /// <param name="excludeChars">When specified, these characters will not be removed.</param>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <exception cref="ArgumentNullException">input.</exception>
        public static string RemoveControlCharsExcept(this string value, params char[] excludeChars)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (excludeChars == null)
                excludeChars = Array.Empty<char>();

            return new string(value
                .Where(c => char.IsControl(c) == false || excludeChars.Contains(c))
                .ToArray());
        }

        /// <summary>
        /// Removes all control characters from a string, including new line sequences.
        /// </summary>
        /// <param name="value">The input.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        /// <exception cref="ArgumentNullException">input.</exception>
        public static string RemoveControlChars(this string value) => value.RemoveControlCharsExcept(null);

        /// <summary>
        /// Outputs JSON string representing this object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> format the output.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string ToJson(this object obj, bool format = true) =>
            obj == null ? string.Empty : Json.Serialize(obj, format);

        /// <summary>
        /// Returns text representing the properties of the specified object in a human-readable format.
        /// While this method is fairly expensive computationally speaking, it provides an easy way to
        /// examine objects.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string Stringify(this object obj)
        {
            if (obj == null)
                return "(null)";

            try
            {
                var jsonText = Json.Serialize(obj, false, "$type");
                var jsonData = Json.Deserialize(jsonText);

                return new HumanizeJson(jsonData, 0).GetResult();
            }
            catch
            {
                return obj.ToStringInvariant();
            }
        }

        /// <summary>
        /// Retrieves a section of the string, inclusive of both, the start and end indexes.
        /// This behavior is unlike JavaScript's Slice behavior where the end index is non-inclusive
        /// If the string is null it returns an empty string.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns>Retrieves a substring from this instance.</returns>
        public static string Slice(this string value, int startIndex, int endIndex)
        {
            if (value == null)
                return string.Empty;

            var end = endIndex.Clamp(startIndex, value.Length - 1);

            return startIndex >= end ? string.Empty : value.Substring(startIndex, (end - startIndex) + 1);
        }

        /// <summary>
        /// Gets a part of the string clamping the length and startIndex parameters to safe values.
        /// If the string is null it returns an empty string. This is basically just a safe version
        /// of string.Substring.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns>Retrieves a substring from this instance.</returns>
        public static string SliceLength(this string str, int startIndex, int length)
        {
            if (str == null)
                return string.Empty;

            var start = startIndex.Clamp(0, str.Length - 1);
            var len = length.Clamp(0, str.Length - start);

            return len == 0 ? string.Empty : str.Substring(start, len);
        }

        /// <summary>
        /// Splits the specified text into r, n or rn separated lines.
        /// </summary>
        /// <param name="value">The text.</param>
        /// <returns>
        /// An array whose elements contain the substrings from this instance 
        /// that are delimited by one or more characters in separator.
        /// </returns>
        public static string[] ToLines(this string value) =>
            value == null ? Array.Empty<string>() : SplitLinesRegex.Value.Split(value);

        /// <summary>
        /// Humanizes (make more human-readable) an identifier-style string 
        /// in either camel case or snake case. For example, CamelCase will be converted to 
        /// Camel Case and Snake_Case will be converted to Snake Case.
        /// </summary>
        /// <param name="value">The identifier-style string.</param>
        /// <returns>A <see cref="string" /> humanized.</returns>
        public static string Humanize(this string value)
        {
            if (value == null)
                return string.Empty;

            var returnValue = UnderscoreRegex.Value.Replace(value, " ");
            returnValue = CamelCaseRegEx.Value.Replace(returnValue, SplitCamelCaseString.Value);
            return returnValue;
        }

        /// <summary>
        /// Humanizes (make more human-readable) an boolean.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns>A <see cref="string" /> that represents the current boolean.</returns>
        public static string Humanize(this bool value) => value ? "Yes" : "No";

        /// <summary>
        /// Humanizes (make more human-readable) the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string Humanize(this object value)
        {
            switch (value)
            {
                case string stringValue:
                    return stringValue.Humanize();
                case bool boolValue:
                    return boolValue.Humanize();
                default:
                    return value.Stringify();
            }
        }

        /// <summary>
        /// Indents the specified multi-line text with the given amount of leading spaces
        /// per line.
        /// </summary>
        /// <param name="value">The text.</param>
        /// <param name="spaces">The spaces.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string Indent(this string value, int spaces = 4)
        {
            if (value == null) value = string.Empty;
            if (spaces <= 0) return value;

            var lines = value.ToLines();
            var builder = new StringBuilder();
            var indentStr = new string(' ', spaces);

            foreach (var line in lines)
            {
                builder.AppendLine($"{indentStr}{line}");
            }

            return builder.ToString().TrimEnd();
        }

        /// <summary>
        /// Gets the line and column number (i.e. not index) of the
        /// specified character index. Useful to locate text in a multi-line
        /// string the same way a text editor does.
        /// Please not that the tuple contains first the line number and then the
        /// column number.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="charIndex">Index of the character.</param>
        /// <returns>A 2-tuple whose value is (item1, item2).</returns>
        public static Tuple<int, int> TextPositionAt(this string value, int charIndex)
        {
            if (value == null)
                return Tuple.Create(0, 0);

            var index = charIndex.Clamp(0, value.Length - 1);

            var lineIndex = 0;
            var colNumber = 0;

            for (var i = 0; i <= index; i++)
            {
                if (value[i] == '\n')
                {
                    lineIndex++;
                    colNumber = 0;
                    continue;
                }

                if (value[i] != '\r')
                    colNumber++;
            }

            return Tuple.Create(lineIndex + 1, colNumber);
        }

        /// <summary>
        /// Makes the file name system safe.
        /// </summary>
        /// <param name="value">The s.</param>
        /// <returns>
        /// A string with a safe file name.
        /// </returns>
        /// <exception cref="ArgumentNullException">s.</exception>
        public static string ToSafeFilename(this string value)
        {
            return value == null
                ? throw new ArgumentNullException(nameof(value))
                : InvalidFilenameChars.Value
                    .Aggregate(value, (current, c) => current.Replace(c, string.Empty))
                    .Slice(0, 220);
        }

        /// <summary>
        /// Formats a long into the closest bytes string.
        /// </summary>
        /// <param name="bytes">The bytes length.</param>
        /// <returns>
        /// The string representation of the current Byte object, formatted as specified by the format parameter.
        /// </returns>
        public static string FormatBytes(this long bytes) => ((ulong) bytes).FormatBytes();

        /// <summary>
        /// Formats a long into the closest bytes string.
        /// </summary>
        /// <param name="bytes">The bytes length.</param>
        /// <returns>
        /// A copy of format in which the format items have been replaced by the string 
        /// representations of the corresponding arguments.
        /// </returns>
        public static string FormatBytes(this ulong bytes)
        {
            int i;
            double dblSByte = bytes;

            for (i = 0; i < ByteSuffixes.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return $"{dblSByte:0.##} {ByteSuffixes[i]}";
        }

        /// <summary>
        /// Truncates the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximumLength">The maximum length.</param>
        /// <returns>
        /// Retrieves a substring from this instance.
        /// The substring starts at a specified character position and has a specified length.
        /// </returns>
        public static string Truncate(this string value, int maximumLength) =>
            Truncate(value, maximumLength, string.Empty);

        /// <summary>
        /// Truncates the specified value and append the omission last.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximumLength">The maximum length.</param>
        /// <param name="omission">The omission.</param>
        /// <returns>
        /// Retrieves a substring from this instance.
        /// The substring starts at a specified character position and has a specified length.
        /// </returns>
        public static string Truncate(this string value, int maximumLength, string omission)
        {
            if (value == null)
                return null;

            return value.Length > maximumLength
                ? value.Substring(0, maximumLength) + (omission ?? string.Empty)
                : value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="string"/> contains any of characters in
        /// the specified array of <see cref="char"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> contains any of <paramref name="chars"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <param name="value">
        /// A <see cref="string"/> to test.
        /// </param>
        /// <param name="chars">
        /// An array of <see cref="char"/> that contains characters to find.
        /// </param>
        public static bool Contains(this string value, params char[] chars) =>
            chars?.Length == 0 || (!string.IsNullOrEmpty(value) && value.IndexOfAny(chars) > -1);

        /// <summary>
        /// Replaces all chars in a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="replaceValue">The replace value.</param>
        /// <param name="chars">The chars.</param>
        /// <returns>The string with the characters replaced.</returns>
        public static string ReplaceAll(this string value, string replaceValue, params char[] chars) =>
            chars.Aggregate(value, (current, c) => current.Replace(new string(new[] {c}), replaceValue));

        /// <summary>
        /// Convert hex character to an integer. Return -1 if char is something
        /// other than a hex char.
        /// </summary>
        /// <param name="value">The c.</param>
        /// <returns>Converted integer.</returns>
        public static int Hex2Int(this char value)
        {
            return value >= '0' && value <= '9'
                ? value - '0'
                : value >= 'A' && value <= 'F'
                    ? value - 'A' + 10
                    : value >= 'a' && value <= 'f'
                        ? value - 'a' + 10
                        : -1;
        }
    }
}