namespace Swan.Data;

/// <summary>
/// Provides connection-specific metadata for sq
/// </summary>
public record ProviderMetadata
{
    private static readonly object CacheLock = new();
    private static readonly Dictionary<Type, ProviderMetadata> _Cache = new(4);

    private ProviderMetadata(Type connectionType)
    {
        if (connectionType is null)
            throw new ArgumentNullException(nameof(connectionType));

        var assemblyName = connectionType.Assembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new ArgumentException("Unable to acquire assembly name for connection type.", nameof(connectionType));

        var factory = DbProviderFactories.GetFactory(assemblyName);

        if (factory is null)
        {
            throw new ArgumentException(
                $"Could not obtain {nameof(DbProviderFactory)} from connection type '{connectionType}'.",
                nameof(connectionType));
        }


        ProviderAssembly = factory.GetType().Assembly;
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
                    nameof(connectionType));
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

    internal static ProviderMetadata Acquire(Type connectionType)
    {
        if (connectionType is null)
            throw new ArgumentNullException(nameof(connectionType));

        lock (CacheLock)
        {
            if (_Cache.TryGetValue(connectionType, out var metadata))
                return metadata;

            metadata = new ProviderMetadata(connectionType);
            _Cache[connectionType] = metadata;
            return metadata;
        }
    }
}
