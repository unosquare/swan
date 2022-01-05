namespace Swan.Data;

/// <summary>
/// Provides connection-specific metadata useful
/// in constructing commands.
/// </summary>
public sealed record DbProvider
{
    private static readonly ValueCache<int, DbProvider> _Cache = new();
    private readonly Lazy<Library.AddWithValueDelegate?> LazyAddWithValue;

    private DbProvider(IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var factory = connection is DbConnection conn
            ? DbProviderFactories.GetFactory(conn)
            : default;

        if (factory is null)
        {
            throw new ArgumentException(
                $"Could not obtain {nameof(DbProviderFactory)} from connection type '{connection}'.",
                nameof(connection));
        }

        ConnectionType = connection.GetType();
        var typeName = ConnectionType.FullName;
        Kind = string.IsNullOrWhiteSpace(typeName)
            ? ProviderKind.Unknown
            : typeName.StartsWith("System.Data.SqlClient", StringComparison.Ordinal)
            ? ProviderKind.SqlServer
            : typeName.StartsWith("MySql.Data.MySqlClient", StringComparison.Ordinal)
            ? ProviderKind.MySql
            : typeName.StartsWith("Microsoft.Data.Sqlite", StringComparison.Ordinal)
            ? ProviderKind.Sqlite
            : ProviderKind.Unknown;

        DefaultSchemaName = Kind == ProviderKind.SqlServer ? "dbo" : string.Empty;

        if (!factory.CanCreateCommandBuilder)
        {
            QuotePrefix = "[";
            QuoteSuffix = "]";
            SchemaSeparator = ".";
        }
        else
        {
            using var builder = factory.CreateCommandBuilder();
            if (builder is null)
            {
                throw new ArgumentException(
                    $"Could not create a {nameof(DbCommandBuilder)} from connection.",
                    nameof(connection));
            }

            QuotePrefix = builder.QuotePrefix;
            QuoteSuffix = builder.QuoteSuffix;
            SchemaSeparator = builder.SchemaSeparator;
        }

        ParameterPrefix = Kind switch
        {
            ProviderKind.Sqlite => "$",
            _ => "@",
        };

        CacheKey = connection.ComputeCacheKey();
        Database = connection.Database;
        TypeMapper = DbTypeMapper.Default;

        using var dummyCommand = connection.CreateCommand();
        var dummyParametersType = dummyCommand.Parameters.GetType().TypeInfo();
        LazyAddWithValue = new(() => Library.GetAddWithValueMethod(dummyParametersType), true);
    }

    /// <summary>
    /// Gets the undrelying ADO.NET connection type.
    /// </summary>
    public Type ConnectionType { get; }

    /// <summary>
    /// Gets the translator between CLR types and DbTypes.
    /// </summary>
    public DbTypeMapper TypeMapper { get; }

    /// <summary>
    /// Gets the SQL dialect that is used to issue commands.
    /// </summary>
    public ProviderKind Kind { get; }

    /// <summary>
    /// Gets the database name from the connection that was used
    /// to build this object.
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Gets the prefix used to quote identifiers.
    /// </summary>
    public string QuotePrefix { get; }

    /// <summary>
    /// Gets the suffix used to quote identifiers.
    /// </summary>
    public string QuoteSuffix { get; }

    /// <summary>
    /// Gets the separator that goes between the schema and the table.
    /// </summary>
    public string SchemaSeparator { get; }

    /// <summary>
    /// Gets the prefix used to name parameters on commands.
    /// </summary>
    public string ParameterPrefix { get; }

    /// <summary>
    /// Gets the default schema name. For example in SQL Server, it will return dbo.
    /// Will return an empty string if no default schema is known.
    /// </summary>
    public string DefaultSchemaName { get; }

    /// <summary>
    /// Gets a default configuration for command timout when
    /// creating commands via this API. Default is 60 seconds.
    /// </summary>
    public TimeSpan DefaultCommandTimeout { get; private set; } = TimeSpan.FromSeconds(60);

    internal Type DbColumnType => Kind == ProviderKind.SqlServer
        ? typeof(SqlServerColumn)
        : Kind == ProviderKind.Sqlite
        ? typeof(SqliteColumn)
        : throw new NotSupportedException();

    /// <summary>
    /// If supported, provides the Parameters.AddWithValue delegate to call when
    /// setting parameters.
    /// </summary>
    internal Library.AddWithValueDelegate? AddWithValueMethod => LazyAddWithValue.Value;

    /// <summary>
    /// Fluet API for setting the default timeout for commands that are
    /// created via this API.
    /// </summary>
    /// <param name="timeout">The timeout to use.</param>
    /// <returns>This object for fluent API compatibility.</returns>
    public DbProvider WithDefaultCommandTimeout(TimeSpan timeout)
    {
        DefaultCommandTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Gets the internal identifier for caching purposes.
    /// </summary>
    internal int CacheKey { get; }

    internal static DbProvider FromConnection(IDbConnection connection)
    {
        return connection is null
            ? throw new ArgumentNullException(nameof(connection))
            : _Cache.GetValue(connection.ComputeCacheKey(), () => new DbProvider(connection));
    }

}
