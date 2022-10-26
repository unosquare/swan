namespace Swan.Data;

/// <summary>
/// Provides utility methods visibly to this library only.
/// </summary>
internal static class Library
{
    private const CommandBehavior OptimizedBahavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult;
    public delegate IDbDataParameter AddWithValueDelegate(IDataParameterCollection collection, string name, object value);

    public const string CommandConnectionErrorMessage = $"The {nameof(DbCommand)}.{nameof(DbCommand.Connection)} cannot be null.";
    public const string NoConnectionErrorMessage = $"The {nameof(DbCommandSource)} no longer contains a valid connection.";

    private const string AddWithValueMethodName = "AddWithValue";
    private static readonly Type[] AddWithValueArgumentTypes = new[] { typeof(string), typeof(object) };

    /// <summary>
    /// Attempts to execute the reader with sequential access and single result, in addition to user-provided
    /// required flags. If execution fails, it then tries to execute the reader without optimization flags.
    /// </summary>
    /// <param name="command">The command to execute the reader.</param>
    /// <param name="requiredFlags">The required behavior flags.</param>
    /// <returns>The data reader resulting from command execution.</returns>
    public static DbDataReader ExecuteOptimizedReader(this DbCommand command, CommandBehavior requiredFlags = CommandBehavior.Default)
    {
        try
        {
            var reader = command.ExecuteReader(OptimizedBahavior | requiredFlags);
            return reader;
        }
        catch (ArgumentException)
        {
            var reader = command.ExecuteReader(requiredFlags);
            return reader;
        }
    }

    /// <summary>
    /// Attempts to execute the reader with sequential access and single result, in addition to user-provided
    /// required flags. If execution fails, it then tries to execute the reader without optimization flags.
    /// </summary>
    /// <param name="command">The command to execute the reader.</param>
    /// <param name="requiredFlags">The required behavior flags.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The data reader resulting from command execution.</returns>
    public static async Task<DbDataReader> ExecuteOptimizedReaderAsync(this DbCommand command,
        CommandBehavior requiredFlags = CommandBehavior.Default, CancellationToken ct = default)
    {
        try
        {
            var reader = await command.ExecuteReaderAsync(OptimizedBahavior | requiredFlags, ct).ConfigureAwait(false);
            return reader;
        }
        catch (ArgumentException)
        {
            var reader = await command.ExecuteReaderAsync(requiredFlags, ct).ConfigureAwait(false);
            return reader;
        }
    }

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
    /// Attempts to retrieve the AddWithValue method on a type derived from
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
                .Lambda<AddWithValueDelegate>(
                    expressionBody, targetParameter, nameParameter, valueParameter)
                .Compile();
        }
        catch
        {
            return null;
        }
    }

    public static long GetBytesIntoArray(this IDataRecord record, int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
    {
        if (record.IsDBNull(i))
            return 0;

        if (record.GetFieldType(i) != typeof(byte[]))
            throw new InvalidOperationException("Cannot get bytes because field is not a byte array.");

        var sourceSpan = (record.GetValue(i) as byte[]).AsSpan();
        var bytesRead = 0L;
        var targetOffset = bufferoffset;
        for (var sourceOffset = (int)fieldOffset; sourceOffset < sourceSpan.Length; sourceOffset++)
        {
            if (buffer is not null)
                buffer[bufferoffset] = sourceSpan[sourceOffset];

            targetOffset++;
            bytesRead++;

            if (bytesRead >= length)
                break;
        }

        return bytesRead;
    }

    public static long GetCharsIntoArray(this IDataRecord record, int i, long fieldOffset, char[]? buffer, int bufferoffset, int length)
    {
        if (record.IsDBNull(i))
            return 0;

        if (record.GetFieldType(i) != typeof(string))
            throw new InvalidOperationException("Cannot get chars because field is not of string type.");

        var sourceSpan = record.GetString(i).AsSpan();
        var charsRead = 0L;
        var targetOffset = bufferoffset;

        for (var sourceOffset = (int)fieldOffset; sourceOffset < sourceSpan.Length; sourceOffset++)
        {
            if (buffer is not null)
                buffer[bufferoffset] = sourceSpan[sourceOffset];

            targetOffset++;
            charsRead++;

            if (charsRead >= length)
                break;
        }

        return charsRead;
    }

    public static int GetValuesIntoArray(this IDataRecord record, object[] values)
    {
        var count = 0;
        for (var i = 0; i < Math.Min(values.Length, record.FieldCount); i++)
        {
            values[i] = record.GetValue(i);
            count++;
        }

        return count;
    }
}

