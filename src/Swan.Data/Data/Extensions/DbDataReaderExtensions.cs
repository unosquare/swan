namespace Swan.Data.Extensions;

using System.Collections;

/// <summary>
/// Provides methods to extend usage of collections as standard <see cref="IDataReader"/> objects.
/// </summary>
public static class DbDataReaderExtensions
{
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

    /// <summary>
    /// Wraps the enumerator of the given collection as a <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="collection">The collection to get the <see cref="IEnumerator"/> from.</param>
    /// <param name="schema">The schema used to produce the record values for the data reader.</param>
    /// <returns>A data reader.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ICollectionDataReader GetDataReader(this IEnumerable collection, IDbTableSchema schema) => collection is null
        ? throw new ArgumentNullException(nameof(collection))
        : new CollectionDataReader(collection.GetEnumerator(), schema);

    /// <summary>
    /// Wraps the enumerator of the given collection as a <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="collection">The collection to get the <see cref="IEnumerator"/> from.</param>
    /// <param name="itemType">The type of the items the collection holds. This produces a basic table schema.</param>
    /// <returns>A data reader.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ICollectionDataReader GetDataReader(this IEnumerable collection, Type itemType) => collection is null
        ? throw new ArgumentNullException(nameof(collection))
        : new CollectionDataReader(collection.GetEnumerator(), itemType);

    /// <summary>
    /// Wraps the enumerator of the given collection as a <see cref="IDataReader"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items the collection holds. This produces a basic table schema.</typeparam>
    /// <param name="collection">The collection to get the <see cref="IEnumerator"/> from.</param>
    /// <returns>A data reader.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ICollectionDataReader<T> GetDataReader<T>(this IEnumerable<T> collection) => collection is null
        ? throw new ArgumentNullException(nameof(collection))
        : new CollectionDataReader<T>(collection.GetEnumerator());

    /// <summary>
    /// Wraps the enumerator of the given collection as a <see cref="IDataReader"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items the collection holds. This produces a basic table schema.</typeparam>
    /// <param name="collection">The collection to get the <see cref="IEnumerator"/> from.</param>
    /// <param name="schema">The schema used to produce the record values for the data reader.</param>
    /// <returns>A data reader.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ICollectionDataReader<T> GetDataReader<T>(this IEnumerable<T> collection, IDbTableSchema schema) => collection is null
        ? throw new ArgumentNullException(nameof(collection))
        : new CollectionDataReader<T>(collection.GetEnumerator(), schema);

    private static IDbColumnSchema? ToColumnSchema(this IPropertyProxy objectProperty, int columnIndex) =>
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

    private static DataRow AddSchemaRow(this DataTable schemaTable, IDbColumnSchema columnSchema, ITypeInfo? columnSchemaType = default)
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
}
