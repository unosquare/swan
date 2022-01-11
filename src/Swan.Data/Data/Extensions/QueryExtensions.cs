namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for querying data.
/// </summary>
public static partial class QueryExtensions
{
    #region DbCommand

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// The associated command is automatically disposed after iterating over the elements.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(this DbCommand command,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.Default)
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
    /// The associated command is automatically disposed after iterating over the elements.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<dynamic> Query(this DbCommand command, CommandBehavior behavior = CommandBehavior.Default) =>
        command.Query((r) => r.ParseExpando(), behavior);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// containing a single row of data.
    /// The associated command is automatically disposed after returning the first element.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <returns>The parse object.</returns>
    public static T? FirstOrDefault<T>(this DbCommand command,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.SingleRow) =>
        command.Query(deserialize, behavior).FirstOrDefault();


    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// containing a single row of data.
    /// The associated command is automatically disposed after returning the first element.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>The parse object.</returns>
    public static dynamic? FirstOrDefault(this DbCommand command, CommandBehavior behavior = CommandBehavior.SingleRow) =>
        command.Query(behavior).FirstOrDefault();

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
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(
        this DbConnection connection, string sql, object? param = default, Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.Default, DbTransaction? transaction = default, TimeSpan? timeout = default)
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

        return command.Query(deserialize, behavior);
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
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<dynamic> Query(
        this DbConnection connection, string sql, object? param = default,
        CommandBehavior behavior = CommandBehavior.Default, DbTransaction? transaction = default, TimeSpan? timeout = default)
        => connection.Query(sql, param, (r) => r.ParseExpando(), behavior, transaction, timeout);

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
    /// <returns>An enumerable, forward-only data source.</returns>
    public static T? FirstOrDefault<T>(
        this DbConnection connection, string sql, object? param = default, Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.SingleRow, DbTransaction? transaction = default, TimeSpan? timeout = default)
        => connection.Query(sql, param, deserialize, behavior, transaction, timeout).FirstOrDefault();

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
    /// <returns>An enumerable, forward-only data source.</returns>
    public static dynamic? FirstOrDefault(
        this DbConnection connection, string sql, object? param = default,
        CommandBehavior behavior = CommandBehavior.SingleRow, DbTransaction? transaction = default, TimeSpan? timeout = default)
        => connection.Query(sql, param, behavior, transaction, timeout).FirstOrDefault();

    #endregion

    #region DataTable

    /// <summary>
    /// Converts a <see cref="DataTable"/> object into an enumerable set
    /// of objects of the given type with property names corresponding to columns.
    /// </summary>
    /// <param name="table">The data table to extract rows from.</param>
    /// <param name="deserialize">An optional deserializer method that produces an object by providing a data row.</param>
    /// <returns>An enumerable set of objects.</returns>
    public static IEnumerable<T> Query<T>(this DataTable table, Func<DataRow, T>? deserialize = default)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        deserialize ??= (r) => r.ParseObject<T>();

        foreach (DataRow row in table.Rows)
            yield return deserialize.Invoke(row);
    }

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

    #endregion
}
