namespace Swan.Data;

public static class DbConnectionExtensions
{
    public static async Task<CommandContext> TableCommandAsync(this DbConnection connection, string tableName, string? schemaName = default)
    {
        var tableMeta = await TableMetadata.AcquireAsync(connection, tableName, schemaName);
        return new CommandContext(connection, tableMeta);
    }

    public static CommandContext TableCommand(this DbConnection connection, string tableName, string? schemaName = default) =>
        connection.TableCommandAsync(tableName, schemaName).ConfigureAwait(false).GetAwaiter().GetResult();

    public static ProviderMetadata Provider(this DbConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        return ProviderMetadata.Acquire(connection.GetType());
    }

    public static async Task EnsureIsValid(this DbConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        if (string.IsNullOrWhiteSpace(connection.Database))
            throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.");
    }
}
