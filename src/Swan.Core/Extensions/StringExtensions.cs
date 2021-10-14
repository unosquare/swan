namespace Swan.Extensions
{
    using Swan.Formatters;
    using Swan.Reflection;
    using System;
    using System.IO;
    using System.Linq;
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

        private static readonly Lazy<Regex> SplitLinesRegex =
            new(() => new Regex("\r\n|\r|\n", StandardRegexOptions));

        private static readonly Lazy<Regex> UnderscoreRegex =
            new(() => new Regex(@"_", StandardRegexOptions));

        private static readonly Lazy<Regex> CamelCaseRegEx =
            new(() => new Regex(@"[a-z][A-Z]", StandardRegexOptions));

        private static readonly Lazy<MatchEvaluator> SplitCamelCaseString = new(() => m =>
        {
            var x = m.ToString();
            return x[0] + " " + x[1..];
        });

        private static readonly Lazy<char[]> InvalidFilenameChars =
            new(() => Path.GetInvalidFileNameChars().ToArray());

        #endregion

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <param name="this">The item.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string ToStringInvariant(this object? @this)
        {
            return @this switch
            {
                null => string.Empty,
                string stringValue => stringValue,
                _ => @this.GetType().TypeInfo().ToStringInvariant(@this)
            };
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
            => typeof(string) == typeof(T) ? item as string ?? string.Empty : ToStringInvariant(item as object);

        /// <summary>
        /// Removes the control characters from a string except for those specified.
        /// </summary>
        /// <param name="value">The input.</param>
        /// <param name="excludeChars">When specified, these characters will not be removed.</param>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <exception cref="ArgumentNullException">input.</exception>
        public static string RemoveControlChars(this string value, params char[]? excludeChars)
        {
            if (excludeChars is null)
                excludeChars = Array.Empty<char>();

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return new string(value
                .Where(c => !char.IsControl(c) || excludeChars.Contains(c))
                .ToArray());
        }

        /// <summary>
        /// Retrieves a section of the string, inclusive of both, the start and end indexes.
        /// This behavior is unlike JavaScript's Slice behavior where the end index is non-inclusive
        /// If the string is null it returns an empty string.
        /// </summary>
        /// <param name="this">The string.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns>Retrieves a substring from this instance.</returns>
        public static string Slice(this string? @this, int startIndex, int endIndex)
        {
            if (@this == null)
                return string.Empty;

            var end = endIndex.Clamp(startIndex, @this.Length - 1);
            return startIndex >= end ? string.Empty : @this.Substring(startIndex, (end - startIndex) + 1);
        }

        /// <summary>
        /// Gets a part of the string clamping the length and startIndex parameters to safe values.
        /// If the string is null it returns an empty string. This is basically just a safe version
        /// of <see cref="string.Substring(int, int)"/>.
        /// </summary>
        /// <param name="this">The string.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns>Retrieves a substring from this instance.</returns>
        public static string SliceLength(this string? @this, int startIndex, int length)
        {
            if (@this == null)
                return string.Empty;

            var start = startIndex.Clamp(0, @this.Length - 1);
            var len = length.Clamp(0, @this.Length - start);

            return len == 0 ? string.Empty : @this.Substring(start, len);
        }

        /// <summary>
        /// Splits the specified text into r, n or rn separated lines.
        /// </summary>
        /// <param name="this">The text.</param>
        /// <returns>
        /// An array whose elements contain the substrings from this instance 
        /// that are delimited by one or more characters in separator.
        /// </returns>
        public static string[] ToLines(this string? @this) =>
            @this is null ? Array.Empty<string>() : SplitLinesRegex.Value.Split(@this);

        /// <summary>
        /// Humanizes (make more human-readable) an identifier-style string 
        /// in either camel case or snake case. For example, CamelCase will be converted to 
        /// Camel Case and Snake_Case will be converted to Snake Case.
        /// </summary>
        /// <param name="value">The identifier-style string.</param>
        /// <returns>A <see cref="string" /> humanized.</returns>
        public static string Humanize(this string? value)
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
        public static string Humanize(this object value) =>
            value switch
            {
                string stringValue => stringValue.Humanize(),
                bool boolValue => boolValue.Humanize(),
                _ => value.Stringify()
            };

        /// <summary>
        /// Makes the file name system safe.
        /// </summary>
        /// <param name="value">The filename to convert.</param>
        /// <returns>
        /// A string with a safe file name.
        /// </returns>
        /// <exception cref="ArgumentNullException">s.</exception>
        public static string ToSafeFilename(this string? value) =>
            value == null
                ? throw new ArgumentNullException(nameof(value))
                : InvalidFilenameChars.Value
                    .Aggregate(value, (current, c) => current.RemoveChar(c))
                    .Slice(0, 220);

        /// <summary>
        /// Parses a YYYY-MM-DD and optionally, its time part, HH:II:SS into a DateTime.
        /// </summary>
        /// <param name="this">The sortable date.</param>
        /// <returns>
        /// A new instance of the DateTime structure to 
        /// the specified year, month, day, hour, minute and second.
        /// </returns>
        /// <exception cref="ArgumentNullException">sortableDate.</exception>
        /// <exception cref="Exception">
        /// Represents errors that occur during application execution.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Unable to parse sortable date and time. - sortableDate.
        /// </exception>
        public static DateTime ToDateTime(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
                throw new ArgumentNullException(nameof(@this));

            var hour = 0;
            var minute = 0;
            var second = 0;

            var dateTimeParts = @this.Split(' ');

            try
            {
                if (dateTimeParts.Length is not 1 and not 2)
                    throw new FormatException("No date or time could be parsed from the specified string.");

                var dateParts = dateTimeParts[0].Split('-');
                if (dateParts.Length != 3)
                    throw new FormatException("The date part must contain exactly 3 components separated by '-'");

                if (!int.TryParse(dateParts[0], out var year) ||
                    !int.TryParse(dateParts[1], out var month) ||
                    !int.TryParse(dateParts[2], out var day))
                    throw new FormatException("The components of the date part must be valid integers.");

                if (dateTimeParts.Length > 1)
                {
                    var timeParts = dateTimeParts[1].Split(':');
                    if (timeParts.Length != 3)
                        throw new FormatException("The time part must contain exactly 3 components separated by ':'");

                    if (!int.TryParse(timeParts[0], out hour) ||
                        !int.TryParse(timeParts[1], out minute) ||
                        !int.TryParse(timeParts[2], out second))
                        throw new FormatException("The components of the time part must be valid integers.");
                }

                return new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception innerEx)
            {
                throw new ArgumentException($"Unable to parse sortable date and time '{@this}'.", nameof(@this), innerEx);
            }
        }

        /// <summary>
        /// Removes all instances of the given character from a string.
        /// </summary>
        /// <param name="value">The string to be searched.</param>
        /// <param name="find">The character to be removed.</param>
        /// <returns>The newly-formed string without the given char.</returns>
        public static string RemoveChar(this string? value, char find)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var builder = new StringBuilder(value.Length);
            foreach (var c in value.Where(c => c != find))
                builder.Append(c);

            return builder.ToString();
        }

        /// <summary>
        /// Truncates the specified string and appends the omission indicator
        /// while at the same time, guaranteeing that the resulting string
        /// never exceeds the specified maximum length.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximumLength">The maximum character length. If zero or negative, the string is not truncated.</param>
        /// <returns>
        /// Retrieves a substring from this instance.
        /// The substring starts at a specified character position and has a specified length.
        /// </returns>
        public static string? Truncate(this string value, int maximumLength) =>
            Truncate(value, maximumLength, string.Empty);

        /// <summary>
        /// Truncates the specified string and appends the omission indicator
        /// while at the same time, guaranteeing that the resulting string
        /// never exceeds the specified maximum length.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximumLength">The maximum character length. If zero or negative, the string is not truncated.</param>
        /// <param name="omissionIndicator">The a string showing that the string has been truncated, such as an ellipsis.</param>
        /// <returns>
        /// Retrieves a substring from this instance.
        /// The substring starts at a specified character position and has a specified length.
        /// </returns>
        public static string? Truncate(this string? value, int maximumLength, string? omissionIndicator)
        {
            if (value == null)
                return null;

            if (maximumLength <= 0)
                return value;

            var ellipsis = omissionIndicator ?? string.Empty;
            return maximumLength < ellipsis.Length + 1
                ? throw new ArgumentOutOfRangeException(nameof(maximumLength))
                : value.Length > maximumLength
                ? $"{value[..(maximumLength - ellipsis.Length)]}{ellipsis}"
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
        public static bool Contains(this string value, params char[] chars) => chars is not null
            && (chars.Length == 0 || (!string.IsNullOrEmpty(value) && value.IndexOfAny(chars) > -1));

        /// <summary>
        /// Replaces all chars in a string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="replaceValue">The replace value.</param>
        /// <param name="chars">The chars.</param>
        /// <returns>The string with the characters replaced.</returns>
        public static string ReplaceAll(this string value, string replaceValue, params char[] chars) =>
            chars.Aggregate(value, (current, c) => current.Replace(new string(c, 1), replaceValue, StringComparison.Ordinal));

        /// <summary>
        /// Convert hex character to an integer. Return -1 if char is something
        /// other than a hex char.
        /// </summary>
        /// <param name="value">The c.</param>
        /// <returns>Converted integer.</returns>
        public static int Hex2Int(this char value) =>
            value switch
            {
                >= '0' and <= '9' => value - '0',
                >= 'A' and <= 'F' => value - 'A' + 10,
                >= 'a' and <= 'f' => value - 'a' + 10,
                _ => -1
            };
    }
}
