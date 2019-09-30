using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Swan
{
    /// <summary>
    /// Provides extension methods for <see cref="DateTime"/>.
    /// </summary>
    public static class DateExtensions
    {
        private static readonly Dictionary<string, int> DateRanges = new Dictionary<string, int>()
        {
            { "minute", 59},
            { "hour", 23},
            { "dayOfMonth", 31},
            { "month", 12},
            { "dayOfWeek", 6},
        };

        /// <summary>
        /// Converts the date to a YYYY-MM-DD string.
        /// </summary>
        /// <param name="this">The <see cref="DateTime"/> on which this method is called.</param>
        /// <returns>The concatenation of date.Year, date.Month and date.Day.</returns>
        public static string ToSortableDate(this DateTime @this)
            => $"{@this.Year:0000}-{@this.Month:00}-{@this.Day:00}";

        /// <summary>
        /// Converts the date to a YYYY-MM-DD HH:II:SS string.
        /// </summary>
        /// <param name="this">The <see cref="DateTime"/> on which this method is called.</param>
        /// <returns>The concatenation of date.Year, date.Month, date.Day, date.Hour, date.Minute and date.Second.</returns>
        public static string ToSortableDateTime(this DateTime @this)
            => $"{@this.Year:0000}-{@this.Month:00}-{@this.Day:00} {@this.Hour:00}:{@this.Minute:00}:{@this.Second:00}";

        /// <summary>
        /// Parses a YYYY-MM-DD and optionally it time part, HH:II:SS into a DateTime.
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
                if (dateTimeParts.Length != 1 && dateTimeParts.Length != 2)
                    throw new Exception();

                var dateParts = dateTimeParts[0].Split('-');
                if (dateParts.Length != 3) throw new Exception();

                var year = int.Parse(dateParts[0]);
                var month = int.Parse(dateParts[1]);
                var day = int.Parse(dateParts[2]);

                if (dateTimeParts.Length > 1)
                {
                    var timeParts = dateTimeParts[1].Split(':');
                    if (timeParts.Length != 3) throw new Exception();

                    hour = int.Parse(timeParts[0]);
                    minute = int.Parse(timeParts[1]);
                    second = int.Parse(timeParts[2]);
                }

                return new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception)
            {
                throw new ArgumentException("Unable to parse sortable date and time.", nameof(@this));
            }
        }

        /// <summary>
        /// Creates a date range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>
        /// A sequence of integral numbers within a specified date's range.
        /// </returns>
        public static IEnumerable<DateTime> DateRange(this DateTime startDate, DateTime endDate)
            => Enumerable.Range(0, (endDate - startDate).Days + 1).Select(d => startDate.AddDays(d));

        /// <summary>
        /// Rounds up a date to match a timespan.
        /// </summary>
        /// <param name="date">The datetime.</param>
        /// <param name="timeSpan">The timespan to match.</param>
        /// <returns>
        /// A new instance of the DateTime structure to the specified datetime and timespan ticks.
        /// </returns>
        public static DateTime RoundUp(this DateTime date, TimeSpan timeSpan)
            => new DateTime(((date.Ticks + timeSpan.Ticks - 1) / timeSpan.Ticks) * timeSpan.Ticks);

        /// <summary>
        /// Get this datetime as a Unix epoch timestamp (seconds since Jan 1, 1970, midnight UTC).
        /// </summary>
        /// <param name="this">The <see cref="DateTime"/> on which this method is called.</param>
        /// <returns>Seconds since Unix epoch.</returns>
        public static long ToUnixEpochDate(this DateTime @this) => new DateTimeOffset(@this).ToUniversalTime().ToUnixTimeSeconds();

        /// <summary>
        /// Compares a Date to another and returns a <c>DateTimeSpan</c>.
        /// </summary>
        /// <param name="dateStart">The date start.</param>
        /// <param name="dateEnd">The date end.</param>
        /// <returns>A DateTimeSpan with the Years, Months, Days, Hours, Minutes, Seconds and Milliseconds between the dates.</returns>
        public static DateTimeSpan GetDateTimeSpan(this DateTime dateStart, DateTime dateEnd)
            => DateTimeSpan.CompareDates(dateStart, dateEnd);

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

        private static bool? GetElementParts(int? status, int value) => status.HasValue ? status.Value == value : (bool?) null;

        private static bool? GetElementParts(string parts, string type, int value)
        {
            if (string.IsNullOrWhiteSpace(parts) || parts == "*")
                return null;
            
            if (parts.Contains(","))
            {
                return parts.Split(',').Select(int.Parse).Contains(value);
            }

            var stop = DateRanges[type];

            if (parts.Contains("/"))
            {
                var multiple = int.Parse(parts.Split('/').Last());
                var start = type == "dayOfMonth" || type == "month" ? 1 : 0;
                
                for (var i = start; i <= stop; i += multiple)
                    if (i == value) return true;

                return false;
            }

            if (parts.Contains("-"))
            {
                var range = parts.Split('-');
                var start = int.Parse(range.First());
                stop = Math.Max(stop, int.Parse(range.Last()));

                if ((type == "dayOfMonth" || type == "month") && start == 0)
                    start = 1;

                for (var i = start; i <= stop; i++)
                    if (i == value) return true;

                return false;
            }
            
            return int.Parse(parts) == value;
        }
    }
}
