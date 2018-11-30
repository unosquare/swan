namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides various extension methods for dates.
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
        /// <param name="date">The date.</param>
        /// <returns>The concatenation of date.Year, date.Month and date.Day.</returns>
        public static string ToSortableDate(this DateTime date)
            => $"{date.Year:0000}-{date.Month:00}-{date.Day:00}";

        /// <summary>
        /// Converts the date to a YYYY-MM-DD HH:II:SS string.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>The concatenation of date.Year, date.Month, date.Day, date.Hour, date.Minute and date.Second.</returns>
        public static string ToSortableDateTime(this DateTime date)
            => $"{date.Year:0000}-{date.Month:00}-{date.Day:00} {date.Hour:00}:{date.Minute:00}:{date.Second:00}";

        /// <summary>
        /// Parses a YYYY-MM-DD and optionally it time part, HH:II:SS into a DateTime.
        /// </summary>
        /// <param name="sortableDate">The sortable date.</param>
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
        public static DateTime ToDateTime(this string sortableDate)
        {
            if (string.IsNullOrWhiteSpace(sortableDate))
                throw new ArgumentNullException(nameof(sortableDate));

            var hour = 0;
            var minute = 0;
            var second = 0;

            var dateTimeParts = sortableDate.Split(' ');

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
                throw new ArgumentException("Unable to parse sortable date and time.", nameof(sortableDate));
            }
        }

        /// <summary>
        /// Creates a date's range.
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
        /// <param name="date">The date to convert.</param>
        /// <returns>Seconds since Unix epoch.</returns>
        public static long ToUnixEpochDate(this DateTime date)
        {
#if NETSTANDARD2_0
            return new DateTimeOffset(date).ToUniversalTime().ToUnixTimeSeconds();
#else
            var epochTicks = new DateTime(1970, 1, 1).Ticks;

            return (date.Ticks - epochTicks) / TimeSpan.TicksPerSecond;
#endif
        }

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
        /// <param name="date">The date.</param>
        /// <param name="minute">The minute (0-59).</param>
        /// <param name="hour">The hour. (0-23).</param>
        /// <param name="dayOfMonth">The day of month. (1-31).</param>
        /// <param name="month">The month. (1-12).</param>
        /// <param name="dayOfWeek">The day of week. (0-6)(Sunday = 0).</param>
        /// <returns>Returns <c>true</c> if Months, Days, Hours and Minutes match, otherwise <c>false</c>.</returns>
        public static bool AsCronCanRun(this DateTime date, int? minute = null, int? hour = null, int? dayOfMonth = null, int? month = null, int? dayOfWeek = null)
        {
            var results = new List<bool?>
            {
                GetElementParts(minute, date.Minute),
                GetElementParts(hour, date.Hour),
                GetElementParts(dayOfMonth, date.Day),
                GetElementParts(month, date.Month),
                GetElementParts(dayOfWeek, (int) date.DayOfWeek),
            };

            return results.Any(x => x != false);
        }

        /// <summary>
        /// Compare the Date elements(Months, Days, Hours, Minutes).
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="minute">The minute (0-59).</param>
        /// <param name="hour">The hour. (0-23).</param>
        /// <param name="dayOfMonth">The day of month. (1-31).</param>
        /// <param name="month">The month. (1-12).</param>
        /// <param name="dayOfWeek">The day of week. (0-6)(Sunday = 0).</param>
        /// <returns>Returns <c>true</c> if Months, Days, Hours and Minutes match, otherwise <c>false</c>.</returns>
        public static bool AsCronCanRun(this DateTime date, string minute = "*", string hour = "*", string dayOfMonth = "*", string month = "*", string dayOfWeek = "*")
        {
            var results = new List<bool?>
            {
                GetElementParts(minute, nameof(minute), date.Minute),
                GetElementParts(hour, nameof(hour), date.Hour),
                GetElementParts(dayOfMonth, nameof(dayOfMonth), date.Day),
                GetElementParts(month, nameof(month), date.Month),
                GetElementParts(dayOfWeek, nameof(dayOfWeek), (int) date.DayOfWeek),
            };

            return results.Any(x => x != false);
        }

        private static bool? GetElementParts(int? status, int value) => status.HasValue ? status.Value == value : (bool?) null;

        private static bool? GetElementParts(string parts, string type, int value)
        {
            if (string.IsNullOrWhiteSpace(parts) || parts == "*") return null;
            
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