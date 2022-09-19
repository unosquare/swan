namespace Swan.Collections;

/// <summary>
/// Provides functional programming extension methods for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Given an enumerable set of values, find the value that gets repeated the most.
    /// </summary>
    /// <typeparam name="T">The structure type.</typeparam>
    /// <param name="values">The values to find.</param>
    /// <returns>The most common value of the type.</returns>
    /// <exception cref="ArgumentNullException">The set cannot be be null.</exception>
    public static T FindMostCommonValue<T>(this IEnumerable<T> values)
        where T : struct
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        var repeatDictionary = new Dictionary<T, int>();
        foreach (var v in values)
        {
            if (repeatDictionary.ContainsKey(v))
                repeatDictionary[v] += 1;
            else
                repeatDictionary.Add(v, 1);
        }

        var maxCount = repeatDictionary.Values.Max();
        return repeatDictionary.First(kvp => kvp.Value >= maxCount).Key;
    }

    /// <summary>
    /// Filters the collection only when the provided condition
    /// evaluates to true. If the condition evaluates to false,
    /// this method simply returns the original collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection to filter.</typeparam>
    /// <param name="sourceCollection">The collection to filter.</param>
    /// <param name="conditionFunction">The condition evaluation function.</param>
    /// <param name="filteringFunction">The filtering function that returns a new collection.</param>
    /// <returns>
    /// The filtered or unfiltered collection.
    /// </returns>
    public static IQueryable<T> When<T>(this IQueryable<T> sourceCollection,
        Func<bool> conditionFunction,
        Func<IQueryable<T>, IQueryable<T>> filteringFunction)
    {
        if (sourceCollection is null)
            throw new ArgumentNullException(nameof(sourceCollection));

        return conditionFunction is null
            ? throw new ArgumentNullException(nameof(conditionFunction))
            : filteringFunction is null
                ? throw new ArgumentNullException(nameof(filteringFunction))
                : conditionFunction()
                    ? filteringFunction(sourceCollection)
                    : sourceCollection;
    }

    /// <summary>
    /// Filters the collection only when the provided condition
    /// evaluates to true. If the condition evaluates to false,
    /// this method simply returns the original collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection to filter.</typeparam>
    /// <param name="sourceCollection">The collection to filter.</param>
    /// <param name="conditionFunction">The condition evaluation function.</param>
    /// <param name="filteringFunction">The filtering function that returns a new collection.</param>
    /// <returns>
    /// The filtered or unfiltered collection.
    /// </returns>
    public static IEnumerable<T> When<T>(this IEnumerable<T> sourceCollection,
        Func<bool> conditionFunction,
        Func<IEnumerable<T>, IEnumerable<T>> filteringFunction)
    {
        if (sourceCollection is null)
            throw new ArgumentNullException(nameof(sourceCollection));

        if (conditionFunction is null)
            throw new ArgumentNullException(nameof(conditionFunction));

        return filteringFunction is null
            ? throw new ArgumentNullException(nameof(filteringFunction))
            : conditionFunction()
                ? filteringFunction(sourceCollection)
                : sourceCollection;
    }

    /// <summary>
    /// Adds a value to the collection when the provided condition function returns true.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="targetCollection">The collection to add the value to.</param>
    /// <param name="conditionFunction">The condition evaluation function.</param>
    /// <param name="valueFactory">The value factory method.</param>
    /// <returns>
    /// The same collection, as to enable fluent API calls.
    /// </returns>
    public static IList<T> AddWhen<T>(this IList<T> targetCollection,
        Func<bool> conditionFunction,
        Func<T> valueFactory)
    {
        if (targetCollection is null)
            throw new ArgumentNullException(nameof(targetCollection));

        if (conditionFunction is null)
            throw new ArgumentNullException(nameof(conditionFunction));

        if (valueFactory is null)
            throw new ArgumentNullException(nameof(valueFactory));

        if (conditionFunction())
            targetCollection.Add(valueFactory());

        return targetCollection;
    }

    /// <summary>
    /// Adds a value to the collection when the provided condition function returns true.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="targetCollection">The collection to add the value to.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>
    /// The same collection, as to enable fluent API calls.
    /// </returns>
    public static IList<T> AddWhen<T>(this IList<T> targetCollection,
        bool condition,
        T value)
    {
        if (targetCollection == null)
            throw new ArgumentNullException(nameof(targetCollection));

        if (condition)
            targetCollection.Add(value);

        return targetCollection;
    }

    /// <summary>
    /// Adds a value to the collection when the provided condition function returns true.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="targetCollection">The collection to add the values to.</param>
    /// <param name="conditionFunction">The condition evaluation function.</param>
    /// <param name="valuesFactory">The factory method that produces the values to add.</param>
    /// <returns>
    /// The same collection, as to enable fluent API calls.
    /// </returns>
    public static IList<T> AddRangeWhen<T>(this IList<T> targetCollection,
        Func<bool> conditionFunction,
        Func<IEnumerable<T>> valuesFactory)
    {
        if (targetCollection is null)
            throw new ArgumentNullException(nameof(targetCollection));

        if (conditionFunction is null)
            throw new ArgumentNullException(nameof(conditionFunction));

        if (valuesFactory is null)
            throw new ArgumentNullException(nameof(valuesFactory));

        if (!conditionFunction())
            return targetCollection;

        foreach (var item in valuesFactory())
            targetCollection.Add(item);

        return targetCollection;
    }
}
