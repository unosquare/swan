namespace Swan.Gizmos;

/// <summary>
/// Represents a struct of DateTimeSpan to compare dates and get in 
/// separate fields the amount of time between those dates.
/// This code is based on https://stackoverflow.com/a/9216404/1096693.
/// </summary>
public record struct DateTimeSpan(int Years, int Months, int Days, int Hours, int Minutes, int Seconds, int Milliseconds)
{
    /// <summary>
    /// Creates a <see cref="DateTimeSpan"/> from 2 dates.
    /// </summary>
    /// <param name="date1">The first date to compare.</param>
    /// <param name="date2">The second date to compare.</param>
    /// <returns>A span between the 2 dates.</returns>
    public static DateTimeSpan FromDates(DateTime date1, DateTime date2)
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
        var initialDay = current.Day;

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
                        if (current.Day < initialDay &&
                            initialDay <= DateTime.DaysInMonth(current.Year, current.Month))
                            current = current.AddDays(initialDay - current.Day);
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

    /// <inheritdoc />
    public override string ToString() =>
        $"{Years} years, {Months} months, {Days} days, {Hours}:{Minutes}:{Seconds}.{Milliseconds}";

    private enum Phase
    {
        Years,
        Months,
        Days,
        Done,
    }
}
