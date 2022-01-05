namespace Swan.Data;

/// <summary>
/// Represents table structure information bound to a particular connection
/// and from which you can issue table specific CRUD commands.
/// </summary>
internal class TableContext : ITableContext
{
    private static readonly ValueCache<int, IDbTable> SchemaCache = new();
    private readonly IDbTable TableSchema;

    /// <summary>
    /// Creates a new instance of the <see cref="TableContext"/> class.
    /// </summary>
    /// <param name="connection">The connection to associate this context to.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The name of the schema.</param>
    public TableContext(IDbConnection connection, string tableName, string? schema = null)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        TableSchema = LoadTableSchema(connection, tableName, schema);
        Connection = connection;
    }

    /// <inheritdoc />
    public IDbColumn? this[string name] => TableSchema[name];

    /// <inheritdoc />
    public IDbConnection Connection { get; }

    /// <inheritdoc />
    public DbProvider Provider => TableSchema.Provider;

    /// <inheritdoc />
    public string Database => TableSchema.Database;

    /// <inheritdoc />
    public string Schema => TableSchema.Schema;

    /// <inheritdoc />
    public string TableName => TableSchema.TableName;

    /// <inheritdoc />
    public IReadOnlyList<IDbColumn> Columns => TableSchema.Columns;

    /// <inheritdoc />
    public void AddColumn(IDbColumn column) => TableSchema.AddColumn(column);

    /// <inheritdoc />
    public void RemoveColumn(string column) => TableSchema.RemoveColumn(column);

    private static IDbTable LoadTableSchema(IDbConnection connection, string tableName, string? schema)
    {
        var provider = connection.Provider();
        schema ??= provider.DefaultSchemaName;
        var cacheKey = Library.ComputeTableCacheKey(provider, tableName, schema);
        return SchemaCache.GetValue(cacheKey, () => DbTableSchema.Load(connection, tableName, schema));
    }
}
