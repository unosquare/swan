﻿namespace Swan.Data;

/// <summary>
/// Provides <see cref="DbConnection"/> extension methods.
/// </summary>
public static class DbConnectionExtensions
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

    public static async Task<DbTableContext> TableCommandAsync(this DbConnection connection, string tableName, string? schemaName = default)
    {
        var tableMeta = await TableMetadata.AcquireAsync(connection, tableName, schemaName);
        await connection.EnsureIsValidAsync();
        return new DbTableContext(connection, tableMeta);
    }

    public static DbTableContext TableCommand(this DbConnection connection, string tableName, string? schemaName = default) =>
        connection.TableCommandAsync(tableName, schemaName).ConfigureAwait(false).GetAwaiter().GetResult();

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
            await connection.OpenAsync(ct);

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

    public static CommandDefinition StartCommand(this DbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : new(connection);
}