namespace Swan.Data;

/// <summary>
/// Represents table structure information bound to a particular connection
/// and from which you can issue table specific CRUD commands.
/// </summary>
public class TableContext : IDbTable, IConnected
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

    /// <summary>
    /// Gets the ssociated databse provider.
    /// </summary>
    public DbProvider Provider => TableSchema.Provider;

    /// <summary>
    /// Gets the database name (catalog) this table belongs to.
    /// </summary>
    public string Database => TableSchema.Database;

    /// <summary>
    /// Gets the schema name this table belongs to. Returns
    /// an empty string if provider does not support schemas.
    /// </summary>
    public string Schema => TableSchema.Schema;

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public string TableName => TableSchema.TableName;

    /// <summary>
    /// Gets the list of columns contained in this table.
    /// </summary>
    public IReadOnlyList<IDbColumn> Columns => TableSchema.Columns;
}

