namespace Swan.Data;

/// <summary>
/// Provides connection-specific metadata useful
/// in constructing commands.
/// </summary>
public record ProviderMetadata
{
    private static readonly object CacheLock = new();
    private static readonly Dictionary<int, ProviderMetadata> _Cache = new(4);

    private ProviderMetadata(DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var factory = DbProviderFactories.GetFactory(connection);

        if (factory is null)
        {
            throw new ArgumentException(
                $"Could not obtain {nameof(DbProviderFactory)} from connection type '{connection}'.",
                nameof(connection));
        }

        ProviderAssembly = factory.GetType().Assembly;
        var assemblyName = ProviderAssembly.GetName().Name;
        Kind = string.IsNullOrWhiteSpace(assemblyName)
            ? ProviderKind.Unknown
            : assemblyName.Contains("System.Data.SqlClient", StringComparison.Ordinal)
            ? ProviderKind.SqlServer
            : assemblyName.Contains("MySql.Data.MySqlClient", StringComparison.Ordinal)
            ? ProviderKind.MySql
            : assemblyName.Contains("Microsoft.Data.Sqlite", StringComparison.Ordinal)
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

        CacheKey = ComputeCacheKey(connection);
        Database = connection.Database;
    }

    /// <summary>
    /// Gets the ADO.NET standard provider assembly.
    /// </summary>
    public Assembly ProviderAssembly { get; }

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
    /// Gets the internal identifier for caching purposes.
    /// </summary>
    internal int CacheKey { get; }

    /// <summary>
    /// Computes a hash code that serves as a cache identifier for all
    /// connections matching the initial connection string, database, and connection type.
    /// </summary>
    /// <param name="connection">The connection to derive the hash code from.</param>
    /// <returns>A unique id for matching connections.</returns>
    /// <exception cref="ArgumentNullException">Connection cannot be null.</exception>
    internal static int ComputeCacheKey(DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        connection.EnsureIsValid();
        var hashA = connection.ConnectionString.GetHashCode(StringComparison.Ordinal);
        var hashB = connection.Database.GetHashCode(StringComparison.Ordinal);
        var hashC = connection.GetType().GetHashCode();

        return HashCode.Combine(hashA, hashB, hashC);
    }

    internal string Quote(TableMetadata table) =>
        !string.IsNullOrWhiteSpace(table.Schema)
            ? string.Join(string.Empty,
                QuotePrefix,
                table.Schema,
                QuoteSuffix,
                SchemaSeparator,
                QuotePrefix,
                table.TableName,
                QuoteSuffix)
            : $"{QuotePrefix}{table.TableName}{QuoteSuffix}";

    internal static ProviderMetadata FromConnection(DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var cacheKey = ComputeCacheKey(connection);
        lock (CacheLock)
        {
            if (_Cache.TryGetValue(cacheKey, out var metadata))
                return metadata;

            metadata = new ProviderMetadata(connection);
            _Cache[cacheKey] = metadata;
            return metadata;
        }
    }
}
