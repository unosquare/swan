namespace Swan.Data;

/// <summary>
/// Provides a repository of data providers.
/// </summary>
public static class DbProviders
{
    private static readonly Dictionary<string, DbProvider> _cache = new(16, StringComparer.Ordinal);

    /// <summary>
    /// Initializes the static members of the <see cref="DbProviders"/> class.
    /// </summary>
    static DbProviders()
    {
        _cache["System.Data.SqlClient"] = new SqlServerDbProvider();
        _cache["Microsoft.Data.SqlClient"] = new SqlServerDbProvider();
        _cache["MySql.Data.MySqlClient"] = new MySqlDbProvider();
        _cache["Microsoft.Data.Sqlite"] = new SqliteDbProvider();
    }

    /// <summary>
    /// Tries to obtain a registered provider for the connection type.
    /// </summary>
    /// <param name="connectionType">The connection type.</param>
    /// <param name="provider">The resulting provider.</param>
    /// <returns>True of the provider was previously registered and was retrieved. False otherwise.</returns>
    public static bool TryGetProvider(Type? connectionType, [MaybeNullWhen(false)] out DbProvider provider)
    {
        provider = null;

        if (connectionType is null)
            return false;

        if (connectionType.TypeInfo().Interfaces.All(c => c != typeof(IDbConnection)))
            return false;

        var providerNs = connectionType.Namespace ?? string.Empty;
        return _cache.TryGetValue(providerNs, out provider);
    }

    /// <summary>
    /// Tries to obtain a registered provider for the connection type.
    /// </summary>
    /// <typeparam name="T">The connection type.</typeparam>
    /// <param name="connection">The connection.</param>
    /// <param name="provider">The resulting provider.</param>
    /// <returns>True of the provider was previously registered and was retrieved. False otherwise.</returns>
    public static bool TryGetProvider<T>(this T? connection, [MaybeNullWhen(false)] out DbProvider provider)
        where T : IDbConnection
    {
        provider = null;
        return connection is not null && TryGetProvider(connection.GetType(), out provider);
    }

    /// <summary>
    /// Adds or updates a provider registration for the given connection type. 
    /// </summary>
    /// <typeparam name="T">The connection type.</typeparam>
    /// <param name="provider">The provider instance to register.</param>
    public static void RegisterProvider<T>(DbProvider provider)
        where T : class, IDbConnection
    {
        if (provider is null)
            throw new ArgumentNullException(nameof(provider));

        var providerNs = typeof(T).Namespace;
        if (string.IsNullOrWhiteSpace(providerNs))
            throw new ArgumentException($"No namespace found for type {typeof(T)}", nameof(T));

        _cache[providerNs] = provider;
    }
}

