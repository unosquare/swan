namespace Swan.Data.Context;

public partial class TableContext
{
	private static readonly ValueCache<int, IDbTableSchema> SchemaCache = new();
	private readonly IDbTableSchema TableSchema;

    /// <inheritdoc />
    public IDbColumnSchema? this[string name] => TableSchema[name];

    /// <inheritdoc />
    public DbConnection Connection { get; }

    /// <inheritdoc />
    public DbProvider Provider { get; }

    /// <inheritdoc />
    public string Database => TableSchema.Database;

    /// <inheritdoc />
    public string Schema => TableSchema.Schema;

    /// <inheritdoc />
    public string TableName => TableSchema.TableName;

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> Columns => TableSchema.Columns;

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> KeyColumns => TableSchema.KeyColumns;

    /// <inheritdoc />
    public IDbColumnSchema? IdentityKeyColumn => TableSchema.IdentityKeyColumn;

    /// <inheritdoc />
    public bool HasKeyIdentityColumn => TableSchema.HasKeyIdentityColumn;

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> InsertableColumns => TableSchema.InsertableColumns;

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> UpdateableColumns => TableSchema.UpdateableColumns;

    /// <inheritdoc />
    public IDbTableSchema AddColumn(IDbColumnSchema column) => TableSchema.AddColumn(column);

    /// <inheritdoc />
    public IDbTableSchema RemoveColumn(string columnName) => TableSchema.RemoveColumn(columnName);

    /// <summary>
    /// Retrieves the table schema information from the database. If the schema
    /// has been previously retrieved, then it simply returns it from cache.
    /// </summary>
    /// <param name="connection">The connection to retrieve the schema from.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional table schema.</param>
    /// <returns>The table schema.</returns>
    public static IDbTableSchema CacheLoadTableSchema(DbConnection connection, string tableName, string? schema)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        var provider = connection.Provider();
        if (string.IsNullOrWhiteSpace(schema))
            schema = provider.DefaultSchemaName;

        connection.EnsureConnected();
        var database = connection.Database;
        var cacheKey = ComputeTableCacheKey(provider, database, tableName, schema);
        return SchemaCache.GetValue(cacheKey, () => DbTableSchema.Load(connection, tableName, schema));
    }

    /// <summary>
    /// Retrieves the table schema information from the database. If the schema
    /// has been previously retrieved, then it simply returns it from cache.
    /// </summary>
    /// <param name="connection">The connection to retrieve the schema from.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional table schema.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The table schema.</returns>
    public static async Task<IDbTableSchema> CacheLoadTableSchemaAsync(DbConnection connection, string tableName, string? schema, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        var provider = connection.Provider();
        if (string.IsNullOrWhiteSpace(schema))
            schema = provider.DefaultSchemaName;

        await connection.EnsureConnectedAsync(ct);
        var database = connection.Database;
        var cacheKey = ComputeTableCacheKey(provider, database, tableName, schema);
        return await SchemaCache.GetValueAsync(cacheKey, () => DbTableSchema.LoadAsync(connection, tableName, schema, ct));
    }

    /// <summary>
    /// Computes an integer representing a hash code for a table schema.
    /// </summary>
    /// <param name="provider">The associated provider.</param>
    /// <param name="database">The database name.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <returns>A hash code representing a cache entry id.</returns>
    private static int ComputeTableCacheKey(DbProvider provider, string database, string tableName, string schema) =>
        HashCode.Combine(
            provider.GetType(),
            database.Trim().ToUpperInvariant(),
            tableName.Trim().ToUpperInvariant(),
            schema.Trim().ToUpperInvariant());
}

