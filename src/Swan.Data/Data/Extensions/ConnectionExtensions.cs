namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DbConnection"/> objects.
/// </summary>
public static class ConnectionExtensions
{
    /// <summary>
    /// Retrieves the <see cref="DbProvider"/> associated with the given connection.
    /// </summary>
    /// <param name="connection">The connection to extract the provider from.</param>
    /// <returns>The associated DB provider.</returns>
    public static DbProvider Provider(this DbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : connection.TryGetProvider(out var provider)
        ? provider
        : throw new ArgumentException($"Provider for connection type '{connection.GetType()}' is not registered.", nameof(connection));

    /// <summary>
    /// Retrieves a list of table names in the database. This may include views and temporary tables.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of table names in the database.</returns>
    public static async Task<IReadOnlyList<TableIdentifier>> GetTableNamesAsync(this DbConnection connection, CancellationToken ct = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        await using var command = connection.Provider().CreateListTablesCommand(connection);
        var result = new List<TableIdentifier>(128);
        await foreach (var item in command.QueryAsync<TableIdentifier>(ct: ct).ConfigureAwait(false))
            result.Add(item);

        return result;
    }

    /// <summary>
    /// Retrieves a list of table names in the database. This may include views and temporary tables.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>A list of table names in the database.</returns>
    public static IReadOnlyList<TableIdentifier> GetTableNames(this DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        using var command = connection.Provider().CreateListTablesCommand(connection);
        var result = new List<TableIdentifier>(128);
        foreach (var item in command.Query<TableIdentifier>())
            result.Add(item);

        return result;
    }

    /// <summary>
    /// Provides a way to generate a table context along with a table schema based on a specific type.
    /// You would typically use this to build a table based on a type and then issue the corresponding DDL
    /// command to the database in order to create a table to store objects of the given type.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="objectType">The type of object.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="schemaName">The optional schema name.</param>
    /// <returns>A generated table context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ITableBuilder TableBuilder(this DbConnection connection, Type objectType, string tableName, string? schemaName = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (objectType is null)
            throw new ArgumentNullException(nameof(objectType));

        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        var typeSchema = objectType.ToTableSchema(connection, tableName, schemaName);
        return new TableContext(connection, typeSchema);
    }

