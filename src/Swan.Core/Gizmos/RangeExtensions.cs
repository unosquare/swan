namespace Swan.Gizmos;

using System;
using System.Collections.Generic;
using System.Linq;

internal static class RangeExtensions
{
    /// <summary>
    /// Given a set of values representing a discretized continuum, this method
    /// maps the provided search value to the closest point that is equal or less than
    /// such value. Please note that the values are first sorted in ascending order.
    /// For example, given 100, 110 and 120:
    /// searching for 90 would yield 100,
    /// searching for 105 would yield 100,
    /// searching for 110 would yield 110,
    /// searching for 119 would yield 110,
    /// searching for 200 would yield 120
    /// </summary>
    /// <typeparam name="T">The type of the values to search for.</typeparam>
    /// <param name="allValues">The values representing the discretized continuum.</param>
    /// <param name="searchValue">The value to be searched.</param>
    /// <returns>A discrete value to which the search value belongs.</returns>
    public static T? FindStartValue<T>(this IEnumerable<T>? allValues, T searchValue)
        where T : IComparable, IComparable<T>
    {
        if (allValues is null)
            return default;

        var sortedValues = allValues.ToArray();
        if (!sortedValues.Any())
            return default;

        Array.Sort(sortedValues);

        var firstValue = sortedValues[0];
        if (searchValue.CompareTo(firstValue) < 0)
            return firstValue;

        var currentValue = firstValue;
        foreach (var value in sortedValues)
        {
            if (searchValue.CompareTo(value) >= 0)
            {
                currentValue = value;
                continue;
            }

            break;
        }

        return currentValue;
    }

    /// <summary>
    /// Provides a continuum mapping method similar to <see cref="FindStartValue{T}(IEnumerable{T}, T)"/>
    /// but operates on a <see cref="SortedList{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of keys to be searched.</typeparam>
    /// <typeparam name="TValue">The type of the values to return.</typeparam>
    /// <param name="values">The sorted list to be searched by key.</param>
    /// <param name="searchKey">The key to be searched.</param>
    /// <returns>The value of the matching point in the continuum.</returns>
    public static TValue? FindStartValue<TKey, TValue>(this SortedList<TKey, TValue>? values, TKey searchKey)
        where TKey : struct, IComparable, IComparable<TKey>
    {
        if (values is null || values.Count <= 0)
            return default;

        var (firstKey, firstValue) = values.First();
        if (searchKey.CompareTo(firstKey) < 0)
            return firstValue;

        var currentValue = firstValue;
        foreach (var (key, value) in values)
        {
            if (searchKey.CompareTo(key) >= 0)
            {
                currentValue = value;
                continue;
            }

            break;
        }

        return currentValue;
    }
}
