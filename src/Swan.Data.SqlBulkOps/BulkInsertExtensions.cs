namespace Swan.Data.SqlBulkOps;

public static class BulkInsertExtensions
{
    public static async Task<long> BulkInsertAsync(this ITableContext table,
        IEnumerable items,
        DbTransaction? transaction = default,
        bool truncate = false,
        bool keepKeys = true,
        int batchSize = 1000,
        int notifyAfter = 100,
        Action<ITableContext, long>? rowsCopiedCallback = default,
        CancellationToken ct = default)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        if (table.Connection is not SqlConnection connection)
            throw new ArgumentException($"The associated table connection is not of the type '{typeof(SqlConnection).FullName}'", nameof(table));

        await connection.EnsureConnectedAsync(ct).ConfigureAwait(false);

        if (transaction is not SqlTransaction tran)
            tran = await connection.BeginTransactionAsync(ct) is not SqlTransaction createdTran
                ? throw new InvalidOperationException($"Unable to create transaction of type '{nameof(SqlTransaction)}'")
                : createdTran;

        var bulkCopyOptions = SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepNulls;
        if (keepKeys) bulkCopyOptions |= SqlBulkCopyOptions.KeepIdentity;

        using var bulkOperation = new SqlBulkCopy(connection, bulkCopyOptions, tran);
        bulkOperation.BatchSize = batchSize;

        if (rowsCopiedCallback is not null)
        {
            bulkOperation.NotifyAfter = notifyAfter;
            bulkOperation.SqlRowsCopied += (s, e) => rowsCopiedCallback?.Invoke(table, e.RowsCopied);
        }

        return 0;
    }
}
