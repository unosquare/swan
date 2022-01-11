namespace Swan.Data.Extensions;

public static partial class QueryExtensions
{
    #region DbCommand

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time. The command is automatically disposed
    /// after the iteration completes.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async IAsyncEnumerable<T> QueryAsync<T>(this DbCommand command,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.Default,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        deserialize ??= (r) => r.ParseObject<T>();
        var reader = await command.ExecuteOptimizedReaderAsync(behavior, ct).ConfigureAwait(false);

        try
        {
            if (reader.FieldCount <= 0)
                yield break;

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
                yield return deserialize(reader);

            // Skip the following result sets.
            while (await reader.NextResultAsync(ct).ConfigureAwait(false)) { }

            // Gracefully dispose the reader.
            await reader.DisposeAsync().ConfigureAwait(false);
            reader = null;
        }
        finally
        {
            if (reader is not null)
            {
                if (!reader.IsClosed)
                {
                    try { command.Cancel(); }
                    catch { /* ignore */ }
                }

                await reader.DisposeAsync().ConfigureAwait(false);
            }

            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time. The command is automatically disposed
    /// after the iteration completes.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async IAsyncEnumerable<dynamic> QueryAsync(this DbCommand command,
        CommandBehavior behavior = CommandBehavior.Default,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var enumerable = command.QueryAsync((r) => r.ParseExpando(), behavior, ct);
        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            yield return item;
    }

    /// <summary>
    /// Retrieves the first result from a query command and parses it as an
    /// object of the given type.
    /// </summary>
    /// <typeparam name="T">The type of element to return.</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization callback.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The element to return or null of not found.</returns>
    public static async Task<T?> FirstOrDefaultAsync<T>(this DbCommand command,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.SingleRow,
        CancellationToken ct = default)
    {
        var enumerable = command.QueryAsync(deserialize, behavior, ct);
        return await enumerable.FirstOrDefault(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the first result from a query command and parses it as a
    /// <see cref="ExpandoObject"/>.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The element to return or null of not found.</returns>
    public static async Task<dynamic?> FirstOrDefaultAsync(this DbCommand command,
        CommandBehavior behavior = CommandBehavior.SingleRow,
        CancellationToken ct = default)
    {
        var enumerable = command.QueryAsync(behavior, ct);
        return await enumerable.FirstOrDefault(ct).ConfigureAwait(false);
    }

    #endregion

    #region DbConnection

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
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async IAsyncEnumerable<T> QueryAsync<T>(this DbConnection connection,
        string sql, object?
        param = default,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.Default,
        DbTransaction? transaction = default,
        TimeSpan? timeout = default,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentNullException(nameof(sql));

        deserialize ??= (reader) => reader.ParseObject<T>();

        var command = connection
            .BeginCommandText(sql)
            .EndCommandText()
            .WithTimeout(timeout ?? connection.Provider().DefaultCommandTimeout)
            .WithTransaction(transaction)
            .SetParameters(param);

        var enumerable = command.QueryAsync(deserialize, behavior, ct);
        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            yield return item;
    }

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
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async IAsyncEnumerable<dynamic> QueryAsync(this DbConnection connection,
        string sql,
        object? param = default,
        CommandBehavior behavior = CommandBehavior.Default,
        DbTransaction? transaction = default, TimeSpan? timeout = default,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var enumerable = connection.QueryAsync(
            sql, param, (r) => r.ParseExpando(), behavior, transaction, timeout, ct);

        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            yield return item;
    }

    /// <summary>
    /// Executes a data reader in the underlying connection as a single result set
    /// and a single row that gets converted to an object via a deserialization callback.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="connection">The source connection.</param>
    /// <param name="sql">The SQL text to execute against the connection.</param>
    /// <param name="deserialize">A desdearialization function that outputs object of the given type.</param>
    /// <param name="param">Typically, an object of anonymous type with properties matching parameter names.</param>
    /// <param name="behavior">Optional command behavior.</param>
    /// <param name="transaction">Optional associated transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async Task<T?> FirstOrDefaultAsync<T>(this DbConnection connection,
        string sql,
        object? param = default,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.SingleRow,
        DbTransaction? transaction = default,
        TimeSpan? timeout = default,
        CancellationToken ct = default)
    {
        var enumerable = connection.QueryAsync(sql, param, deserialize, behavior, transaction, timeout, ct);
        enumerable.ConfigureAwait(false);
        return await enumerable.FirstOrDefault(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a data reader in the underlying connection as a single result set
    /// and a single row that gets converted to an object via a deserialization callback.
    /// </summary>
    /// <param name="connection">The source connection.</param>
    /// <param name="sql">The SQL text to execute against the connection.</param>
    /// <param name="param">Typically, an object of anonymous type with properties matching parameter names.</param>
    /// <param name="behavior">Optional command behavior.</param>
    /// <param name="transaction">Optional associated transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async Task<dynamic?> FirstOrDefaultAsync(this DbConnection connection,
        string sql,
        object? param = default,
        CommandBehavior behavior = CommandBehavior.SingleRow,
        DbTransaction? transaction = default,
        TimeSpan? timeout = default,
        CancellationToken ct = default)
    {
        var enumerable = connection.QueryAsync(sql, param, behavior, transaction, timeout, ct);
        enumerable.ConfigureAwait(false);
        return await enumerable.FirstOrDefault(ct).ConfigureAwait(false);
    }

    #endregion
}
