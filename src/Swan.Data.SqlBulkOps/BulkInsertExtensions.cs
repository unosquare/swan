namespace Swan.Data.SqlBulkOps;

public static class BulkInsertExtensions
{
    public static async Task<long> BulkInsertAsync(
        this ITableContext table, IEnumerable items, DbTransaction? transaction = default, bool truncate = false, bool keepKeys = true, int batchSize = 1000, Action<ITableContext, long>? rowsCopiedCallback = default, CancellationToken ct = default)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        if (table.Connection is not SqlConnection connection)
            throw new ArgumentException($"The associated table connection is not of the type '{typeof(SqlConnection).FullName}'", nameof(table));

        await connection.EnsureConnectedAsync(ct).ConfigureAwait(false);

        if (transaction is not SqlTransaction tran)
            tran = connection.BeginTransaction();

        var bulkCopyOptions = SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepNulls;
        if (keepKeys) bulkCopyOptions |= SqlBulkCopyOptions.KeepIdentity;

        using var bulkOperation = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, tran);

    }
}
