namespace Swan.Data.SqlBulkOps;

/// <summary>
/// Provides bulk updates methods for <see cref="ITableContext"/> with a connected <see cref="SqlConnection"/>.
/// </summary>
/// 
public static class BulkUpdateExtensions
{
    public static async Task<long> BulkUpdateAsync(this ITableContext table,
        IEnumerable items,
        DbTransaction? transaction = default,
        bool truncate = false,
        bool keepIndentity = true,
        int timeoutSeconds = 0,
        int batchSize = 0,
        int notifyAfter = 0,
        Action<ITableContext, long>? rowsCopiedCallback = default,
        CancellationToken ct = default)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        if (table.Connection is not SqlConnection connection)
            throw new ArgumentException($"The associated table connection is not of the type '{typeof(SqlConnection).FullName}'", nameof(table));

        // Ensure active connection
        await connection.EnsureConnectedAsync(ct).ConfigureAwait(false);

        // Read or create a provider-specific transaction.
        if (transaction is not SqlTransaction sqlTransaction)
            sqlTransaction = await connection.BeginTransactionAsync(ct) is not SqlTransaction createdTran
                ? throw new InvalidOperationException($"Unable to create transaction of type '{nameof(SqlTransaction)}'")
                : createdTran;

        // Determine if we had to create the transaction (local) or if it is external.
        // if it is local, then we need to manage the lifecycle of the transaction.
        var isLocalTransaction = transaction is null or not SqlTransaction;

        // Generate bulk copy options defaults
        var bulkCopyOptions =
            SqlBulkCopyOptions.TableLock |
            SqlBulkCopyOptions.KeepNulls |
            SqlBulkCopyOptions.KeepIdentity;

        return 0;
    }
}
