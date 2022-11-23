namespace Swan.Data;

/// <summary>
/// Represents a mapper that is able to resolve types between
/// a database provider and CLR types.
/// </summary>
public interface IDbTypeMapper
{
    /// <summary>
    /// Gets a list of supported types for this type mapper.
    /// </summary>
    IReadOnlyList<Type> SupportedTypes { get; }

    /// <summary>
    /// Tries to obtain an equivalent <see cref="DbType"/> for the given CLR type.
    /// </summary>
    /// <param name="type">The CLR type.</param>
    /// <param name="dbType">The <see cref="DbType"/>.</param>
    /// <returns>True if the type is found, false otherwise.</returns>
    bool TryGetProviderTypeFor(Type type, out DbType? dbType);

    /// <summary>
    /// Tries to obtain a provider-specific type expressed as a DDL string.
    /// This method does not take into account precision, scale, or maximum length.
    /// </summary>
    /// <param name="type">The CLR type.</param>
    /// <param name="providerType">The provider-specific data type.</param>
    /// <returns>True if the type is found, false otherwise.</returns>
    bool TryGetDatabaseTypeFor(Type type, [MaybeNullWhen(false)] out string providerType);

    /// <summary>
    /// Tries to obtain a provider-specific column type expressed as a DDL string.
    /// This method takes into account precision, scale, or maximum length.
    /// </summary>
    /// <param name="column">The CLR type.</param>
    /// <param name="providerType">The provider-specific data type.</param>
    /// <returns>True if the type is found, false otherwise.</returns>
    bool TryGetProviderTypeFor(IDbColumnSchema column, [MaybeNullWhen(false)] out string providerType);
}

