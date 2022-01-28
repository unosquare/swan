﻿namespace Swan.Data;

/// <summary>
/// Provides connection-specific metadata that is useful
/// when building commands or doing Type translation.
/// </summary>
public record DbProvider
{
    private static readonly ValueCache<int, DbProvider> _Cache = new();
    private readonly Lazy<Library.AddWithValueDelegate?> LazyAddWithValue;

    /// <summary>
    /// Creates a new instance of the <see cref="DbProvider"/> class.
    /// </summary>
    /// <param name="connection">The connection to crete the provider from.</param>
    private DbProvider(DbConnection connection)
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

        ConnectionType = connection.GetType();
        var typeName = ConnectionType.FullName;
        Kind = string.IsNullOrWhiteSpace(typeName)
            ? ProviderKind.Unknown
            : typeName.StartsWith("System.Data.SqlClient", StringComparison.Ordinal)
            ? ProviderKind.SqlServer
            : typeName.StartsWith("Microsoft.Data.SqlClient", StringComparison.Ordinal)
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
        TypeMapper = DbTypeMapper.GetDefault(Kind);

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
    public IDbTypeMapper TypeMapper { get; }

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
    /// Gets the internal identifier for caching purposes.
    /// </summary>
    internal int CacheKey { get; }

    /// <summary>
    /// Gets the deserialization type for chema table columns.
    /// </summary>
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
    /// Retrieves a the cached provider for the connection or create a new one if
    /// it does not yet exist. A provider is the same for connection with matching types and
    /// connection strings.
    /// </summary>
    /// <param name="connection">The connection to get the provider for.</param>
    /// <returns>The db provider.</returns>
    public static DbProvider FromConnection(DbConnection connection)
    {
        return connection is null
            ? throw new ArgumentNullException(nameof(connection))
            : _Cache.GetValue(connection.ComputeCacheKey(), () => new DbProvider(connection));
    }

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
    /// Adds quotes around a table name along with an optional schema name.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>A quoted table name.</returns>
    public virtual string QuoteTable(string tableName, string? schemaName = default) =>
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

    /// <summary>
    /// Adds quotes arounf a field or column name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>A quited field name.</returns>
    public virtual string QuoteField(string fieldName) =>
        $"{QuotePrefix}{fieldName}{QuoteSuffix}";

    /// <summary>
    /// Adds the provider-specific parameter prefix to the specified parameter name.
    /// If the specified name already contains the parameter prefix, it simply returns
    /// the trimmed name.
    /// </summary>
    /// <param name="name">The name to add the parameter prefix to.</param>
    /// <returns>The quoted parameter name.</returns>
    public virtual string QuoteParameter(string name) => string.IsNullOrWhiteSpace(name)
        ? throw new ArgumentNullException(nameof(name))
        : !string.IsNullOrWhiteSpace(ParameterPrefix) && name.StartsWith(ParameterPrefix, StringComparison.Ordinal)
        ? name.Trim()
        : $"{ParameterPrefix}{name.Trim()}";

    /// <summary>
    /// Removes the provider-specific parameter prefix from the specified parameter name.
    /// If the specified parameter name does not contain a parameter prefix, it simply returns
    /// the trimmed name.
    /// </summary>
    /// <param name="name">The name to remove the parameter prefix from.</param>
    /// <returns>The unquoted parameter name.</returns>
    public virtual string UnquoteParameter(string name) => string.IsNullOrWhiteSpace(name)
        ? throw new ArgumentNullException(nameof(name))
        : !string.IsNullOrWhiteSpace(ParameterPrefix) && name.StartsWith(ParameterPrefix, StringComparison.Ordinal)
        ? new string(name.AsSpan()[ParameterPrefix.Length..]).Trim()
        : name.Trim();

    /// <summary>
    /// Gets a column DDL definition for the specific provider. Useful for building CREATE TABLE commands.
    /// </summary>
    /// <param name="column">The column schema.</param>
    /// <returns>The DDL string that represents the column.</returns>
    public virtual string? GetColumnDdlString(IDbColumnSchema column)
    {
        if (column is null)
            throw new ArgumentNullException(nameof(column));

        if (!string.IsNullOrWhiteSpace(column.ProviderDataType))
            return $"{QuoteField(column.Name),16} {column.ProviderDataType}{(!column.AllowsDBNull ? " NOT" : string.Empty)} NULL";

        if (!TypeMapper.TryGetProviderTypeFor(column.DataType, out var providerType))
            return default;

        if (column.IsIdentity)
        {
            return Kind switch
            {
                ProviderKind.MySql => $"{QuoteField(column.Name),16} {providerType} NOT NULL AUTO_INCREMENT",
                ProviderKind.SqlServer => $"{QuoteField(column.Name),16} {providerType} IDENTITY NOT NULL PRIMARY KEY",
                ProviderKind.Sqlite => $"{QuoteField(column.Name),16} INTEGER PRIMARY KEY",
                _ => throw new NotSupportedException("Provider is not supported.")
            };
        }

        var hasLength = column.MaxLength > 0;
        var hasPrecision = column.Precision > 0;
        var hasScale = column.Scale > 0;
        var trimmedType = providerType!.Contains('(', StringComparison.Ordinal)
            ? providerType[..providerType.IndexOf('(', StringComparison.Ordinal)]
            : providerType;

        if (hasLength)
            providerType = $"{trimmedType}({column.MaxLength})";
        else if (hasPrecision && !hasScale)
            providerType = $"{trimmedType}({column.Precision})";
        else if (hasPrecision && hasScale)
            providerType = $"{trimmedType}({column.Precision}, {column.Scale})";

        return $"{QuoteField(column.Name),16} {providerType}{(!column.AllowsDBNull ? " NOT" : string.Empty)} NULL";
    }

    /// <summary>
    /// Gets the command text that provides a list of table names.
    /// </summary>
    /// <returns>The command text.</returns>
    internal string GetListTablesCommandText() => (Kind) switch
    {
        ProviderKind.SqlServer => "SELECT [TABLE_NAME] AS [Name], [TABLE_SCHEMA] AS [Schema] FROM [INFORMATION_SCHEMA].[TABLES]",
        ProviderKind.MySql => $"SELECT [table_name] AS [Name], '' AS [Schema] FROM [information_schema].[tables] WHERE [table_schema] = '{Database}'",
        ProviderKind.Sqlite => "SELECT name AS [Name], '' AS [Schema] FROM (SELECT * FROM sqlite_schema UNION ALL SELECT * FROM sqlite_temp_schema) WHERE type= 'table' ORDER BY name",
        _ => throw new NotSupportedException("Connection provider does not support retrieving table names.")
    };
}
