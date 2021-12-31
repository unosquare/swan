namespace Swan.Data;

using System.Linq.Expressions;

/// <summary>
/// Provides connection-specific metadata useful
/// in constructing commands.
/// </summary>
public sealed record DbProvider
{
    internal delegate IDbDataParameter AddWithValueDelegate(IDataParameterCollection collection, string name, object value);

    private const string AddWithValueMethodName = "AddWithValue";
    private static readonly Type[] AddWithValueArgumentTypes = new Type[] { typeof(string), typeof(object) };
    private static readonly object CacheLock = new();
    private static readonly Dictionary<int, DbProvider> _Cache = new(4);

    private readonly Lazy<AddWithValueDelegate?> LazyAddWithValue;

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

        CacheKey = ComputeCacheKey(connection);
        Database = connection.Database;
        TypeMapper = DbTypeMapper.Default;

        using var dummyCommand = connection.CreateCommand();
        var dummyParametersType = dummyCommand.Parameters.GetType().TypeInfo();
        LazyAddWithValue = new(() => GetAddWithValueMethod(dummyParametersType), true);
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

    /// <summary>
    /// If supported, provides the Parameters.AddWithValue delegate to call when
    /// setting parameters.
    /// </summary>
    internal AddWithValueDelegate? AddWithValueMethod => LazyAddWithValue.Value;

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

    /// <summary>
    /// Computes a hash code that serves as a cache identifier for all
    /// connections matching the initial connection string, database, and connection type.
    /// </summary>
    /// <param name="connection">The connection to derive the hash code from.</param>
    /// <returns>A unique id for matching connections.</returns>
    /// <exception cref="ArgumentNullException">Connection cannot be null.</exception>
    internal static int ComputeCacheKey(IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        connection.EnsureIsValid();
        var hashA = connection.ConnectionString.GetHashCode(StringComparison.Ordinal);
        var hashB = connection.Database.GetHashCode(StringComparison.Ordinal);
        var hashC = connection.GetType().GetHashCode();

        return HashCode.Combine(hashA, hashB, hashC);
    }

    internal string QuoteTable(string tableName, string? schemaName = default) =>
        !string.IsNullOrWhiteSpace(schemaName)
            ? string.Join(string.Empty,
                QuotePrefix,
                schemaName,
                QuoteSuffix,
                SchemaSeparator,
                QuotePrefix,
                tableName,
                QuoteSuffix)
            : $"{QuotePrefix}{tableName}{QuoteSuffix}";

    internal string QuoteField(string fieldName) =>
        $"{QuotePrefix}{fieldName}{QuoteSuffix}";

    /// <summary>
    /// Adds the provider-specific parameter prefix to the specified parameter name.
    /// If the specified name already contains the parameter prefix, it simply returns
    /// the trimmed name.
    /// </summary>
    /// <param name="name">The name to add the parameter prefix to.</param>
    /// <returns>The quoted parameter name.</returns>
    internal string QuoteParameter(string name) =>
        !string.IsNullOrWhiteSpace(ParameterPrefix) && name.StartsWith(ParameterPrefix, StringComparison.Ordinal)
            ? name.Trim()
            : $"{ParameterPrefix}{name.Trim()}";

    /// <summary>
    /// Removes the provider-specific parameter prefix from the specified parameter name.
    /// If the specified parameter name does not contain a parameter prefix, it simply returns
    /// the trimmed name.
    /// </summary>
    /// <param name="name">The name to remove the parameter prefix from.</param>
    /// <returns>The unquoted parameter name.</returns>
    internal string UnquoteParameter(string name) =>
        !string.IsNullOrWhiteSpace(ParameterPrefix) && name.StartsWith(ParameterPrefix, StringComparison.Ordinal)
            ? new string(name.AsSpan()[ParameterPrefix.Length..]).Trim()
            : name.Trim();

    internal static DbProvider FromConnection(IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var cacheKey = ComputeCacheKey(connection);
        lock (CacheLock)
        {
            if (_Cache.TryGetValue(cacheKey, out var metadata))
                return metadata;

            metadata = new DbProvider(connection);
            _Cache[cacheKey] = metadata;
            return metadata;
        }
    }

    private static AddWithValueDelegate? GetAddWithValueMethod(ITypeInfo? collectionType)
    {
        if (collectionType is null)
            return null;

        if (!collectionType.TryFindPublicMethod(AddWithValueMethodName, AddWithValueArgumentTypes, out var addWithValueMethod))
            return null;

        try
        {
            var targetParameter = Expression.Parameter(typeof(IDataParameterCollection), "target");
            var nameParameter = Expression.Parameter(typeof(string), "name");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var expressionBody = Expression.Convert(
                Expression.Call(Expression.Convert(targetParameter, collectionType.NativeType),
                    addWithValueMethod, nameParameter, valueParameter), typeof(IDbDataParameter));

            return Expression
                .Lambda<AddWithValueDelegate>(expressionBody,targetParameter, nameParameter, valueParameter)
                .Compile();
        }
        catch
        {
            return null;
        }
    }
}
