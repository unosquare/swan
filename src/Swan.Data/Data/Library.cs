namespace Swan.Data;

using System.Linq.Expressions;

/// <summary>
/// Provides utility methods visibly to this library only.
/// </summary>
internal static partial class Library
{
    public delegate IDbDataParameter AddWithValueDelegate(IDataParameterCollection collection, string name, object value);

    public const string CommandConnectionErrorMessage = $"The {nameof(IDbCommand)}.{nameof(IDbCommand.Connection)} cannot be null.";
    private const string AddWithValueMethodName = "AddWithValue";
    private static readonly Type[] AddWithValueArgumentTypes = new Type[] { typeof(string), typeof(object) };

    /// <summary>
    /// Removes special characters that cannot be represented as property names such as spaces.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="fieldIndex">The index appearance of the field.</param>
    /// <returns>A valid property name with only letters, digits or underscores.</returns>
    public static string ToExpandoPropertyName(this string fieldName, int fieldIndex)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            fieldName = $"Field_{fieldIndex.ToString(CultureInfo.InvariantCulture)}";
        
        var builder = new StringBuilder(fieldName.Length);
        foreach (var c in fieldName)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                continue;

            builder.Append(c);
        }

        return char.IsDigit(builder[0])
            ? $"_{builder}"
            : builder.ToString();
    }

    /// <summary>
    /// Attempts to execute the reader with sequential access and single result, in addition to user-provided
    /// required falgs. If execution fails, it then tries to execute the reader without optimization flags.
    /// </summary>
    /// <param name="command">The command to execute the reader.</param>
    /// <param name="requiredFlags">The required behavior flags.</param>
    /// <returns>The data reader resulting from command execution.</returns>
    public static IDataReader ExecuteOptimizedReader(this IDbCommand command, CommandBehavior requiredFlags = CommandBehavior.Default)
    {
        const CommandBehavior OptimizedBahavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult;
        IDataReader? reader = null;

        try
        {
            reader = command.ExecuteReader(OptimizedBahavior | requiredFlags);
            return reader;
        }
        catch (ArgumentException)
        {
            reader = command.ExecuteReader(requiredFlags);
            return reader;
        }
    }

    /// <summary>
    /// Attempts to rertieve the AddWithValue method on a type derived from
    /// <see cref="IDataParameterCollection"/> and compiles a generic lambda
    /// expression to call it regardless of the concrete type being used in
    /// the provider.
    /// </summary>
    /// <param name="collectionType">The concrete type info of <see cref="IDataParameterCollection"/></param>
    /// <returns>If successful, a compiled delegate that can be called on the provider.</returns>
    public static AddWithValueDelegate? GetAddWithValueMethod(ITypeInfo? collectionType)
    {
        if (collectionType is null)
            return null;

        if (!collectionType.TryFindPublicMethod(AddWithValueMethodName,
            AddWithValueArgumentTypes, out var addWithValueMethod))
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
                .Lambda<Library.AddWithValueDelegate>(
                    expressionBody, targetParameter, nameParameter, valueParameter)
                .Compile();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Adds quotes around a table name along with an optional schema name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>A quoted table name.</returns>
    public static string QuoteTable(this DbProvider provider, string tableName, string? schemaName = default) =>
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
    public static string QuoteField(this DbProvider provider, string fieldName) =>
        $"{provider.QuotePrefix}{fieldName}{provider.QuoteSuffix}";

    /// <summary>
    /// Adds the provider-specific parameter prefix to the specified parameter name.
    /// If the specified name already contains the parameter prefix, it simply returns
    /// the trimmed name.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="name">The name to add the parameter prefix to.</param>
    /// <returns>The quoted parameter name.</returns>
    public static string QuoteParameter(this DbProvider provider, string name) =>
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
    public static string UnquoteParameter(this DbProvider provider, string name) =>
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
    public static int ComputeCacheKey(DbProvider provider, string tableName, string schema) =>
        HashCode.Combine(provider.CacheKey, tableName, schema);

    /// <summary>
    /// Computes an integer representing a hash code for the connection,
    /// taking into account the connection string, the database, and the type of the
    /// connection.
    /// </summary>
    /// <param name="connection">The connection to compute the hash key for.</param>
    /// <returns>A hash code representing a cache entry id.</returns>
    public static int ComputeCacheKey(this IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        connection.EnsureIsValid();
        var hashA = connection.ConnectionString.GetHashCode(StringComparison.Ordinal);
        var hashB = connection.Database.GetHashCode(StringComparison.Ordinal);
        var hashC = connection.GetType().GetHashCode();

        return HashCode.Combine(hashA, hashB, hashC);
    }
}

