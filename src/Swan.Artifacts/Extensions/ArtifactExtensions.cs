namespace Swan.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Provides miscellaneous extension methods.
    /// </summary>
    public static class ArtifactExtensions
    {
        private static readonly Dictionary<string, int> DateRanges = new()
        {
            { "minute", 59 },
            { "hour", 23 },
            { "dayOfMonth", 31 },
            { "month", 12 },
            { "dayOfWeek", 6 },
        };

        /// <summary>
        /// Indents the specified multi-line text with the given amount of leading spaces
        /// per line.
        /// </summary>
        /// <param name="value">The text.</param>
        /// <param name="spaces">The spaces.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string Indent(this string? value, int spaces = 4)
        {
            value ??= string.Empty;
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
        public static Tuple<int, int> TextPositionAt(this string? value, int charIndex)
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
        /// Rounds up a date to match a timespan.
        /// </summary>
        /// <param name="date">The datetime.</param>
        /// <param name="timeSpan">The timespan to match.</param>
        /// <returns>
        /// A new instance of the DateTime structure to the specified datetime and timespan ticks.
        /// </returns>
        public static DateTime RoundUp(this DateTime date, TimeSpan timeSpan)
            => new(((date.Ticks + timeSpan.Ticks - 1) / timeSpan.Ticks) * timeSpan.Ticks);

        /// <summary>
        /// Compare the Date elements(Months, Days, Hours, Minutes).
        /// </summary>
        /// <param name="this">The <see cref="DateTime"/> on which this method is called.</param>
        /// <param name="minute">The minute (0-59).</param>
        /// <param name="hour">The hour. (0-23).</param>
        /// <param name="dayOfMonth">The day of month. (1-31).</param>
        /// <param name="month">The month. (1-12).</param>
        /// <param name="dayOfWeek">The day of week. (0-6)(Sunday = 0).</param>
        /// <returns>Returns <c>true</c> if Months, Days, Hours and Minutes match, otherwise <c>false</c>.</returns>
        public static bool AsCronCanRun(this DateTime @this, int? minute = null, int? hour = null, int? dayOfMonth = null, int? month = null, int? dayOfWeek = null)
        {
            var results = new List<bool?>
            {
                GetElementParts(minute, @this.Minute),
                GetElementParts(hour, @this.Hour),
                GetElementParts(dayOfMonth, @this.Day),
                GetElementParts(month, @this.Month),
                GetElementParts(dayOfWeek, (int) @this.DayOfWeek),
            };

            return results.Any(x => x != false);
        }

        /// <summary>
        /// Compare the Date elements(Months, Days, Hours, Minutes).
        /// </summary>
        /// <param name="this">The <see cref="DateTime"/> on which this method is called.</param>
        /// <param name="minute">The minute (0-59).</param>
        /// <param name="hour">The hour. (0-23).</param>
        /// <param name="dayOfMonth">The day of month. (1-31).</param>
        /// <param name="month">The month. (1-12).</param>
        /// <param name="dayOfWeek">The day of week. (0-6)(Sunday = 0).</param>
        /// <returns>Returns <c>true</c> if Months, Days, Hours and Minutes match, otherwise <c>false</c>.</returns>
        public static bool AsCronCanRun(this DateTime @this, string minute = "*", string hour = "*", string dayOfMonth = "*", string month = "*", string dayOfWeek = "*")
        {
            var results = new List<bool?>
            {
                GetElementParts(minute, nameof(minute), @this.Minute),
                GetElementParts(hour, nameof(hour), @this.Hour),
                GetElementParts(dayOfMonth, nameof(dayOfMonth), @this.Day),
                GetElementParts(month, nameof(month), @this.Month),
                GetElementParts(dayOfWeek, nameof(dayOfWeek), (int) @this.DayOfWeek),
            };

            return results.Any(x => x != false);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to the <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#RFC1123">RFC1123 format</see>.
        /// </summary>
        /// <param name="this">The <see cref="DateTime"/> on which this method is called.</param>
        /// <returns>The string representation of <paramref name="this"/> according to <see href="https://tools.ietf.org/html/rfc1123#page-54">RFC1123</see>.</returns>
        /// <remarks>
        /// <para>If <paramref name="this"/> is not a UTC date / time, its UTC equivalent is converted, leaving <paramref name="this"/> unchanged.</para>
        /// </remarks>
        public static string ToRfc1123String(this DateTime @this)
            => @this.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture);

        private static bool? GetElementParts(int? status, int value) => status.HasValue ? status.Value == value : null;

        private static bool? GetElementParts(string parts, string type, int value)
        {
            if (string.IsNullOrWhiteSpace(parts) || parts == "*")
                return null;

            if (parts.Contains(',', StringComparison.Ordinal))
            {
                return parts.Split(',').Select(int.Parse).Contains(value);
            }

            var stop = DateRanges[type];

            if (parts.Contains('/', StringComparison.Ordinal))
            {
                var multiple = int.Parse(parts.Split('/').Last(), CultureInfo.InvariantCulture);
                var start = type is "dayOfMonth" or "month" ? 1 : 0;

                for (var i = start; i <= stop; i += multiple)
                    if (i == value) return true;

                return false;
            }

            if (parts.Contains('-', StringComparison.Ordinal))
            {
                var range = parts.Split('-');
                var start = int.Parse(range.First(), CultureInfo.InvariantCulture);
                stop = Math.Max(stop, int.Parse(range.Last(), CultureInfo.InvariantCulture));

                if ((type is "dayOfMonth" or "month") && start == 0)
                    start = 1;

                for (var i = start; i <= stop; i++)
                    if (i == value) return true;

                return false;
            }

            return int.Parse(parts, CultureInfo.InvariantCulture) == value;
        }
    }
}
