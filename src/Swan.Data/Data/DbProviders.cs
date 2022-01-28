namespace Swan.Data;

/// <summary>
/// Provides a repository of specific data providers.
/// </summary>
public static class DbProviders
{
    private static readonly Dictionary<string, DbProvider> _cache = new(16, StringComparer.Ordinal);

    static DbProviders()
    {
        _cache["System.Data.SqlClient"] = new SqlServerDbProvider();
        _cache["Microsoft.Data.SqlClient"] = new SqlServerDbProvider();
        _cache["MySql.Data.MySqlClient"] = new MySqlDbProvider();
        _cache["Microsoft.Data.Sqlite"] = new SqliteDbProvider();
    }

    public static bool TryGetProvider(Type connectionType, [MaybeNullWhen(false)] out DbProvider provider)
    {
        provider = null;

        if (connectionType is null)
            return false;

        if (!connectionType.TypeInfo().Interfaces.Any(c => c == typeof(IDbConnection)))
            return false;

        var providerNs = connectionType.Namespace ?? string.Empty;
        return _cache.TryGetValue(providerNs, out provider);
    }

    public static bool TryGetProvider<T>(this T connection, [MaybeNullWhen(false)] out DbProvider provider)
        where T : IDbConnection
    {
        provider = null;
        if (connection is null)
            return false;

        return TryGetProvider(connection.GetType(), out provider);
    }

    public static void RegisterProvider<T>(DbProvider provider)
        where T : IDbConnection
    {
        if (provider is null)
            throw new ArgumentNullException(nameof(provider));

        var providerNs = typeof(T).Namespace;
        if (string.IsNullOrWhiteSpace(providerNs))
            throw new ArgumentException($"No namespace found for type {typeof(T)}", nameof(T));

        _cache[providerNs] = provider;
    }
}

