namespace Swan.Data;

/// <summary>
/// Provides data reader extensions to efficiently parse objects from the underlying data source.
/// </summary>
public static class DbDataReaderExtensions
{
    /// <summary>
    /// Reads an object from the underlying reader at the current position.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="t">The type of object this method returns.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static object ReadObject(this DbDataReader reader, Type t, Func<object>? typeFactory = default)
    {
        if (reader is null)
            throw new ArgumentNullException(nameof(reader));

        typeFactory ??= () => t.TypeInfo().CreateInstance();
        var result = typeFactory.Invoke();
        var properties = t.Properties();
        var fieldNames = new Dictionary<string, int>(reader.FieldCount, StringComparer.InvariantCultureIgnoreCase);
        for (var i = 0; i < reader.FieldCount; i++)
            fieldNames[reader.GetName(i)] = i;

        foreach (var property in properties)
        {
            if (!property.CanWrite)
                continue;

            var ordinal = fieldNames.ContainsKey(property.PropertyName)
                ? fieldNames[property.PropertyName]
                : -1;

            if (ordinal < 0)
                continue;

            if (reader.IsDBNull(ordinal))
            {
                property.Write(result, property.PropertyType.DefaultValue);
                continue;
            }

            var fieldValue = reader.GetValue(ordinal);
            if (!property.TryWrite(result, fieldValue))
                throw new InvalidCastException(
                $"Unable to convert field type '{fieldValue.GetType().Name}'" +
                $" to property '{property.PropertyType}' for field '{property.PropertyName}'");
        }

        return result;
    }

    /// <summary>
    /// Reads an object from the underlying reader at the current position.
    /// </summary>
    /// <typeparam name="T">The type of object this method returns.</typeparam>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static T ReadObject<T>(this DbDataReader reader, Func<T>? typeFactory = default) =>
        (T)reader.ReadObject(typeof(T), typeFactory is null ? default : () => typeFactory.Invoke()!);

    /// <summary>
    /// Reads a dynamically typed object from the underlying reader at the current position.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns>A dynamic <see cref="ExpandoObject"/></returns>
    public static dynamic ReadObject(this DbDataReader reader)
    {
        if (reader is null)
            throw new ArgumentNullException(nameof(reader));

        var result = new ExpandoObject();

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var fieldName = reader.GetName(i);
            var fieldValue = reader.IsDBNull(i) ? null : reader.GetValue(i);
            var propertyName = GetExpandoPropertyName(fieldName, i);

            result.TryAdd(propertyName, fieldValue);
        }

        return result;
    }

    /// <summary>
    /// Removes special characters that cannot be represented as property names such as spaces.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="fieldIndex">The index appearance of the field.</param>
    /// <returns>A valid property name with only letters, digits or underscores.</returns>
    private static string GetExpandoPropertyName(string fieldName, int fieldIndex)
    {
        fieldName ??= $"Field_{fieldIndex.ToString(CultureInfo.InvariantCulture)}";
        var builder = new StringBuilder(fieldName.Length);
        foreach (var c in fieldName)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                continue;

            builder.Append(c);
        }

        return char.IsDigit(builder[0])
            ? $"_{builder}"
            : builder.ToString();
    }
}
