namespace Swan.Extensions;

using System.Globalization;

/// <summary>
/// Provides miscellaneous extension methods.
/// </summary>
public static class ArtifactExtensions
{
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
    /// Converts a <see cref="DateTime"/> to the <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#RFC1123">RFC1123 format</see>.
    /// </summary>
    /// <param name="this">The <see cref="DateTime"/> on which this method is called.</param>
    /// <returns>The string representation of <paramref name="this"/> according to <see href="https://tools.ietf.org/html/rfc1123#page-54">RFC1123</see>.</returns>
    /// <remarks>
    /// <para>If <paramref name="this"/> is not a UTC date / time, its UTC equivalent is converted, leaving <paramref name="this"/> unchanged.</para>
    /// </remarks>
    public static string ToRfc1123String(this DateTime @this)
        => @this.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture);
}
