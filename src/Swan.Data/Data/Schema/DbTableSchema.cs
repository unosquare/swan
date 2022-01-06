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
    public DbTableSchema(DbProvider provider, string tableName, string schema, IEnumerable<IDbColumnSchema>? columns = default)
    {
        if (provider is null)
            throw new ArgumentNullException(nameof(provider));

        if (tableName is null)
            throw new ArgumentNullException(nameof(tableName));

        if (schema is null)
            throw new ArgumentNullException(nameof(schema));

        Provider = provider;
        Database = provider.Database;
        TableName = tableName;
        Schema = schema;

        if (columns is null)
            return;

        foreach (var column in columns)
            _columns[column.Name] = column;
    }

    /// <inheritdoc />
    public IDbColumnSchema? this[string name] => _columns.TryGetValue(name, out var column) ? column : null;

    /// <inheritdoc />
    public DbProvider Provider { get; }

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
    public void AddColumn(IDbColumnSchema column)
    {
        if (column is null)
            throw new ArgumentNullException(nameof(column));

        if (string.IsNullOrWhiteSpace(column.Name))
            throw new ArgumentException("The column name must be specified.", nameof(column));

        _columns[column.Name] = column;
    }

    /// <inheritdoc />
    public void RemoveColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        _columns.Remove(name);
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
        var provider = connection.Provider();
        schema ??= provider.DefaultSchemaName;
        using var schemaCommand = connection.BeginCommandText()
                .Select().Fields().From(tableName, schema).Where("1 = 2").EndCommandText();

        using var schemaReader = schemaCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
        using var schemaTable = schemaReader.GetSchemaTable();

        if (schemaTable == null)
            throw new InvalidOperationException("Could not retrieve table schema.");

        var columns = schemaTable.Query(provider.DbColumnType).Cast<IDbColumnSchema>().ToList();
        return new DbTableSchema(provider, tableName, schema, columns);
    }
}
