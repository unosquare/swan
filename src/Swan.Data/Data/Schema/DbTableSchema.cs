namespace Swan.Data.Schema;

/// <summary>
/// Represents table structure information from the backing data store.
/// </summary>
internal sealed class DbTableSchema : IDbTableSchema
{
    /// <summary>
    /// Holds the column collection as a dictionary where column names are case-insensitive.
    /// </summary>
    private readonly Dictionary<string, IDbColumnSchema> _columns = new(128, StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Creates a new instance of the <see cref="DbTableSchema"/> class.
    /// </summary>
    public DbTableSchema(string database, string tableName, string schema, IEnumerable<IDbColumnSchema>? columns = default)
    {
        if (string.IsNullOrWhiteSpace(database))
            throw new ArgumentNullException(nameof(database));

        Database = database;
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));

        if (columns is null)
            return;

        foreach (var column in columns)
            _columns[column.Name] = column;
    }

    /// <inheritdoc />
    public IDbColumnSchema? this[string name] => _columns.TryGetValue(name, out var column) ? column : null;

    /// <inheritdoc />
    public string Database { get; }

    /// <inheritdoc />
    public string Schema { get; }

    /// <inheritdoc />
    public string TableName { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> Columns => _columns.Values.ToArray();

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> KeyColumns => Columns.Where(c => c.IsKey).ToArray();

    /// <inheritdoc />
    public IDbColumnSchema? IdentityKeyColumn => Columns.FirstOrDefault(c => c.IsKey && c.IsAutoIncrement);

    /// <inheritdoc />
    public bool HasKeyIdentityColumn => IdentityKeyColumn != null;

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> InsertableColumns => Columns.Where(c => !c.IsAutoIncrement && !c.IsReadOnly).ToArray();

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> UpdateableColumns => Columns.Where(c => !c.IsKey && !c.IsAutoIncrement && !c.IsReadOnly).ToArray();

    /// <inheritdoc />
    public IDbTableSchema AddColumn(IDbColumnSchema column)
    {
        if (column is null)
            throw new ArgumentNullException(nameof(column));

        if (string.IsNullOrWhiteSpace(column.Name))
            throw new ArgumentException("The column name must be specified.", nameof(column));

        _columns[column.Name] = column;
        return this;
    }

    /// <inheritdoc />
    public IDbTableSchema RemoveColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return this;

        _columns.Remove(name);
        return this;
    }

    /// <summary>
    /// Loads table schema information from a database connection.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional schema name.</param>
    /// <returns>A populated table schema.</returns>
    public static IDbTableSchema Load(DbConnection connection, string tableName, string? schema)
    {
        connection.EnsureConnected();
        var provider = connection.Provider();
        schema ??= provider.DefaultSchemaName;
        using var schemaCommand = connection.BeginCommandText()
            .Select().Fields().From(tableName, schema).Where("1 = 2").EndCommandText();

        using var schemaReader = schemaCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
        using var schemaTable = schemaReader.GetSchemaTable();

        if (schemaTable is null)
            throw new InvalidOperationException("Could not retrieve table schema.");

        var columnInstance = provider.ColumnSchemaFactory.Invoke();
        var columnType = columnInstance.GetType().TypeInfo();
        var columns = schemaTable.Query(r => DeserializeColumn(r, columnType)).ToList();
        return new DbTableSchema(connection.Database, tableName, schema, columns);
    }

    /// <summary>
    /// Loads table schema information from a database connection.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional schema name.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A populated table schema.</returns>
    public static async Task<IDbTableSchema> LoadAsync(DbConnection connection, string tableName, string? schema, CancellationToken ct = default)
    {
        connection.EnsureConnected();
        var provider = connection.Provider();
        schema ??= provider.DefaultSchemaName;
        await using var schemaCommand = connection.BeginCommandText()
            .Select().Fields().From(tableName, schema).Where("1 = 2").EndCommandText();

        await using var schemaReader = await schemaCommand.ExecuteReaderAsync(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo, ct);
        using var schemaTable = await schemaReader.GetSchemaTableAsync(ct);

        if (schemaTable is null)
            throw new InvalidOperationException("Could not retrieve table schema.");

        var columnInstance = provider.ColumnSchemaFactory.Invoke();
        var columnType = columnInstance.GetType().TypeInfo();
        var columns = schemaTable.Query(r => DeserializeColumn(r, columnType)).ToList();
        return new DbTableSchema(connection.Database, tableName, schema, columns);
    }

    private static IDbColumnSchema DeserializeColumn(DataRow columnInfo, ITypeInfo columnType) =>
        (columnInfo.ParseObject(columnType.NativeType, columnType.CreateInstance) as IDbColumnSchema)!;
}
