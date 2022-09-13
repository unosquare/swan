namespace Swan.Extensions;

/// <summary>
/// Provides various extension methods for value types and structs.
/// </summary>
public static class ValueTypeExtensions
{
    /// <summary>
    /// Clamps the specified value between the minimum and the maximum.
    /// </summary>
    /// <typeparam name="T">The type of the value being clamped.</typeparam>
    /// <typeparam name="TMin">The type of the minimum value comparison.</typeparam>
    /// <typeparam name="TMax">The type of the maximum value comparison.</typeparam>
    /// <param name="this">The value being clamped.</param>
    /// <param name="min">The minimum clamp value.</param>
    /// <param name="max">The maximum clamp value.</param>
    /// <returns>The value clamped between the minimum and the maximum specified values.</returns>
    public static T Clamp<T, TMin, TMax>(this T @this, TMin min, TMax max)
        where T : struct, IComparable, IConvertible
        where TMin : struct, IConvertible
        where TMax : struct, IConvertible
    {
        var (minValue, maxValue) = AdjustValues<T, TMin, TMax>(min, max);

        if (@this.CompareTo(minValue) < 0)
            return minValue;

        return @this.CompareTo(maxValue) > 0
            ? maxValue
            : @this;
    }

    /// <summary>
    /// Determines whether the specified value is between a minimum and a maximum value.
    /// </summary>
    /// <typeparam name="T">The type of the value being compared.</typeparam>
    /// <typeparam name="TMin">The type of the minimum value comparison.</typeparam>
    /// <typeparam name="TMax">The type of the maximum value comparison.</typeparam>
    /// <param name="this">The value.</param>
    /// <param name="min">The minimum.</param>
    /// <param name="max">The maximum.</param>
    /// <returns>
    ///   <c>true</c> if the specified minimum is between; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsBetween<T, TMin, TMax>(this T @this, TMin min, TMax max)
        where T : struct, IComparable, IConvertible
        where TMin : struct, IConvertible
        where TMax : struct, IConvertible
    {
        var (minValue, maxValue) = AdjustValues<T, TMin, TMax>(min, max);

        return @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Returns a value that is not less than the provided limit.
    /// </summary>
    /// <typeparam name="T">The type of the value being clamped.</typeparam>
    /// <typeparam name="TLimit">The type of the limit value.</typeparam>
    /// <param name="this">The value being tested.</param>
    /// <param name="limit">The limit value.</param>
    /// <returns>A value not less than the provided limit.</returns>
    public static T ClampMin<T, TLimit>(this T @this, TLimit limit)
        where T : struct, IComparable, IConvertible
        where TLimit : struct, IConvertible
    {
        if (limit is not T limitValue)
            limitValue = (T)Convert.ChangeType(limit, typeof(T), CultureInfo.InvariantCulture);

        return @this.CompareTo(limitValue) < 0 ? limitValue : @this;
    }

    /// <summary>
    /// Returns a value that is not greater than the provided limit.
    /// </summary>
    /// <typeparam name="T">The type of the value being clamped.</typeparam>
    /// <typeparam name="TLimit">The type of the limit value.</typeparam>
    /// <param name="this">The value being tested.</param>
    /// <param name="limit">The limit value.</param>
    /// <returns>A value not greater than the provided limit.</returns>
    public static T ClampMax<T, TLimit>(this T @this, TLimit limit)
        where T : struct, IComparable, IConvertible
        where TLimit : struct, IConvertible
    {
        if (limit is not T limitValue)
            limitValue = (T)Convert.ChangeType(limit, typeof(T), CultureInfo.InvariantCulture);

        return @this.CompareTo(limitValue) > 0 ? limitValue : @this;
    }

    private static (T minValue, T maxValue) AdjustValues<T, TMin, TMax>(TMin min, TMax max)
        where T : struct, IComparable, IConvertible where TMin : struct, IConvertible where TMax : struct, IConvertible
    {
        if (min is not T minValue)
            minValue = (T)Convert.ChangeType(min, typeof(T), CultureInfo.InvariantCulture);

        if (max is not T maxValue)
            maxValue = (T)Convert.ChangeType(max, typeof(T), CultureInfo.InvariantCulture);

        return minValue.CompareTo(maxValue) <= 0
            ? (minValue, maxValue)
            : throw new ArgumentOutOfRangeException(nameof(min), minValue,
                "Minimum value has to be less than or equal to maximum value.");
    }
}
