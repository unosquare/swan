namespace Swan.Data.Schema;

/// <summary>
/// Represents table structure information from the backing data store.
/// While it uses a connection object to retrieve a schema, it does not store
/// store a reference to a connection.
/// </summary>
public sealed class DbTableSchema : IDbTable
{
    private static readonly ValueCache<int, DbTableSchema> Cache = new();
    private readonly Dictionary<string, IDbColumn> _columns = new(128);

    /// <summary>
    /// Creates a new instance of the <see cref="DbTableSchema"/> class.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="schema">The schema name.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private DbTableSchema(IDbConnection connection, string tableName, string schema)
    {
        Provider = connection.Provider();
        Database = connection.Database;
        TableName = tableName;
        Schema = schema;

        using var schemaCommand = connection.BeginCommandText()
            .Select().Fields().From(TableName, Schema).Where("1 = 2").EndCommandText();

        using var schemaReader = schemaCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
        using var schemaTable = schemaReader.GetSchemaTable();

        if (schemaTable == null)
            throw new InvalidOperationException("Could not retrieve table schema.");

        foreach (IDbColumn column in schemaTable.Query(Provider.DbColumnType))
            _columns[column.Name] = column;

        CacheKey = Library.ComputeCacheKey(Provider, TableName, Schema);
    }

    /// <summary>
    /// Gets the ssociated databse provider.
    /// </summary>
    public DbProvider Provider { get; }

    /// <summary>
    /// Gets the database name (catalog) this table belongs to.
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Gets the schema name this table belongs to. Returns
    /// an empty string if provider does not support schemas.
    /// </summary>
    public string Schema { get; }

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// Gets the list of columns contained in this table.
    /// </summary>
    public IReadOnlyList<IDbColumn> Columns => _columns.Values.ToArray();

    /// <summary>
    /// Gets the key that uniquely identifies this table.
    /// </summary>
    internal int CacheKey { get; }

    /// <summary>
    /// If not existing, creates a new table schema, otherwise, retrieves it from cache.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional schema name.</param>
    /// <returns>A populated table schema.</returns>
    internal static DbTableSchema FromConnection(IDbConnection connection, string tableName, string? schema = default)
    {
        var provider = connection.Provider();
        schema ??= provider.DefaultSchemaName;
        var cacheKey = Library.ComputeCacheKey(provider, tableName, schema);
        return Cache.GetValue(cacheKey, () => new(connection, tableName, schema));
    }
}

