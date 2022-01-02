namespace Swan.Data;

/// <summary>
/// Represents table structure information bound to a particular connection
/// and from which you can issue table specific CRUD commands.
/// </summary>
internal class TableContext : ITableContext
{
    private readonly DbTableSchema TableSchema;

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

        TableSchema = DbTableSchema.FromConnection(connection, tableName, schema);
        Connection = connection;
    }

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
}

