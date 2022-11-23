namespace Swan.Data.Extensions;


/// <summary>
/// Provides extensions methods for <see cref="IDataRecord"/> objects.
/// </summary>
public static class DataRecordExtensions
{
    /// <summary>
    /// Converts a <see cref="DataRow"/> to an <see cref="IDataRecord"/>.
    /// </summary>
    /// <param name="row">The data row to convert.</param>
    /// <returns>The data record.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IDataRecord ToDataRecord(this DataRow row) => row is null
        ? throw new ArgumentNullException(nameof(row))
        : new DataRowDbRecord(row);

    /// <summary>
    /// Reads an object from the underlying record at the current position.
    /// </summary>
    /// <param name="record">The reader to read from.</param>
    /// <param name="t">The type of object this method returns.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static object ParseObject(this IDataRecord record, Type t, Func<object>? typeFactory = default) =>
        record.ParseObject(t.TypeInfo(), typeFactory);

    /// <summary>
    /// Reads an object from the underlying record at the current position.
    /// </summary>
    /// <typeparam name="T">The type of object this method returns.</typeparam>
    /// <param name="record">The reader to read from.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static T ParseObject<T>(this IDataRecord record, Func<T>? typeFactory = default) =>
        record.ParseObject(typeof(T).TypeInfo(), typeFactory);

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

    internal static T ParseObject<T>(this IDataRecord record, ITypeInfo typeInfo, Func<T>? typeFactory = default) =>
        (T)record.ParseObject(typeInfo, typeFactory is null ? default : () => typeFactory.Invoke()!);

    internal static object ParseObject(this IDataRecord record, ITypeInfo typeInfo, Func<object>? typeFactory = default)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        if (typeInfo is null)
            throw new ArgumentNullException(nameof(typeInfo));

        typeFactory ??= typeInfo.CreateInstance;
        var result = typeFactory.Invoke();

        // Write the mapped properties in sequential field index property.
        var columnMap = typeInfo.GetColumnMap();
        for (var fieldIndex = 0; fieldIndex < record.FieldCount; fieldIndex++)
        {
            var columnName = record.GetName(fieldIndex);
            if (string.IsNullOrWhiteSpace(columnName))
                continue;

            if (!columnMap.TryGetValue(columnName, out var property))
                continue;

            if (!property.CanWrite)
                continue;

            var fieldValue = record.IsDBNull(fieldIndex)
                ? property.PropertyType.DefaultValue
                : record.GetValue(fieldIndex);

            if (!property.TryWrite(result, fieldValue))
            {
                var errorMessage = $"Unable to convert value for column '{columnName}' of type '{record.GetType().Name}' to '{property.PropertyType}'";
                throw new InvalidCastException(errorMessage);
            }
        }

        return result;
    }
}

