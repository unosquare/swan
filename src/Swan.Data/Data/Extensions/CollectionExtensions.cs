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
    /// Converts a <see cref="IDbTableSchema"/> object into its <see cref="DataTable"/>
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

    public static DataTable GetSchemaTable(this ITypeInfo objectType, string? tableName = default)
    {
        var table = new DataTable
        {
            TableName = tableName ?? objectType.NativeType.Name
        };

        var schemaTypeInfo = typeof(IDbColumnSchema).TypeInfo();
        var schemaTypeProps = schemaTypeInfo.Properties();

        foreach (var schemaColumn in schemaTypeProps)
            table.Columns.Add(schemaColumn.PropertyName, schemaColumn.PropertyType.NativeType);

        var objectProperties = objectType.Properties();

        var columnIndex = 0;
        foreach (var objectProperty in objectProperties)
        {
            if (!DbTypeMapper.Default.TryGetProviderTypeFor(objectProperty.PropertyType.NativeType, out var providerType))
                continue;

            if (objectProperty.PropertyName.Contains('.', StringComparison.Ordinal))
                continue;

            var columnSchema = new DbColumnSchema
            {
                DataType = objectProperty.PropertyType.NativeType,
                AllowsDBNull = objectProperty.PropertyType.IsNullable,
                Name = objectProperty.PropertyName,
                IsReadOnly = !objectProperty.CanWrite,
                Ordinal = columnIndex,
                ProviderDataType = providerType
            };
            columnIndex++;

            var fieldValues = new object?[schemaTypeProps.Count];
            var fieldIndex = 0;
            foreach (var propery in schemaTypeProps)
            {
                if (propery.TryRead(column, out var value))
                    fieldValues[fieldIndex] = value;

                fieldIndex++;
            }

            table.Rows.Add(fieldValues);
        }

        return table;
    }

    public static IDataReader GetDataReader(this IEnumerable collection, IDbTableSchema schema)
    {

    }
}
