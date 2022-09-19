namespace Swan.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DateTime"/>.
/// </summary>
public static class DateTimeExtensions
{
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
    /// Creates a range of <see cref="DateTime"/> values in one-day increments.
    /// The time component is stripped off the range values.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>
    /// A sequence of  within a specified date's range.
    /// </returns>
    public static IEnumerable<DateTime> DateRange(this DateTime startDate, DateTime endDate)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;
        var daysDifference = (endDate - startDate).Days;

        for (var i = 0; i <= daysDifference; i++)
            yield return startDate.AddDays(i);
    }
}
