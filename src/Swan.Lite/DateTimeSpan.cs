using System;

namespace Swan
{
    /// <summary>
    /// Represents a struct of DateTimeSpan to compare dates and get in 
    /// separate fields the amount of time between those dates.
    /// 
    /// Based on https://stackoverflow.com/a/9216404/1096693.
    /// </summary>
    public struct DateTimeSpan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeSpan"/> struct.
        /// </summary>
        /// <param name="years">The years.</param>
        /// <param name="months">The months.</param>
        /// <param name="days">The days.</param>
        /// <param name="hours">The hours.</param>
        /// <param name="minutes">The minutes.</param>
        /// <param name="seconds">The seconds.</param>
        /// <param name="milliseconds">The milliseconds.</param>
        public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            Years = years;
            Months = months;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
        }

        /// <summary>
        /// Gets the years.
        /// </summary>
        /// <value>
        /// The years.
        /// </value>
        public int Years { get; }

        /// <summary>
        /// Gets the months.
        /// </summary>
        /// <value>
        /// The months.
        /// </value>
        public int Months { get; }

        /// <summary>
        /// Gets the days.
        /// </summary>
        /// <value>
        /// The days.
        /// </value>
        public int Days { get; }

        /// <summary>
        /// Gets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public int Hours { get; }

        /// <summary>
        /// Gets the minutes.
        /// </summary>
        /// <value>
        /// The minutes.
        /// </value>
        public int Minutes { get; }

        /// <summary>
        /// Gets the seconds.
        /// </summary>
        /// <value>
        /// The seconds.
        /// </value>
        public int Seconds { get; }

        /// <summary>
        /// Gets the milliseconds.
        /// </summary>
        /// <value>
        /// The milliseconds.
        /// </value>
        public int Milliseconds { get; }
        
        internal static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            var current = date1;
            var years = 0;
            var months = 0;
            var days = 0;

            var phase = Phase.Years;
            var span = new DateTimeSpan();
            var officialDay = current.Day;

            while (phase != Phase.Done)
            {
                switch (phase)
                {
                    case Phase.Years:
                        if (current.AddYears(years + 1) > date2)
                        {
                            phase = Phase.Months;
                            current = current.AddYears(years);
                        }
                        else
                        {
                            years++;
                        }

                        break;
                    case Phase.Months:
                        if (current.AddMonths(months + 1) > date2)
                        {
                            phase = Phase.Days;
                            current = current.AddMonths(months);
                            if (current.Day < officialDay &&
                                officialDay <= DateTime.DaysInMonth(current.Year, current.Month))
                                current = current.AddDays(officialDay - current.Day);
                        }
                        else
                        {
                            months++;
                        }

                        break;
                    case Phase.Days:
                        if (current.AddDays(days + 1) > date2)
                        {
                            current = current.AddDays(days);
                            var timespan = date2 - current;
                            span = new DateTimeSpan(
                                years, 
                                months, 
                                days, 
                                timespan.Hours, 
                                timespan.Minutes,
                                timespan.Seconds, 
                                timespan.Milliseconds);
                            phase = Phase.Done;
                        }
                        else
                        {
                            days++;
                        }

                        break;
                }
            }

            return span;
        }

        private enum Phase
        {
            Years,
            Months,
            Days,
            Done,
        }
    }
}