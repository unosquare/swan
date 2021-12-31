namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for querying data.
/// </summary>
public static partial class QueryExtensions
{
    /// <summary>
    /// Executes a data reader in the underlying connection as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="connection">The source connection.</param>
    /// <param name="sql">The SQL text to execute against the connection.</param>
    /// <param name="deserialize">A desdearialization function that outputs object of the given type.</param>
    /// <param name="param">Typically, an object of anonymous type with properties matching parameter names.</param>
    /// <param name="behavior">Optional command behavior.</param>
    /// <param name="transaction">Optional associated transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(
        this IDbConnection connection, string sql, Func<IDataReader, T> deserialize, object? param = default,
        CommandBehavior behavior = CommandBehavior.Default, IDbTransaction? transaction = default, TimeSpan? timeout = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentNullException(nameof(sql));

        if (deserialize is null)
            throw new ArgumentNullException(nameof(deserialize));

        var command = connection
            .StartCommand()
            .WithText(sql)
            .WithTimeout(timeout)
            .FinishCommand(transaction);

        if (param != null)
            command.SetParameters(param);

        return command.Query(behavior, deserialize);
    }

    /// <summary>
    /// Executes a data reader in the underlying connection as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="connection">The source connection.</param>
    /// <param name="sql">The SQL text to execute against the connection.</param>
    /// <param name="param">Typically, an object of anonymous type with properties matching parameter names.</param>
    /// <param name="behavior">Optional command behavior.</param>
    /// <param name="transaction">Optional associated transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(
        this IDbConnection connection, string sql, object? param = default,
        CommandBehavior behavior = CommandBehavior.Default, IDbTransaction? transaction = default, TimeSpan? timeout = default)
        => connection.Query(sql, (reader) => reader.ParseObject<T>(), param, behavior, transaction, timeout);

    /// <summary>
    /// Executes a data reader in the underlying connection as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <param name="connection">The source connection.</param>
    /// <param name="sql">The SQL text to execute against the connection.</param>
    /// <param name="param">Typically, an object of anonymous type with properties matching parameter names.</param>
    /// <param name="behavior">Optional command behavior.</param>
    /// <param name="transaction">Optional associated transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<dynamic> Query(
        this IDbConnection connection, string sql, object? param = default,
        CommandBehavior behavior = CommandBehavior.Default, IDbTransaction? transaction = default, TimeSpan? timeout = default)
        => connection.Query(sql, (reader) => reader.ParseExpando(), param, behavior, transaction, timeout);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(this IDbCommand command, CommandBehavior behavior, Func<IDataReader, T> deserialize)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(InternalExtensions.CommandConnectionErrorMessage, nameof(command));

        if (deserialize == null)
            throw new ArgumentNullException(nameof(deserialize));

        var reader = command.ExecuteOptimizedReader(behavior);

        try
        {
            if (reader.FieldCount == 0)
                yield break;

            while (reader.Read())
            {
                yield return deserialize(reader);
            }

            // skip the following result sets.
            while (reader.NextResult()) { }
            reader.Dispose();
            reader = null;
        }
        finally
        {
            if (reader != null)
            {
                if (!reader.IsClosed)
                {
                    try { command.Cancel(); }
                    catch { /* don't spoil the existing exception */ }
                }
                reader.Dispose();
            }

            command.Parameters?.Clear();
            command.Dispose();
        }
    }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize records into.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(this IDbCommand command, CommandBehavior behavior = CommandBehavior.Default) =>
        command.Query(behavior, (reader) => reader.ParseObject<T>());

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<dynamic> Query(this IDbCommand command, CommandBehavior behavior = CommandBehavior.Default) =>
        command.Query(behavior, (reader) => reader.ParseExpando());
}