    /// <summary>
    /// Provides a way to generate a table context along with a table schema based on a specific type.
    /// You would typically use this to build a table based on a type and then issue the corresponding DDL
    /// command to the database in order to create a table to store objects of the given type.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <typeparam name="T">The type of object.</typeparam>
    /// <param name="tableName">The table name.</param>
    /// <param name="schemaName">The optional schema name.</param>
    /// <returns>A generated table context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ITableBuilder TableBuilder<T>(this DbConnection connection, string tableName, string? schemaName = default)
        where T : class
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        var typeSchema = typeof(T).ToTableSchema(connection, tableName, schemaName);
        return new TableContext<T>(connection, typeSchema);
    }

    /// <summary>
    /// Acquires a connected table context that can be used to inspect the associated
    /// table schema and issue CRUD commands. Once the schema is obtained, it is cached
    /// and reused whenever the table context is re-acquired. Caching keys are
    /// computed based on the connection string, provider type and table name and schema.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The associated table name.</param>
    /// <param name="schema">The optional schema.</param>
    /// <returns>A connected table context.</returns>
    public static ITableContext Table(this DbConnection connection, string tableName, string? schema = default) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : new TableContext(connection, TableContext.CacheLoadTableSchema(connection, tableName, schema));

    /// <summary>
    /// Acquires a connected table context that can be used to inspect the associated
    /// table schema and issue CRUD commands. Once the schema is obtained, it is cached
    /// and reused whenever the table context is re-acquired. Caching keys are
    /// computed based on the connection string, provider type and table name and schema.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The associated table name.</param>
    /// <param name="schema">The optional schema.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A connected table context.</returns>
    public static async Task<ITableContext> TableAsync(
        this DbConnection connection, string tableName, string? schema = default, CancellationToken ct = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var tableSchema = await TableContext.CacheLoadTableSchemaAsync(connection, tableName, schema, ct).ConfigureAwait(false);
        return new TableContext(connection, tableSchema);
    }

    /// <summary>
    /// Acquires a typed, connected table context that can be used to inspect the associated
    /// table schema and issue CRUD commands. Once the schema is obtained, it is cached
    /// and reused whenever the table context is re-acquired. Caching keys are
    /// computed based on the connection string, provider type and table name and schema.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The associated table name.</param>
    /// <param name="schema">The optional schema.</param>
    /// <returns>A connected table context.</returns>
    public static ITableContext<T> Table<T>(this DbConnection connection, string tableName, string? schema = default)
        where T : class => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : new TableContext<T>(connection, TableContext.CacheLoadTableSchema(connection, tableName, schema));

    /// <summary>
    /// Acquires a typed, connected table context that can be used to inspect the associated
    /// table schema and issue CRUD commands. Once the schema is obtained, it is cached
    /// and reused whenever the table context is re-acquired. Caching keys are
    /// computed based on the connection string, provider type and table name and schema.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The associated table name.</param>
    /// <param name="schema">The optional schema.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A connected table context.</returns>
    public static async Task<ITableContext<T>> TableAsync<T>(
        this DbConnection connection, string tableName, string? schema = default, CancellationToken ct = default)
        where T : class
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var tableSchema = await TableContext.CacheLoadTableSchemaAsync(connection, tableName, schema, ct).ConfigureAwait(false);
        return new TableContext<T>(connection, tableSchema);
    }

    /// <summary>
    /// Ensures the connection state is open and that the <see cref="DbConnection.Database"/> property has been set.
    /// </summary>
    /// <param name="connection">The connection to check for validity.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    public static async Task<T> EnsureConnectedAsync<T>(this T connection, CancellationToken ct = default)
        where T : DbConnection
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        connection.ConfigureAwait(false);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct).ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(connection.Database)
            ? throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.")
            : connection;
    }

    /// <summary>
    /// Ensures the connection state is open and that the <see cref="DbConnection.Database"/> property has been set.
    /// </summary>
    /// <param name="connection">The connection to check for validity.</param>
    public static T EnsureConnected<T>(this T connection)
        where T : DbConnection
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        connection.ConfigureAwait(false);

        if (connection.State != ConnectionState.Open)
            connection.Open();

        return string.IsNullOrWhiteSpace(connection.Database)
            ? throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.")
            : connection;
    }

    /// <summary>
    /// Configures the connection's provider with a default command timeout.
    /// </summary>
    /// <typeparam name="T">The connection type.</typeparam>
    /// <param name="connection">The associated connection.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>The connection object for fluent API support.</returns>
    public static T WithDefaultCommandTimeout<T>(this T connection, TimeSpan timeout)
        where T : DbConnection
    {
        connection.Provider().WithDefaultCommandTimeout(timeout);
        return connection;
    }

    /// <summary>
    /// Starts a fluent command definition using a <see cref="DbCommandSource"/>.
    /// When done, use the <see cref="DbCommandSource.EndCommandText"/> method call
    /// to extract the action <see cref="DbCommand"/>.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="initialText">The optional, initial command text to start building upon.</param>
    /// <returns>A fluent command definition.</returns>
    public static DbCommandSource BeginCommandText(this DbConnection connection, string? initialText = default) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : new(connection, initialText);

    /// <summary>
    /// Executes SQL statements against the database and returns the number of affected records.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="sql">The SQL statements to execute.</param>
    /// <param name="param">The object containing parameter names as properties and their values.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="timeout">The timeout value to execute and retrieve the result.</param>
    /// <returns>The number of affected records.</returns>
    public static int ExecuteNonQuery(this DbConnection connection, string sql, object? param = default,
        DbTransaction? transaction = default, CommandType commandType = CommandType.Text, TimeSpan? timeout = default)
    {
        connection.EnsureConnected();

        var command = new DbCommandSource(connection, sql).EndCommandText()
            .SetParameters(param)
            .WithTransaction(transaction)
            .WithCommandType(commandType)
            .WithTimeout(timeout ?? connection.Provider().DefaultCommandTimeout);

        try
        {
            return command.ExecuteNonQuery();
        }
        finally
        {
            command.Parameters?.Clear();
            command.Dispose();
        }
    }

    /// <summary>
    /// Executes SQL statements against the database and returns the number of affected records.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="sql">The SQL statements to execute.</param>
    /// <param name="param">The object containing parameter names as properties and their values.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="timeout">The timeout value to execute and retrieve the result.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    public static async Task<int> ExecuteNonQueryAsync(this DbConnection connection, string sql, object? param = default,
        DbTransaction? transaction = default, CommandType commandType = CommandType.Text, TimeSpan? timeout = default,
        CancellationToken ct = default)
    {
        await connection.EnsureConnectedAsync(ct).ConfigureAwait(false);

        var command = new DbCommandSource(connection, sql).EndCommandText()
            .SetParameters(param)
            .WithTransaction(transaction)
            .WithCommandType(commandType)
            .WithTimeout(timeout ?? connection.Provider().DefaultCommandTimeout);

        try
        {
            return await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync();
        }
    }

    /// <summary>
    /// Executes SQL statements against the database and returns the first column of the row.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="sql">The SQL statements to execute.</param>
    /// <param name="param">The object containing parameter names as properties and their values.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="timeout">The timeout value to execute and retrieve the result.</param>
    /// <returns>The the first column of the first row of the result.</returns>
    public static object? ExecuteScalar(this DbConnection connection, string sql, object? param = default,
        DbTransaction? transaction = default, CommandType commandType = CommandType.Text, TimeSpan? timeout = default)
    {
        connection.EnsureConnected();

        var command = new DbCommandSource(connection, sql).EndCommandText()
            .SetParameters(param)
            .WithTransaction(transaction)
            .WithCommandType(commandType)
            .WithTimeout(timeout ?? connection.Provider().DefaultCommandTimeout);

        try
        {
            return command.ExecuteScalar();
        }
        finally
        {
            command.Parameters?.Clear();
            command.Dispose();
        }
    }

    /// <summary>
    /// Executes SQL statements against the database and returns the first column of the row.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="sql">The SQL statements to execute.</param>
    /// <param name="param">The object containing parameter names as properties and their values.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="timeout">The timeout value to execute and retrieve the result.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The the first column of the first row of the result.</returns>
    public static async Task<object?> ExecuteScalarAsync(this DbConnection connection, string sql, object? param = default,
        DbTransaction? transaction = default, CommandType commandType = CommandType.Text, TimeSpan? timeout = default,
        CancellationToken ct = default)
    {
        await connection.EnsureConnectedAsync(ct).ConfigureAwait(false);

        var command = new DbCommandSource(connection, sql).EndCommandText()
            .SetParameters(param)
            .WithTransaction(transaction)
            .WithCommandType(commandType)
            .WithTimeout(timeout ?? connection.Provider().DefaultCommandTimeout);

        try
        {
            return await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync();
        }
    }

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
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="connection">The source connection.</param>
    /// <param name="sql">The SQL text to execute against the connection.</param>
    /// <param name="deserialize">A deserialization function that outputs object of the given type.</param>
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
    /// and provides a forward-only enumerable set which can then be processed by
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
        return await enumerable.FirstOrDefaultAsync(ct).ConfigureAwait(false);
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
        return await enumerable.FirstOrDefaultAsync(ct).ConfigureAwait(false);
    }
}
