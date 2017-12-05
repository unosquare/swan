namespace Unosquare.Swan
{
    using System;

    /// <summary>
    /// Base on https://stackoverflow.com/a/9216404/1096693
    /// </summary>
    public struct DateTimeSpan
    {
        private readonly int years;
        private readonly int months;
        private readonly int days;
        private readonly int hours;
        private readonly int minutes;
        private readonly int seconds;
        private readonly int milliseconds;

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
            this.years = years;
            this.months = months;
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.milliseconds = milliseconds;
        }

        /// <summary>
        /// Gets the years.
        /// </summary>
        /// <value>
        /// The years.
        /// </value>
        public int Years => years;

        /// <summary>
        /// Gets the months.
        /// </summary>
        /// <value>
        /// The months.
        /// </value>
        public int Months => months;

        /// <summary>
        /// Gets the days.
        /// </summary>
        /// <value>
        /// The days.
        /// </value>
        public int Days => days;

        /// <summary>
        /// Gets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public int Hours => hours;

        /// <summary>
        /// Gets the minutes.
        /// </summary>
        /// <value>
        /// The minutes.
        /// </value>
        public int Minutes => minutes;

        /// <summary>
        /// Gets the seconds.
        /// </summary>
        /// <value>
        /// The seconds.
        /// </value>
        public int Seconds => seconds;

        /// <summary>
        /// Gets the milliseconds.
        /// </summary>
        /// <value>
        /// The milliseconds.
        /// </value>
        public int Milliseconds => milliseconds;

        /// <summary>
        /// 
        /// </summary>
        enum Phase { Years, Months, Days, Done }

        /// <summary>
        /// Compares the dates.
        /// </summary>
        /// <param name="date1">The date1.</param>
        /// <param name="date2">The date2.</param>
        /// <returns></returns>
        public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
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
                            if (current.Day < officialDay && officialDay <= DateTime.DaysInMonth(current.Year, current.Month))
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
                            span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
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
    }
}
