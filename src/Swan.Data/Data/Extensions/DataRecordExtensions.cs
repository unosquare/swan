namespace Swan.Data.Extensions;

using Swan.Data.Records;
using Swan.Reflection;

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

    internal static object ParseObject(this IDataRecord record, ITypeInfo typeInfo, Func<object>? typeFactory = default)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));

        if (typeInfo is null)
            throw new ArgumentNullException(nameof(typeInfo));

        typeFactory ??= () => typeInfo.CreateInstance();
        var result = typeFactory.Invoke();

        // Create a property map where keys are field indices and values are properties to wrote to.
        var propertyList = typeInfo.Properties.Values.Where(c => c.CanWrite).ToList();
        var propertyMap = new SortedDictionary<int, IPropertyProxy>();

        for (var fieldIndex = 0; fieldIndex < record.FieldCount; fieldIndex++)
        {
            // No more properties to map.
            if (propertyList.Count <= 0)
                break;

            // Get the name of the field to map
            var columnName = record.GetName(fieldIndex);
            if (string.IsNullOrWhiteSpace(columnName))
                continue;

            // First, try mapping by column attribute
            var property = propertyList.FirstOrDefault(c =>
                columnName.Equals(c.Attribute<ColumnAttribute>()?.Name ?? string.Empty,
                StringComparison.OrdinalIgnoreCase));

            // If no column attribute, try to find it by name.
            if (property is null && typeInfo.TryFindProperty(columnName, out var foundProperty) && propertyList.Contains(foundProperty))
                property = foundProperty;

            // move on if we can't find a property to map to
            if (property is null)
                continue;

            // add to property map and remove from property list
            propertyMap[fieldIndex] = property;
            propertyList.Remove(property);
        }

        // Read fields in sequential access mode and wrote to the mapped properties.
        foreach ((var fieldIndex, var property) in propertyMap)
        {
            var fieldValue = record.IsDBNull(fieldIndex)
                ? property.PropertyType.DefaultValue
                : record.GetValue(fieldIndex);

            if (!property.TryWrite(result, fieldValue))
                throw new InvalidCastException(
                $"Unable to convert value for field '{record.GetName(fieldIndex)}' of type '{record.GetType().Name}' to '{property.PropertyType}'");
        }

        return result;
    }

    internal static T ParseObject<T>(this IDataRecord record, ITypeInfo typeInfo, Func<T>? typeFactory = default) =>
        (T)record.ParseObject(typeInfo, typeFactory is null ? default : () => typeFactory.Invoke()!);

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
}

