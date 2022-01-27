namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="DbProvider"/> class.
/// </summary>
internal static class ProviderExtensions
{
    /// <summary>
    /// Adds quotes around a table name along with an optional schema name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>A quoted table name.</returns>
    internal static string QuoteTable(this DbProvider provider, string tableName, string? schemaName = default) =>
        !string.IsNullOrWhiteSpace(schemaName)
            ? string.Join(string.Empty,
                provider.QuotePrefix,
                schemaName,
                provider.QuoteSuffix,
                provider.SchemaSeparator,
                provider.QuotePrefix,
                tableName,
                provider.QuoteSuffix)
            : $"{provider.QuotePrefix}{tableName}{provider.QuoteSuffix}";

    /// <summary>
    /// Adds quotes arounf a field or column name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>A quited field name.</returns>
    internal static string QuoteField(this DbProvider provider, string fieldName) =>
        $"{provider.QuotePrefix}{fieldName}{provider.QuoteSuffix}";

    /// <summary>
    /// Adds the provider-specific parameter prefix to the specified parameter name.
    /// If the specified name already contains the parameter prefix, it simply returns
    /// the trimmed name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="name">The name to add the parameter prefix to.</param>
    /// <returns>The quoted parameter name.</returns>
    internal static string QuoteParameter(this DbProvider provider, string name) =>
        !string.IsNullOrWhiteSpace(provider.ParameterPrefix) && name.StartsWith(provider.ParameterPrefix, StringComparison.Ordinal)
            ? name.Trim()
            : $"{provider.ParameterPrefix}{name.Trim()}";

    /// <summary>
    /// Removes the provider-specific parameter prefix from the specified parameter name.
    /// If the specified parameter name does not contain a parameter prefix, it simply returns
    /// the trimmed name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="name">The name to remove the parameter prefix from.</param>
    /// <returns>The unquoted parameter name.</returns>
    internal static string UnquoteParameter(this DbProvider provider, string name) =>
        !string.IsNullOrWhiteSpace(provider.ParameterPrefix) && name.StartsWith(provider.ParameterPrefix, StringComparison.Ordinal)
            ? new string(name.AsSpan()[provider.ParameterPrefix.Length..]).Trim()
            : name.Trim();

    /// <summary>
    /// Computes an integer representing a hash code for a table schema.
    /// </summary>
    /// <param name="provider">The associated provider.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <returns>A hash code representing a cache entry id.</returns>
    internal static int ComputeTableCacheKey(this DbProvider provider, string tableName, string schema) =>
        HashCode.Combine(provider.CacheKey, tableName, schema);

    /// <summary>
    /// Gets a column DDL definition for the specific provider. Useful for building CREATE TABLE commands.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="column">The column schema.</param>
    /// <returns>The DDL string that represents the column.</returns>
    internal static string? GetColumnDdlString(this DbProvider provider, IDbColumnSchema column)
    {
        if (!string.IsNullOrWhiteSpace(column.ProviderDataType))
            return $"{provider.QuoteField(column.Name),16} {column.ProviderDataType}{(!column.AllowsDBNull ? " NOT" : string.Empty)} NULL";

        if (!provider.TypeMapper.TryGetProviderTypeFor(column.DataType, out var providerType))
            return default;

        var isIdentity = !column.AllowsDBNull && column.IsAutoIncrement && column.DataType.TypeInfo().IsNumeric && column.IsKey;

        if (column.IsIdentity)
        {
            return provider.Kind switch
            {
                ProviderKind.MySql => $"{provider.QuoteField(column.Name),16} {providerType} NOT NULL AUTO_INCREMENT",
                ProviderKind.SqlServer => $"{provider.QuoteField(column.Name),16} {providerType} IDENTITY NOT NULL PRIMARY KEY",
                ProviderKind.Sqlite => $"{provider.QuoteField(column.Name),16} INTEGER PRIMARY KEY",
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

        return $"{provider.QuoteField(column.Name),16} {providerType}{(!column.AllowsDBNull ? " NOT" : string.Empty )} NULL";
    }
}

