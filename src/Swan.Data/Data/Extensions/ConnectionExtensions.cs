namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DbConnection"/> objects.
/// </summary>
public static partial class ConnectionExtensions
{
    /// <summary>
    /// Retrieves the <see cref="DbProvider"/> associated with the given connection.
    /// </summary>
    /// <param name="connection">The connection to extract the provider from.</param>
    /// <returns>The associated DB provider.</returns>
    public static DbProvider Provider(this DbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : DbProvider.FromConnection(connection);

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

        await connection.EnsureIsValidAsync(ct).ConfigureAwait(false);
        if (connection.Database.Contains('\'', StringComparison.Ordinal))
            throw new NotSupportedException("Unable to get tables from database names with special character '");

        var commandText = connection.Provider().GetListTablesCommandText();
        var result = new List<TableIdentifier>(128);
        await foreach (var item in connection.QueryAsync<TableIdentifier>(commandText, ct: ct).ConfigureAwait(false))
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

        connection.EnsureIsValid();
        if (connection.Database.Contains('\'', StringComparison.Ordinal))
            throw new NotSupportedException("Unable to get tables from database names with special character '");

        var commandText = connection.Provider().GetListTablesCommandText();
        var result = new List<TableIdentifier>(128);
        foreach (var item in connection.Query<TableIdentifier>(commandText))
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
    /// <returns></returns>
    public static TableSchemaBuilder TableBuilder<T>(this DbConnection connection, string tableName, string? schemaName = default)
    {
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
        new TableContext(connection, tableName, schema);

    /// <summary>
    /// Acquires a connected table context that can be used to inspect the associated
    /// table schema and issue CRUD commands. Once the schema is obtained, it is cached
    /// and reused whenever the table context is re-acquired. Caching keys are
    /// computed based on the connection string, provider type and table name and schema.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="table">The associated table.</param>
    /// <returns>A connected table context.</returns>
    public static ITableContext Table(this DbConnection connection, TableIdentifier table) => table is not null
        ? new TableContext(connection, table.Name, table.Schema)
        : throw new ArgumentNullException(nameof(table));

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
        new TableContext<T>(connection, tableName, schema);

    /// <summary>
    /// Acquires a typed, connected table context that can be used to inspect the associated
    /// table schema and issue CRUD commands. Once the schema is obtained, it is cached
    /// and reused whenever the table context is re-acquired. Caching keys are
    /// computed based on the connection string, provider type and table name and schema.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="table">The associated table.</param>
    /// <returns>A connected table context.</returns>
    public static ITableContext<T> Table<T>(this DbConnection connection, TableIdentifier table)
        where T : class => table is not null
        ? new TableContext<T>(connection, table.Name, table.Schema)
        : throw new ArgumentNullException(nameof(table));

    /// <summary>
    /// Ensures the connection state is open and that the <see cref="DbConnection.Database"/> property has been set.
    /// </summary>
    /// <param name="connection">The connection to check for validity.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    public static async Task EnsureIsValidAsync(this DbConnection connection, CancellationToken ct = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(connection.Database))
            throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.");
    }

    /// <summary>
    /// Ensures the connection state is open and that the <see cref="DbConnection.Database"/> property has been set.
    /// </summary>
    /// <param name="connection">The connection to check for validity.</param>
    public static void EnsureIsValid(this DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (connection.State != ConnectionState.Open)
            connection.Open();

        if (string.IsNullOrWhiteSpace(connection.Database))
            throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.");
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

    /// <summary>
    /// Computes an integer representing a hash code for the connection,
    /// taking into account the connection string, the database, and the type of the
    /// connection.
    /// </summary>
    /// <param name="connection">The connection to compute the hash key for.</param>
    /// <returns>A hash code representing a cache entry id.</returns>
    internal static int ComputeCacheKey(this DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        connection.EnsureIsValid();
        var hashA = connection.ConnectionString.GetHashCode(StringComparison.Ordinal);
        var hashB = connection.Database.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);
        var hashC = connection.GetType().GetHashCode();

        return HashCode.Combine(hashA, hashB, hashC);
    }
}
