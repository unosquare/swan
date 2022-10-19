namespace Swan.Data.Extensions;

/// <summary>
/// Provides extensions methods for <see cref="IDataRecord"/> objects.
/// </summary>
public static class DataRecordExtensions
{
    /// <summary>
    /// Reads an object from the underlying record at the current position.
    /// </summary>
    /// <param name="record">The reader to read from.</param>
    /// <param name="t">The type of object this method returns.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static object ParseObject(this IDataRecord record, Type t, Func<object>? typeFactory = default)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        if (t is null)
            throw new ArgumentNullException(nameof(t));

        var typeInfo = t.TypeInfo();
        typeFactory ??= () => typeInfo.CreateInstance();
        var result = typeFactory.Invoke();

        var fieldNames = new Dictionary<string, int>(record.FieldCount, StringComparer.Ordinal);
        for (var i = 0; i < record.FieldCount; i++)
            fieldNames[record.GetName(i)] = i;

        foreach (var (fieldName, columnIndex) in fieldNames)
        {
            if (!typeInfo.TryFindProperty(fieldName, out var property) ||
                property is null || !property.CanWrite || !property.HasPublicSetter)
                continue;


            if (record.IsDBNull(columnIndex))
            {
                property.Write(result, property.PropertyType.DefaultValue);
                continue;
            }

            var fieldValue = record.GetValue(columnIndex);
            if (!property.TryWrite(result, fieldValue))
                throw new InvalidCastException(
                $"Unable to convert value for field '{fieldName}' of type '{fieldValue.GetType().Name}' to '{property.PropertyType}'");
        }

        return result;
    }

    /// <summary>
    /// Reads an object from the underlying record at the current position.
    /// </summary>
    /// <typeparam name="T">The type of object this method returns.</typeparam>
    /// <param name="record">The reader to read from.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static T ParseObject<T>(this IDataRecord record, Func<T>? typeFactory = default) =>
        (T)record.ParseObject(typeof(T), typeFactory is null ? default : () => typeFactory.Invoke()!);

    /// <summary>
    /// Reads a dynamically typed object from the underlying record at the current position.
    /// </summary>
    /// <param name="record">The data record to read from</param>
    /// <returns>A dynamic <see cref="ExpandoObject"/></returns>
    public static ExpandoObject ParseExpando(this IDataRecord record)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        var result = new ExpandoObject();

        for (var i = 0; i < record.FieldCount; i++)
        {
            var fieldName = record.GetName(i);
            var fieldValue = record.IsDBNull(i) ? null : record.GetValue(i);
            var propertyName = fieldName.ToExpandoPropertyName(i);

            result.TryAdd(propertyName, fieldValue);
        }

        return result;
    }
}

