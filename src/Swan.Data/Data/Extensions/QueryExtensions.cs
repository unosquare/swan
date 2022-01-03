namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for querying data.
/// </summary>
public static partial class QueryExtensions
{
    #region IDbConnection

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

    #endregion

    #region IDbCommand

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
    public static IEnumerable<T> Query<T>(this IDbCommand command, CommandBehavior behavior = CommandBehavior.Default,
        Func<IDataReader, T>? deserialize = default)
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
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<dynamic> Query(this IDbCommand command, CommandBehavior behavior = CommandBehavior.Default) =>
        command.Query(behavior, (reader) => reader.ParseExpando());

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
    public static IEnumerable Query(this DataTable table, Type t, Func<DataRow, object>? deserialize = default)
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
