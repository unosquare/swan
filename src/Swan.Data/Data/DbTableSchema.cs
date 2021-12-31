namespace Swan.Data;

public class DbTableSchema
{
    private static readonly object CacheLock = new();
    private static readonly Dictionary<int, DbTableSchema> Cache = new();

    private readonly Dictionary<string, IDbColumn> _columns = new(128);

    private DbTableSchema(IDbConnection connection, string tableName, string schema)
    {
        Provider = connection.Provider();
        Database = connection.Database;
        TableName = tableName;
        Schema = schema;

        using var schemaCommand = connection.StartCommand()
            .Select().Fields().From(TableName, Schema).Where("1 = 2").FinishCommand();

        using var schemaReader = schemaCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
        using var schemaTable = schemaReader.GetSchemaTable();

        if (schemaTable == null)
            throw new InvalidOperationException("Could not retrieve table schema.");

        foreach (IDbColumn row in schemaTable.Query<SqlServerColumn>())
        {
            _columns[row.Name] = row;
        }

        CacheKey = ComputeCacheKey(Provider, TableName, Schema);
    }

    public DbProvider Provider { get; }

    public string Database { get; }

    public string Schema { get; }

    public string TableName { get; }

    internal int CacheKey { get; }

    internal static int ComputeCacheKey(DbProvider provider, string tableName, string schema) =>
        HashCode.Combine(provider.CacheKey, tableName, schema);

    public IReadOnlyList<IDbColumn> Columns => _columns.Values.ToArray();

    internal static DbTableSchema FromConnection(IDbConnection connection, string tableName, string? schema = default)
    {
        lock (CacheLock)
        {
            var provider = connection.Provider();
            schema ??= provider.DefaultSchemaName;
            var cacheKey = ComputeCacheKey(provider, tableName, schema);
            if (Cache.TryGetValue(cacheKey, out var dbTable))
                return dbTable;

            dbTable = new(connection, tableName, schema);
            Cache[cacheKey] = dbTable;
            return dbTable;
        }
    }
}

