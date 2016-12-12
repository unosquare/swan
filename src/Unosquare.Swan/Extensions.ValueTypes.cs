namespace Unosquare.Swan
{
    using System;

    /// <summary>
    /// Provides various extension methods
    /// </summary>
    partial class Extensions
    {
        /// <summary>
        /// Clamps the specified value between the minimum and the maximum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public static T Clamp<T>(this T value, T min, T max)
            where T : struct, IComparable
        {
            if (value.CompareTo(min) < 0) return min;

            return value.CompareTo(max) > 0 ? max : value;
        }

        /// <summary>
        /// Determines whether the specified value is between a minimum and a maximum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>
        ///   <c>true</c> if the specified minimum is between; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBetween<T>(this T value, T min, T max)
            where T : struct, IComparable
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                return false;

            return true;
        }

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

    }
}
