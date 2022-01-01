namespace Swan.Data.Extensions;

/// <summary>
/// Provides utility methods visibly to this library only.
/// </summary>
internal static partial class InternalExtensions
{
    public const string CommandConnectionErrorMessage = $"The {nameof(IDbCommand)}.{nameof(IDbCommand.Connection)} cannot be null.";

    /// <summary>
    /// Removes special characters that cannot be represented as property names such as spaces.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="fieldIndex">The index appearance of the field.</param>
    /// <returns>A valid property name with only letters, digits or underscores.</returns>
    public static string ToExpandoPropertyName(this string fieldName, int fieldIndex)
    {
        fieldName ??= $"Field_{fieldIndex.ToString(CultureInfo.InvariantCulture)}";
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

    public static int ComputeCacheKey(DbProvider provider, string tableName, string schema) =>
        HashCode.Combine(provider.CacheKey, tableName, schema);

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

