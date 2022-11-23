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
        var columns = new List<IDbColumnSchema>(128);
        var typeMapper = connection?.Provider()?.TypeMapper ?? DbTypeMapper.Default;

        foreach (var (columnName, property) in typeInfo.GetColumnMap())
        {
            var columnSchema = property.ToColumnSchema(columnName, columns.Count, typeMapper);
            if (columnSchema is not DbColumnSchema dbColumn)
                continue;

            // Database generated attribute
            if (property.Attribute<DatabaseGeneratedAttribute>() is DatabaseGeneratedAttribute generatedOption
                && generatedOption.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
            {
                dbColumn.IsAutoIncrement = generatedOption.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
                dbColumn.IsReadOnly = dbColumn.IsAutoIncrement ||
                    generatedOption.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed;
            }

            // Key attribute
            if (property.Attribute<KeyAttribute>() is not null)
                dbColumn.IsKey = true;

            columns.Add(dbColumn);
        }

        // Key and AutoIncrement
        if (TryGuessIdentityKey(columns, out var keyColumn) && keyColumn is DbColumnSchema identityKeyColumn)
        {
            identityKeyColumn.IsKey = true;
            identityKeyColumn.IsAutoIncrement = true;
        }

        // Unique property
        var keyColumns = columns.Where(c => c.IsKey).ToArray();
        if (keyColumns.Length == 1 && keyColumns[0] is DbColumnSchema singleKeyColumn)
            singleKeyColumn.IsUnique = true;

        // Populate table name and schema name properties when available
        var tableAttribute = objectType.Attribute<TableAttribute>();
        tableName ??= tableAttribute is not null && !string.IsNullOrWhiteSpace(tableAttribute.Name)
            ? tableAttribute.Name
            : objectType.Name;

        schemaName ??= tableAttribute is not null && !string.IsNullOrWhiteSpace(tableAttribute.Schema)
            ? tableAttribute.Schema
            : string.Empty;

        connection?.EnsureConnected();
        return new DbTableSchema(connection?.Database ?? "db", tableName, schemaName, columns);
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

    /// <summary>
    /// Create a <see cref="ITableBuilder"/> based on a <see cref="ITableContext"/> object.
    /// </summary>
    /// <param name="table">The table context to read the schema information from..</param>
    /// <returns>The table builder object.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ITableBuilder<T> ToTableBuilder<T>(this ITableContext<T> table)
        where T : class =>
        table is null ? throw new ArgumentNullException(nameof(table)) : new TableContext<T>(table.Connection, table);

    /// <summary>
    /// Quotes the specified table.
    /// </summary>
    /// <typeparam name="T">The table type.</typeparam>
    /// <param name="table">The table.</param>
    /// <returns>A string representing a quoted table name.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string QuoteTable<T>(this T table)
        where T : IDbTableSchema, IDbConnected =>
        table is null ? throw new ArgumentNullException(nameof(table)) : table.Provider.QuoteTable(table);

    internal static bool TryGuessIdentityKey(IList<IDbColumnSchema> columns, [MaybeNullWhen(false)] out IDbColumnSchema dbColumn)
    {
        var candidates = columns.Where(c =>
            !c.AllowDBNull &&
            !string.IsNullOrWhiteSpace(c.ColumnName) &&
            c.ColumnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
            c.IsReadOnly &&
            c.DataType.TypeInfo().IsNumeric &&
            c.IsAutoIncrement).ToArray();

        if (candidates.Length > 0)
        {
            if (candidates.Length == 1)
            {
                dbColumn = candidates[0];
                return true;
            }

            var keyColumns = candidates.Where(c => c.IsKey).ToArray();
            if (keyColumns.Length == 1)
            {
                dbColumn = keyColumns[0];
                return true;
            }
        }

        candidates = columns.Where(c =>
            !c.AllowDBNull &&
            !string.IsNullOrWhiteSpace(c.ColumnName) &&
            c.ColumnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
            c.DataType.TypeInfo().IsNumeric)
            .OrderBy(c => c.ColumnOrdinal)
            .ThenBy(c => c.ColumnName)
            .ToArray();

        if (candidates.Length > 0)
        {
            dbColumn = candidates[0];
            return true;
        }

        dbColumn = default;
        return false;
    }

    private static IDbColumnSchema? ToColumnSchema(this IPropertyProxy p, string columnName, int columnIndex, IDbTypeMapper typeMapper)
    {
        if (!p.CanRead || !p.HasPublicGetter)
            return default;

        if (!typeMapper.TryGetProviderTypeFor(p.PropertyType.NativeType, out var providerType))
            return default;

        if (!typeMapper.TryGetDatabaseTypeFor(p.PropertyType.NativeType, out var databaseType))
            return default;

        var dataType = p.PropertyType.BackingType.NativeType;
        var length = dataType == typeof(string)
            ? 512 : dataType == typeof(byte[])
            ? 4000 : dataType == typeof(ulong)
            ? 20 : 0;

        // TODO: Scale and precision not yet fully resolved.
        var precision = dataType == typeof(decimal) ? 19 : 0;
        var scale = dataType == typeof(decimal) ? 4 : 0;

        return new DbColumnSchema
        {
            DataType = dataType,
            AllowDBNull = p.PropertyType.IsNullable,
            ColumnName = columnName,
            IsReadOnly = !p.CanWrite || !p.HasPublicSetter,
            ProviderType = $"{providerType}",
            DataTypeName = $"{databaseType}",
            ColumnOrdinal = columnIndex,
            ColumnSize = length,
            NumericScale = scale,
            NumericPrecision = precision
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
