namespace Swan.Data.Extensions;

using Swan.Reflection;
using System.Collections;

/// <summary>
/// Provides extensions for asynchronous enumerables
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Retrieves the first result from a <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The enumerable type.</typeparam>
    /// <param name="enumerable">The enumerable object.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The first item or the default value for the type parameter.</returns>
    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken ct = default)
    {
        if (enumerable is null)
            return default;

        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            return item;

        return default;
    }

    /// <summary>
    /// Asynchronously iterates over each element and produces a list of items.
    /// This materializes the asynchronous enumerable set.
    /// </summary>
    /// <typeparam name="T">The element type of the list.</typeparam>
    /// <param name="enumerable">The enumerable to iterate over.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of elements.</returns>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken ct = default)
    {
        const int BufferSize = 1024;

        if (enumerable is null)
            return new(0);

        var result = new List<T>(BufferSize);
        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
        {
            if (item is null)
                continue;

            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Converts a type to a compatible form of <see cref="IDbTableSchema"/>.
    /// </summary>
    /// <param name="objectType">The type to produce the schema from.</param>
    /// <returns>A table schema.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IDbTableSchema ToTableSchema(this Type objectType)
    {
        if (objectType is null)
            throw new ArgumentNullException(nameof(objectType));

        var typeInfo = objectType.TypeInfo();
        var columns = new List<IDbColumnSchema>(128);

        var columnIndex = 0;
        foreach (var property in typeInfo.Properties())
        {
            var columnSchema = property.ToColumnSchema(columnIndex);
            if (columnSchema is null)
                continue;

            columns.Add(columnSchema);
            columnIndex++;
        }

        return new DbTableSchema("database", objectType.Name, string.Empty, columns);
    }

    /// <summary>
    /// Converts a <see cref="IDbTableSchema"/> object into its <see cref="DataTable"/> representation.
    /// </summary>
    /// <param name="tableSchema">The table schema to convert.</param>
    /// <returns>A DataTable that represents the schema.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DataTable ToSchemaTable(this IDbTableSchema tableSchema)
    {
        if (tableSchema is null)
            throw new ArgumentNullException(nameof(tableSchema));

        var dataTable = new DataTable(tableSchema.TableName);
        ITypeInfo? columnSchemaType = null;
        foreach (var column in tableSchema.Columns)
        {
            if (column is null)
                continue;

            // initialize the data table's columns and capture the
            // concrete column schema type.
            if (columnSchemaType is null)
            {
                columnSchemaType = column.GetType().TypeInfo();
                foreach (var col in columnSchemaType.Properties())
                {
                    if (col.PropertyName.Contains('.', StringComparison.Ordinal))
                        continue;

                    dataTable.Columns.Add(col.PropertyName, col.PropertyType.NativeType);
                }
            }

            dataTable.AddSchemaRow(column, columnSchemaType);
        }

        return dataTable;
    }

    /// <summary>
    /// Converst a type's properties into a DataTable representing it's basic schema.
    /// </summary>
    /// <param name="objectType">The type to extract the schema from.</param>
    /// <returns>A <see cref="DataTable"/> containing the types schema.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DataTable ToSchemaTable(this Type objectType) => objectType is null
        ? throw new ArgumentNullException(nameof(objectType))
        : objectType.ToTableSchema().ToSchemaTable();

    internal static IDbColumnSchema? ToColumnSchema(this IPropertyProxy objectProperty, int columnIndex) =>
        !DbTypeMapper.Default.TryGetProviderTypeFor(objectProperty.PropertyType.NativeType, out var providerType)
            ? null
            : objectProperty.PropertyName.Contains('.', StringComparison.Ordinal)
            ? null
            : new DbColumnSchema
            {
                DataType = objectProperty.PropertyType.NativeType,
                AllowsDBNull = objectProperty.PropertyType.IsNullable,
                Name = objectProperty.PropertyName,
                IsReadOnly = !objectProperty.CanWrite,
                ProviderDataType = providerType,
                Ordinal = columnIndex,
            } as IDbColumnSchema;

    internal static DataRow AddSchemaRow(this DataTable schemaTable, IDbColumnSchema columnSchema, ITypeInfo? columnSchemaType = default)
    {
        if (schemaTable is null)
            throw new ArgumentNullException(nameof(schemaTable));

        if (columnSchema is null)
            throw new ArgumentNullException(nameof(columnSchema));

        columnSchemaType ??= columnSchema.GetType().TypeInfo();

        var rowValues = new object?[schemaTable.Columns.Count];
        var columnIndex = 0;
        foreach (DataColumn column in schemaTable.Columns)
        {
            if (columnSchemaType.TryReadProperty(columnSchema, column.ColumnName, out var value))
                rowValues[columnIndex] = value;

            columnIndex++;
        }

        return schemaTable.Rows.Add(rowValues);
    }

    public static IDataReader GetDataReader(this IEnumerable collection, IDbTableSchema schema)
    {

    }
}
