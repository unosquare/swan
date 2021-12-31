namespace Swan.Data;

/// <summary>
/// Provides <see cref="DbConnection"/> extension methods.
/// </summary>
public static partial class DbConnectionHelper
{
    public static DbProvider Provider(this IDbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : DbProvider.FromConnection(connection);

    public static async Task<IReadOnlyList<string>> TableNames(this DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        await connection.EnsureIsValidAsync();

        var tables = new List<string>();
        var dt = connection.GetSchema("Tables");
        foreach (DataRow row in dt.Rows)
        {
            string tablename = (string)row[2];
            tables.Add(tablename);
        }
        return tables;
    }

    /// <summary>
    /// Ensures the connection state is open and that the <see cref="IDbConnection.Database"/> property has been set.
    /// </summary>
    /// <param name="connection">The connection to check for validity.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    public static async Task EnsureIsValidAsync(this IDbConnection connection, CancellationToken ct = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (connection.State != ConnectionState.Open)
        {
            if (connection is DbConnection dbConnection)
                await dbConnection.OpenAsync(ct);
            else
                connection.Open();
        }

        if (string.IsNullOrWhiteSpace(connection.Database))
            throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.");
    }

    /// <summary>
    /// Ensures the connection state is open and that the <see cref="DbConnection.Database"/> property has been set.
    /// </summary>
    /// <param name="connection">The connection to check for validity.</param>
    public static void EnsureIsValid(this IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (connection.State != ConnectionState.Open)
            connection.Open();

        if (string.IsNullOrWhiteSpace(connection.Database))
            throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.");
    }

    public static DbCommandSource StartCommand(this IDbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : new(connection);

    public static IEnumerable<T> Query<T>(this IDbConnection connection, string sql, Func<IDataReader, T> deserialize, object? param = default, CommandBehavior behavior = CommandBehavior.Default, IDbTransaction? transaction = default, TimeSpan? timeout = default)
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

    public static IEnumerable<T> Query<T>(
        this IDbConnection connection, string sql, object? param = default, CommandBehavior behavior = CommandBehavior.Default, IDbTransaction? transaction = default, TimeSpan? timeout = default)
        => connection.Query(sql, (reader) => reader.ParseObject<T>(), param, behavior, transaction, timeout);

    public static IEnumerable<dynamic> Query(
        this IDbConnection connection, string sql, object? param = default, CommandBehavior behavior = CommandBehavior.Default, IDbTransaction? transaction = default, TimeSpan? timeout = default)
        => connection.Query(sql, (reader) => reader.ParseExpando(), param, behavior, transaction, timeout);
}
