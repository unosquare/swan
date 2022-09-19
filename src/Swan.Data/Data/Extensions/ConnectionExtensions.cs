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
    /// Provides a quick way to generate and execute stand-alone table DDL commands.
    /// No foreign keys or indexes are generated.
    /// </summary>
    /// <typeparam name="T">The type to get column names and types from.</typeparam>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <returns>A table schema builder for schema generation.</returns>
    public static TableSchemaBuilder TableBuilder<T>(this DbConnection connection, string tableName, string? schemaName = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var builder = new TableSchemaBuilder(connection, tableName, schemaName ?? string.Empty);
        builder.MapType(typeof(T));
        return builder;
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
    public static ITableContext Table(this DbConnection connection, string tableName, string? schema = default) =>
        new TableContext(connection, TableContext.CacheLoadTableSchema(connection, tableName, schema));

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
        where T : class =>
        new TableContext<T>(connection, TableContext.CacheLoadTableSchema(connection, tableName, schema));

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
    /// Starts a fluent command definition using a <see cref="CommandSource"/>.
    /// When done, use the <see cref="CommandSource.EndCommandText"/> method call
    /// to extract the action <see cref="DbCommand"/>.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="initialText">The optional, initial command text to start building upon.</param>
    /// <returns>A fluent command definition.</returns>
    public static CommandSource BeginCommandText(this DbConnection connection, string? initialText = default) => connection is null
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

        var command = new CommandSource(connection, sql).EndCommandText()
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

        var command = new CommandSource(connection, sql).EndCommandText()
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

        var command = new CommandSource(connection, sql).EndCommandText()
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

        var command = new CommandSource(connection, sql).EndCommandText()
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
}
