using System.IO;
using System.Reflection;

namespace Unosquare.Swan
{
    using Formatters;
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    partial class Extensions
    {
        private const RegexOptions StandardRegexOptions =
            RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant;

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

        private static readonly Lazy<MatchEvaluator> SplitCamelCaseString = new Lazy<MatchEvaluator>(() =>
        {
            return ((m) =>
            {
                var x = m.ToString();
                return x[0] + " " + x.Substring(1, x.Length - 1);
            });
        });

        /// <summary>
        /// Computes the MD5 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static byte[] ComputeMD5(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString);
            return Md5Hasher.Value.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Computes the SHA-1 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static byte[] ComputeSha1(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString);
            return SHA1Hasher.Value.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Computes the SHA-256 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static byte[] ComputeSha256(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString);
            return SHA256Hasher.Value.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Computes the SHA-512 hash of the given string using UTF8 byte encoding.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static byte[] ComputeSha512(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString);
            return SHA512Hasher.Value.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static string ToStringInvariant(this object item)
        {
            if (item == null)
                return string.Empty;

            var itemType = item.GetType();

            if (itemType == typeof(string))
                return item as string;

            return Constants.BasicTypesInfo.ContainsKey(itemType)
                ? Constants.BasicTypesInfo[itemType].ToStringInvariant(item)
                : item.ToString();
        }

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static string ToStringInvariant<T>(this T item)
        {
            if (typeof(string) == typeof(T))
                return item == null ? string.Empty : item as string;

            return ToStringInvariant(item as object);
        }

        /// <summary>
        /// Removes the control characters from a string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string RemoveControlChars(this string input)
        {
            return new string(input.Where(c => !char.IsControl(c)).ToArray());
        }


        /// <summary>
        /// Outputs formatted JSON representing this object
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string Jsonify(this object obj)
        {
            return Json.Serialize(obj, true);
        }

        /// <summary>
        /// Retrieves a section of the string includive of the start and end indexes.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns></returns>
        public static string Section(this string str, int startIndex, int endIndex)
        {
            endIndex = endIndex.Clamp(startIndex, str.Length - 1);
            if (startIndex >= endIndex) return string.Empty;
            return str.Substring(startIndex, (endIndex - startIndex) + 1);
        }

        /// <summary>
        /// Gets a part of the string clamping the length and startIndex parameters to safe values.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string SafeSubstring(this string str, int startIndex, int length)
        {
            startIndex = startIndex.Clamp(0, str.Length - 1);
            length = length.Clamp(0, str.Length - startIndex);

            if (length == 0) return string.Empty;
            return str.Substring(startIndex, length);
        }

        /// <summary>
        /// Converts a set of hexadecimal characters (upercase or lowervase)
        /// to a byte array. String length must be a multiple of 2 and 
        /// any prefix (such as 0x) has to be avoided for this to work properly
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        public static byte[] HexToBytes(this string hex)
        {
            return Enumerable
                .Range(0, hex.Length / 2)
                .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// Splits the specified text into r, n or rn separated lines
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string[] ToLines(this string text)
        {
            if (text == null) return new string[] { };
            return SplitLinesRegex.Value.Split(text);
        }

        /// <summary>
        /// Humanizes (make more human-readable) an identifier-style string 
        /// in either camel case or snake case. For example, CamelCase will be converted to 
        /// Camel Case and Snake_Case will be converted to Snake Case.
        /// </summary>
        /// <param name="identifierString">The identifier-style string.</param>
        /// <returns></returns>
        public static string Humanize(this string identifierString)
        {
            var returnValue = identifierString ?? string.Empty;
            returnValue = UnderscoreRegex.Value.Replace(returnValue, " ");
            returnValue = CamelCaseRegEx.Value.Replace(returnValue, SplitCamelCaseString.Value);
            return returnValue;
        }

        /// <summary>
        /// Indents the specified multi-line text with the given amount of spaces.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="spaces">The spaces.</param>
        /// <returns></returns>
        public static string Indent(this string text, int spaces = 4)
        {
            if (text == null) text = string.Empty;
            if (spaces <= 0) return text;

            var lines = text.ToLines();
            var builder = new StringBuilder();
            var indentStr = new string(' ', spaces);
            foreach (var line in lines)
            {
                builder.AppendLine($"{indentStr}{line}");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Humanizes the specified exception object so it is readable
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public static string Humanize(this Exception ex)
        {
            // TODO: JSON is NOT too useful here. We need cutom humanization logic for exceptions
            if (ex == null) return string.Empty;
            var builder = new StringBuilder();
            builder.AppendLine($"{ex.GetType()}: {ex.Message}");
            var jsonData = Json.SerializeExcluding(ex, true, nameof(Exception.Data), nameof(Exception.Source), "TargetSite");
            builder.AppendLine(jsonData);
            return builder.ToString();
        }

        /// <summary>
        /// To the one character string.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
        public static string ToOneCharString(this char c)
        {
            return new string(c, 1);
        }

        // TODO: Test Humanize, Add pluralize, singularize, dashize, capitalize titleize, etc.
    }
}