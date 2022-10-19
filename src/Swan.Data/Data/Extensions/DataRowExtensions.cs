namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension menthods for <see cref="DataRow"/> objects.
/// </summary>
public static class DataRowExtensions
{
    /// <summary>
    /// Reads an object from the underlying row.
    /// </summary>
    /// <param name="row">The data row to convert.</param>
    /// <param name="t">The type of object this method returns.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the reader.</returns>
    public static object ParseObject(this DataRow row, Type t, Func<object>? typeFactory = default)
    {
        if (row is null)
            throw new ArgumentNullException(nameof(row));

        if (t is null)
            throw new ArgumentNullException(nameof(t));

        var typeInfo = t.TypeInfo();
        typeFactory ??= () => typeInfo.CreateInstance();
        var result = typeFactory.Invoke();

        var fieldNames = new Dictionary<string, int>(row.Table.Columns.Count, StringComparer.Ordinal);
        for (var i = 0; i < row.Table.Columns.Count; i++)
            fieldNames[row.Table.Columns[i].ColumnName] = i;

        foreach (var (fieldName, columnIndex) in fieldNames)
        {
            if (!typeInfo.TryFindProperty(fieldName, out var property) ||
                property is null || !property.CanWrite || !property.HasPublicSetter)
                continue;

            if (row[columnIndex] == DBNull.Value)
            {
                property.Write(result, property.PropertyType.DefaultValue);
                continue;
            }

            var fieldValue = row[columnIndex];
            if (!property.TryWrite(result, fieldValue))
                throw new InvalidCastException(
                $"Unable to convert value for field '{fieldName}' of type '{fieldValue.GetType().Name}' to '{property.PropertyType}'");

        }

        return result;
    }

    /// <summary>
    /// Reads an object from the underlying row.
    /// </summary>
    /// <typeparam name="T">The type of object this method returns.</typeparam>
    /// <param name="row">The row to read from.</param>
    /// <param name="typeFactory">An optional factory method to create an instance.</param>
    /// <returns>An object instance filled with the values from the row.</returns>
    public static T ParseObject<T>(this DataRow row, Func<T>? typeFactory = default) =>
        (T)row.ParseObject(typeof(T), typeFactory is null ? default : () => typeFactory.Invoke()!);

    /// <summary>
    /// Reads a dynamically typed object from the underlying row.
    /// </summary>
    /// <param name="row">The data row to read from.</param>
    /// <returns>A dynamic <see cref="ExpandoObject"/></returns>
    public static ExpandoObject ParseExpando(this DataRow row)
    {
        if (row is null)
            throw new ArgumentNullException(nameof(row));

        var result = new ExpandoObject();

        for (var i = 0; i < row.Table.Columns.Count; i++)
        {
            var fieldName = row.Table.Columns[i].ColumnName;
            var fieldValue = row[i] == DBNull.Value ? null : row[i];
            var propertyName = fieldName.ToExpandoPropertyName(i);

            result.TryAdd(propertyName, fieldValue);
        }

        return result;
    }
}
