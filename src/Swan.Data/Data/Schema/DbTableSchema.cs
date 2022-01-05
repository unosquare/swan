namespace Swan.Data.Schema;

/// <summary>
/// Represents table structure information from the backing data store.
/// </summary>
internal sealed class DbTableSchema : IDbTable
{
    
    private readonly Dictionary<string, IDbColumn> _columns = new(128, StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Creates a new instance of the <see cref="DbTableSchema"/> class.
    /// </summary>
    internal DbTableSchema(DbProvider provider, string tableName, string schema, IEnumerable<IDbColumn>? columns = default)
    {
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
    public IDbColumn? this[string name] => _columns.TryGetValue(name, out var column) ? column : null;

    /// <inheritdoc />
    public DbProvider Provider { get; }

    /// <inheritdoc />
    public string Database { get; }

    /// <inheritdoc />
    public string Schema { get; }

    /// <inheritdoc />
    public string TableName { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDbColumn> Columns => _columns.Values.ToArray();

    /// <inheritdoc />
    public void AddColumn(IDbColumn column)
    {
        if (column is null)
            throw new ArgumentNullException(nameof(column));

        if (string.IsNullOrWhiteSpace(column.Name))
            throw new ArgumentException("The column name is mandatory.", nameof(column));

        _columns[column.Name] = column;
    }

    /// <inheritdoc />
    public void RemoveColumn(string name)
    {
        _columns.Remove(name);
    }

    /// <summary>
    /// Loads table schema information from a database connection.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional schema name.</param>
    /// <returns>A populated table schema.</returns>
    public static DbTableSchema Load(IDbConnection connection, string tableName, string? schema)
    {
        var provider = connection.Provider();
        schema ??= provider.DefaultSchemaName;
        using var schemaCommand = connection.BeginCommandText()
                .Select().Fields().From(tableName, schema).Where("1 = 2").EndCommandText();

        using var schemaReader = schemaCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
        using var schemaTable = schemaReader.GetSchemaTable();

        if (schemaTable == null)
            throw new InvalidOperationException("Could not retrieve table schema.");

        var columns = schemaTable.Query(provider.DbColumnType).Cast<IDbColumn>().ToList();
        return new DbTableSchema(provider, tableName, schema, columns);
    }
}
