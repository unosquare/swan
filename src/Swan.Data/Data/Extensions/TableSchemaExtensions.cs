namespace Swan.Data.Extensions;

/// <summary>
/// Provides methods to generate and convert <see cref="IDbTableSchema"/> objects.
/// </summary>
public static class TableSchemaExtensions
{
    /// <summary>
    /// Converts a type to a compatible form of <see cref="IDbTableSchema"/>.
    /// </summary>
    /// <param name="objectType">The type to produce the schema from.</param>
    /// <param name="connection">The optional connection.</param>
    /// <param name="tableName">The optional table name. If not specified, it will use the type name.</param>
    /// <param name="schemaName">The database schema name. If not specified it will be set to the provider-specific default.</param>
    /// <returns>A table schema.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IDbTableSchema ToTableSchema(this Type objectType, DbConnection? connection = default, string? tableName = default, string? schemaName = default)
    {
        if (objectType is null)
            throw new ArgumentNullException(nameof(objectType));

        var typeInfo = objectType.TypeInfo();
        var properties = typeInfo.Properties();
        var columns = new List<DbColumnSchema>(properties.Count);
        var typeMapper = connection?.Provider()?.TypeMapper ?? DbTypeMapper.Default;

        var columnIndex = 0;
        foreach (var property in properties)
        {
            var columnSchema = property.ToColumnSchema(columnIndex, typeMapper);
            if (columnSchema is not DbColumnSchema dbColumn)
                continue;

            columns.Add(dbColumn);
            columnIndex++;
        }

        var identityCandidate = columns.Where(
            c => !string.IsNullOrWhiteSpace(c.Name) &&
            c.Name.ToUpperInvariant().EndsWith("ID", StringComparison.Ordinal) &&
            c.DataType.TypeInfo().IsNumeric &&
            !c.DataType.TypeInfo().IsNullable)
            .OrderBy(c => c.Ordinal)
            .ThenBy(c => c.Name)
            .FirstOrDefault();

        if (identityCandidate is not null)
        {
            identityCandidate.IsKey = true;
            identityCandidate.IsAutoIncrement = true;
        }

        connection?.EnsureConnected();
        return new DbTableSchema(connection?.Database ?? "db", tableName ?? objectType.Name, schemaName ?? string.Empty, columns);
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
    /// Create a <see cref="ITableBuilder"/> based on a <see cref="ITableContext"/> object.
    /// </summary>
    /// <param name="table">The table context to read the schema information from..</param>
    /// <returns>The table builder object.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ITableBuilder ToTableBuilder(this ITableContext table) => 
        table is null ? throw new ArgumentNullException(nameof(table)) : new TableContext(table.Connection, table);

    private static IDbColumnSchema? ToColumnSchema(this IPropertyProxy p, int columnIndex, IDbTypeMapper typeMapper)
    {
        if (!typeMapper.TryGetProviderTypeFor(p.PropertyType.NativeType, out var providerType) ||
            p.PropertyName.Contains('.', StringComparison.Ordinal) || !p.CanRead || !p.HasPublicGetter)
            return default;

        var dataType = p.PropertyType.BackingType.NativeType;
        var length = dataType == typeof(string)
            ? 512 : dataType == typeof(byte[])
            ? 4000 : dataType == typeof(ulong)
            ? 20 : 0;

        var precision = dataType == typeof(decimal) ? 19 : 0;
        var scale = dataType == typeof(decimal) ? 4 : 0;

        return new DbColumnSchema
        {
            DataType = dataType,
            AllowsDBNull = p.PropertyType.IsNullable,
            Name = p.PropertyName,
            IsReadOnly = !p.CanWrite,
            ProviderDataType = providerType,
            Ordinal = columnIndex,
            MaxLength = length,
            // TODO: Scale and precision not yet fully resolved.
            Scale = (byte)scale,
            Precision = (byte)precision
        };
    }

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
