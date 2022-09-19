namespace Swan.Mapping;

using Reflection;
/// <inheritdoc/>

public static class MappingExtensions
{
    /// <summary>
    /// Iterates over the public, instance, readable properties of the source and
    /// tries to write a compatible value to a public, instance, writable property in the destination.
    /// </summary>
    /// <typeparam name="T">The type of the source.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="ignoreProperties">The ignore properties.</param>
    /// <returns>
    /// Number of properties that was copied successful.
    /// </returns>
    public static int CopyPropertiesTo<T>(this T? source, object? target, params string[]? ignoreProperties)
        where T : class =>
        ObjectMapper.Copy(source, target, GetCopyableProperties(target), ignoreProperties);

    /// <summary>
    /// Iterates over the public, instance, readable properties of the source and
    /// tries to write a compatible value to a public, instance, writable property in the destination.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The destination.</param>
    /// <param name="propertiesToCopy">Properties to copy.</param>
    /// <returns>
    /// Number of properties that were successfully copied.
    /// </returns>
    public static int CopyOnlyPropertiesTo(this object? source, object? target, params string[]? propertiesToCopy)
        => ObjectMapper.Copy(source, target, propertiesToCopy);

    /// <summary>
    /// Copies the properties to new instance of T.
    /// </summary>
    /// <typeparam name="T">The new object type.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="ignoreProperties">The ignore properties.</param>
    /// <returns>
    /// The specified type with properties copied.
    /// </returns>
    /// <exception cref="ArgumentNullException">source.</exception>
    public static T CopyPropertiesToNew<T>(this object? source, string[]? ignoreProperties = null)
        where T : class
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var target = TypeManager.CreateInstance<T>();
        ObjectMapper.Copy(source, target, GetCopyableProperties(target), ignoreProperties);

        return target;
    }

    /// <summary>
    /// Copies the only properties to new instance of T.
    /// </summary>
    /// <typeparam name="T">Object Type.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="propertiesToCopy">The properties to copy.</param>
    /// <returns>
    /// The specified type with properties copied.
    /// </returns>
    /// <exception cref="ArgumentNullException">source.</exception>
    public static T CopyOnlyPropertiesToNew<T>(this object source, params string[] propertiesToCopy)
        where T : class
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var target = TypeManager.CreateInstance<T>();
        ObjectMapper.Copy(source, target, propertiesToCopy);

        return target;
    }

    /// <summary>
    /// Iterates over the keys of the source and tries to write a compatible value to a public, 
    /// instance, writable property in the destination.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="ignoreKeys">The ignore keys.</param>
    /// <returns>Number of properties that was copied successful.</returns>
    public static int CopyKeyValuePairTo(
        this IDictionary<string, object?> source,
        object? target,
        params string[] ignoreKeys) =>
        source == null
            ? throw new ArgumentNullException(nameof(source))
            : ObjectMapper.Copy(source, target, null, ignoreKeys);

    /// <summary>
    /// Iterates over the keys of the source and tries to write a compatible value to a public,
    /// instance, writable property in the destination.
    /// </summary>
    /// <typeparam name="T">Object Type.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="ignoreKeys">The ignore keys.</param>
    /// <returns>
    /// The specified type with properties copied.
    /// </returns>
    public static T CopyKeyValuePairToNew<T>(
        this IDictionary<string, object?> source,
        params string[] ignoreKeys)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var target = TypeManager.CreateInstance<T>();
        source.CopyKeyValuePairTo(target, ignoreKeys);
        return target;
    }

    /// <summary>
    /// Gets the copyable properties.
    ///
    /// If there is no properties with the attribute <c>AttributeCache</c> returns all the properties.
    /// </summary>
    /// <param name="this">The object.</param>
    /// <returns>
    /// Array of properties.
    /// </returns>
    /// <exception cref="ArgumentNullException">model.</exception>
    public static IEnumerable<string> GetCopyableProperties(this object? @this)
    {
        if (@this == null)
            throw new ArgumentNullException(nameof(@this));

        var collection = @this.GetType().Properties();

        var properties = collection
            .Select(x => new
            {
                x.PropertyName,
                HasAttribute = x.HasAttribute<CopyableAttribute>(),
            })
            .Where(x => x.HasAttribute)
            .Select(x => x.PropertyName);

        return properties.Any()
            ? properties
            : collection.Select(x => x.PropertyName);
    }
}
