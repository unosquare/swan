namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    partial class Extensions
    {
        /// <summary>
        /// Converts the date to a YYYY-MM-DD string
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static string ToSortableDate(this DateTime date)
        {
            return $"{date.Year:0000}-{date.Month:00}-{date.Day:00}";
        }

        /// <summary>
        /// Converts the date to a YYYY-MM-DD HH:II:SS string
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static string ToSortableDateTime(this DateTime date)
        {
            return $"{date.Year:0000}-{date.Month:00}-{date.Day:00} {date.Hour:00}:{date.Minute:00}:{date.Second:00}";
        }

        /// <summary>
        /// Parses a YYYY-MM-DD and optionally it time part, HH:II:SS into a DateTime
        /// </summary>
        /// <param name="sortableDate">The sortable date.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">sortableDate</exception>
        /// <exception cref="Exception">
        /// </exception>
        /// <exception cref="ArgumentException">Unable to parse sortable date and time. - sortableDate</exception>
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
        /// <returns></returns>
        public static IEnumerable<DateTime> DateRange(this DateTime startDate, DateTime endDate)
        {
            return Enumerable.Range(0, (endDate - startDate).Days + 1).Select(d => startDate.AddDays(d));
        }
        
        /// <summary>
        /// Rounds up a date to match a timespan.
        /// </summary>
        /// <param name="dt">The datetime.</param>
        /// <param name="d">The timespan to match.</param>
        /// <returns></returns>
        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }
    }
}
