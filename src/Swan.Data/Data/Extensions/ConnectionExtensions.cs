namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IDbConnection"/> objects.
/// </summary>
public static partial class ConnectionExtensions
{
    /// <summary>
    /// Retrieves the <see cref="DbProvider"/> associated with the given connection.
    /// </summary>
    /// <param name="connection">The connection to extract the provider from.</param>
    /// <returns>The associated DB provider.</returns>
    public static DbProvider Provider(this IDbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : DbProvider.FromConnection(connection);

    public static async Task<IReadOnlyList<string>> TableNames(this IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        await connection.EnsureIsValidAsync();

        var tables = new List<string>();
        var dt = (connection as DbConnection).GetSchema("Tables");
        foreach (DataRow row in dt.Rows)
        {
            string tablename = (string)row[2];
            tables.Add(tablename);
        }
        return tables;
    }

    public static ITableContext Table(this IDbConnection connection, string tableName, string? schema = default) =>
        new TableContext(connection, tableName, schema);

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
    /// Ensures the connection state is open and that the <see cref="IDbConnection.Database"/> property has been set.
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

    /// <summary>
    /// Starts a fluent command definition using a <see cref="CommandSource"/>.
    /// When done, use the <see cref="CommandSource.EndCommand(IDbTransaction?)"/> method call
    /// to extract the action <see cref="IDbCommand"/>.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>A fluent command definition.</returns>
    public static CommandSource BeginCommand(this IDbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : new(connection);
}

