namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for querying data.
/// </summary>
public static partial class QueryExtensions
{
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
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(
        this DbConnection connection, string sql, Func<IDataRecord, T> deserialize, object? param = default,
        CommandBehavior behavior = CommandBehavior.Default, DbTransaction? transaction = default, TimeSpan? timeout = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentNullException(nameof(sql));

        if (deserialize is null)
            throw new ArgumentNullException(nameof(deserialize));

        var command = connection
            .BeginCommandText(sql)
            .EndCommandText()
            .WithTimeout(timeout ?? connection.Provider().DefaultCommandTimeout)
            .WithTransaction(transaction);

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
        this DbConnection connection, string sql, object? param = default,
        CommandBehavior behavior = CommandBehavior.Default, DbTransaction? transaction = default, TimeSpan? timeout = default)
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
        this DbConnection connection, string sql, object? param = default,
        CommandBehavior behavior = CommandBehavior.Default, DbTransaction? transaction = default, TimeSpan? timeout = default)
        => connection.Query(sql, (reader) => reader.ParseExpando(), param, behavior, transaction, timeout);

    #endregion

    #region DbCommand

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
    public static IEnumerable<T> Query<T>(this DbCommand command, CommandBehavior behavior = CommandBehavior.Default,
        Func<IDataRecord, T>? deserialize = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        deserialize ??= (r) => r.ParseObject<T>();
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
                    catch { /* ignore */ }
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
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async IAsyncEnumerable<T> QueryAsync<T>(this DbCommand command, CommandBehavior behavior = CommandBehavior.Default,
        Func<IDataRecord, T>? deserialize = default, [EnumeratorCancellation] CancellationToken ct = default)
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
            {
                await Task.Delay(500, ct).ConfigureAwait(false);
                yield return deserialize(reader);
            }

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
    /// iterating over records, one at a time.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<dynamic> Query(this DbCommand command, CommandBehavior behavior = CommandBehavior.Default) =>
        command.Query(behavior, (reader) => reader.ParseExpando());

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IAsyncEnumerable<dynamic> QueryAsync(this DbCommand command, CommandBehavior behavior = CommandBehavior.Default,
        CancellationToken ct = default) =>
        command.QueryAsync(behavior, (reader) => reader.ParseExpando(), ct);

    #endregion

    #region DataTable

    /// <summary>
    /// Converts a <see cref="DataTable"/> object into an enumerable set
    /// of <see cref="ExpandoObject"/> with property names corresponding to columns.
    /// Property names are normalized by removing whitespace, special
    /// characters or leading digits.
    /// </summary>
    /// <param name="table">The data table to extract rows from.</param>
    /// <returns>An enumerable set of dynamically typed Expando objects.</returns>
    public static IEnumerable<dynamic> Query(this DataTable table)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        foreach (DataRow row in table.Rows)
            yield return row.ParseExpando();
    }

    /// <summary>
    /// Converts a <see cref="DataTable"/> object into an enumerable set
    /// of objects of the given type with property names corresponding to columns.
    /// </summary>
    /// <param name="table">The data table to extract rows from.</param>
    /// <returns>An enumerable set of objects.</returns>
    public static IEnumerable<T> Query<T>(this DataTable table)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        foreach (DataRow row in table.Rows)
            yield return row.ParseObject<T>();
    }

    /// <summary>
    /// Converts a <see cref="DataTable"/> object into an enumerable set
    /// of objects of the given type with property names corresponding to columns.
    /// </summary>
    /// <param name="table">The data table to extract rows from.</param>
    /// <param name="t">The type to parse data rows into.</param>
    /// <param name="deserialize">An optional deserializer method that produces an object by providing a data row.</param>
    /// <returns>An enumerable set of objects.</returns>
    public static IEnumerable<object> Query(this DataTable table, Type t, Func<DataRow, object>? deserialize = default)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        if (t is null)
            throw new ArgumentNullException(nameof(t));

        deserialize ??= (r) => r.ParseObject(t);

        foreach (DataRow row in table.Rows)
            yield return deserialize.Invoke(row);
    }

    #endregion
}
