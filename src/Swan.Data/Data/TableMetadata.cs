namespace Swan.Data;

/// <summary>
/// Provides generalized table schema information as a dictionary of columns.
/// </summary>
public sealed class TableMetadata : Dictionary<string, ColumnMetadata>
{
    private static readonly object CacheLock = new();
    private static readonly Dictionary<string, Dictionary<string, TableMetadata>> TableMetaCache = new(1024, StringComparer.InvariantCultureIgnoreCase);

    private TableMetadata(DbConnection connection, string tableName, string? schemaName)
        : base(128, StringComparer.InvariantCultureIgnoreCase)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        var factory = DbProviderFactories.GetFactory(connection);

        if (factory is null)
            throw new ArgumentException($"Could not obtain {nameof(DbProviderFactory)} from connection.", nameof(connection));

        var provider = connection.Provider();
        Database = connection.Database ?? string.Empty;
        TableName = tableName;
        Schema = schemaName ?? provider.DefaultSchemaName;
        QuotedName = provider.Quote(this);

        FillSchemaColumns(connection);

        KeyColumns = Keys.Count > 0
            ? Values.Where(c => c.IsKey).OrderBy(c => c.ColumnOrdinal).ToArray()
            : Array.Empty<ColumnMetadata>();

        ColumnNames = Keys.Count > 0
            ? Values.OrderBy(c => c.ColumnOrdinal).Select(c => c.ColumnName).ToArray()
            : Array.Empty<string>();

        IsReadOnly = KeyColumns == null || KeyColumns.Count <= 0;
    }

    /// <summary>
    /// Gets the database name this table belongs to.
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Gets the schema name with no quotes.
    /// </summary>
    public string Schema { get; }

    /// <summary>
    /// Gets the table name with no quotes or schema names.
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// Gets the fully quoted name of the table, including its schema.
    /// </summary>
    public string QuotedName { get; }

    /// <summary>
    /// When the table has no key columns, this will return true.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Gets the unquoted list of all column names.
    /// </summary>
    public IReadOnlyList<string> ColumnNames { get; }

    /// <summary>
    /// Gets the list of all key columns.
    /// </summary>
    public IReadOnlyList<ColumnMetadata> KeyColumns { get; }

    internal static async Task<TableMetadata> AcquireAsync(DbConnection connection, string tableName, string? schemaName)
    {
        await connection.EnsureIsValidAsync();

        var existingMeta = ReadSchemaCache(connection, tableName);
        if (existingMeta != null)
            return existingMeta;

        var tableMeta = new TableMetadata(connection, tableName, schemaName);
        WriteSchemaCache(connection, tableMeta);
        return tableMeta;
    }

    private void FillSchemaColumns(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {QuotedName} WHERE 1 = 2";

        using var reader = command.ExecuteReader(CommandBehavior.KeyInfo);
        var schemaTable = reader.GetSchemaTable();
        if (schemaTable is null)
            throw new NotSupportedException("Unable to obtain schema information.");

        Clear();

        for (var columnIndex = 0; columnIndex < schemaTable.Rows.Count; columnIndex++)
        {
            var columnData = schemaTable.Rows[columnIndex];
            var columnMeta = new ColumnMetadata();
            var properties = columnMeta.GetType().TypeInfo().Properties;

            for (var propertyIndex = 0; propertyIndex < schemaTable.Columns.Count; propertyIndex++)
            {
                var propertyName = schemaTable.Columns[propertyIndex].ColumnName;
                var property = properties.ContainsKey(propertyName)
                    ? properties[propertyName]
                    : null;

                if (property == null)
                    continue;

                var value = columnData[propertyName];
                if (!property.TryWrite(columnMeta, value))
                    throw new InvalidCastException(
                    $"Unable to convert field '{propertyName}' of type '{value.GetType().Name}' " +
                    $"to property '{property.PropertyType}' for field '{property.PropertyName}'");
            }

            this[columnMeta.ColumnName] = columnMeta;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{QuotedName} ({Keys.Count} columns)";
    }

    private static TableMetadata? ReadSchemaCache(DbConnection connection, string tableName)
    {
        var connectionString = connection.ConnectionString;

        lock (CacheLock)
        {
            if (!TableMetaCache.ContainsKey(connectionString))
                TableMetaCache[connectionString] = new Dictionary<string, TableMetadata>(128, StringComparer.InvariantCultureIgnoreCase);

            return TableMetaCache[connectionString].ContainsKey(tableName)
                ? TableMetaCache[connectionString][tableName]
                : null;
        }
    }

    private static void WriteSchemaCache(DbConnection connection, TableMetadata tableMeta)
    {
        if (tableMeta.Count <= 0)
            return;

        var connectionString = connection.ConnectionString;

        lock (CacheLock)
        {
            if (!TableMetaCache.ContainsKey(connectionString))
                TableMetaCache[connectionString] = new Dictionary<string, TableMetadata>(128, StringComparer.InvariantCultureIgnoreCase);

            TableMetaCache[connectionString][tableMeta.TableName] = tableMeta;
        }
    }
}
