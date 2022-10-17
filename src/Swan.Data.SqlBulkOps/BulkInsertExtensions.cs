namespace Swan.Data.SqlBulkOps;

using Swan.Reflection;

/// <summary>
/// Provides bulk insert methods for <see cref="ITableContext"/> with a connected <see cref="SqlConnection"/>.
/// </summary>
public static class BulkInsertExtensions
{
    /// <summary>
    /// Performs a bulk insert operation on the provided table.
    /// </summary>
    /// <typeparam name="T">The item type of the collection.</typeparam>
    /// <param name="table">The table context.</param>
    /// <param name="items">The collection to insert.</param>
    /// <param name="transaction">An optional external transaction. If not provided, the transaction commit and rollback will be handled internally.</param>
    /// <param name="truncate">If set to true, the target table will be truncated first.</param>
    /// <param name="keepKeys">If keys and autoincrement values should be kept as directly read from the source.</param>
    /// <param name="timeoutSeconds">Seconds before the operation times out. 0 for indefinite wait time.</param>
    /// <param name="batchSize">The number of rows to be processed at a time. Value will be clamped between 10 and 10000.</param>
    /// <param name="notifyAfter">Notification callback every number of rows. Value will be clamped between 10 and 1000.</param>
    /// <param name="rowsCopiedCallback">The action callback triggered upon a notification event.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>The total number of rows that were processed.</returns>
    public static async Task<long> BulkInsertAsync<T>(this ITableContext<T> table,
        IEnumerable<T> items,
        DbTransaction? transaction = default,
        bool truncate = false,
        bool keepKeys = true,
        int timeoutSeconds = 0,
        int batchSize = 1000,
        int notifyAfter = 100,
        Action<ITableContext, long>? rowsCopiedCallback = default,
        CancellationToken ct = default)
        where T : class =>
        await BulkInsertAsync(
            table as ITableContext,
            items,
            transaction,
            truncate,
            keepKeys,
            timeoutSeconds,
            batchSize,
            notifyAfter,
            rowsCopiedCallback,
            ct).ConfigureAwait(false);

    /// <summary>
    /// Performs a bulk insert operation on the provided table.
    /// </summary>
    /// <param name="table">The table context.</param>
    /// <param name="items">The collection to insert.</param>
    /// <param name="transaction">An optional external transaction. If not provided, the transaction commit and rollback will be handled internally.</param>
    /// <param name="truncate">If set to true, the target table will be truncated first.</param>
    /// <param name="keepKeys">If keys and autoincrement values should be kept as directly read from the source.</param>
    /// <param name="timeoutSeconds">Seconds before the operation times out. 0 for indefinite wait time.</param>
    /// <param name="batchSize">The number of rows to be processed at a time. Value will be clamped between 10 and 10000.</param>
    /// <param name="notifyAfter">Notification callback every number of rows. Value will be clamped between 10 and 1000.</param>
    /// <param name="rowsCopiedCallback">The action callback triggered upon a notification event.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>The total number of rows that were processed.</returns>
    public static async Task<long> BulkInsertAsync(this ITableContext table,
        IEnumerable items,
        DbTransaction? transaction = default,
        bool truncate = false,
        bool keepKeys = true,
        int timeoutSeconds = 0,
        int batchSize = 1000,
        int notifyAfter = 100,
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

        var isLocalTransaction = transaction is null or not SqlTransaction;

        // Generate bulk copy options
        var bulkCopyOptions = SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepNulls;
        if (keepKeys) bulkCopyOptions |= SqlBulkCopyOptions.KeepIdentity;

        // configure the bulk copy operation
        using var bulkOperation = new SqlBulkCopy(connection, bulkCopyOptions, sqlTransaction)
        {
            BatchSize = batchSize.Clamp(10, 10000),
            DestinationTableName = table.Provider.QuoteTable(table.TableName, table.Schema),
            EnableStreaming = true,
            BulkCopyTimeout = timeoutSeconds.ClampMin(0)
        };

        // configure batch notification
        bulkOperation.NotifyAfter = rowsCopiedCallback is not null ? notifyAfter.Clamp(10, 1000) : 100;

        long rowsCopiedCount = default;

        SqlRowsCopiedEventHandler onRowsCopied = (s, e) =>
        {
            Interlocked.Exchange(ref rowsCopiedCount, e.RowsCopied);
            rowsCopiedCallback?.Invoke(table, e.RowsCopied);
        };

        bulkOperation.SqlRowsCopied += onRowsCopied;

        // Build the column mappings
        foreach (var column in table.Columns)
            bulkOperation.ColumnMappings.Add(column.Name, column.Name);

        try
        {
            // Execute truncate command if needed.
            if (truncate)
            {
                await connection
                    .BeginCommandText()
                    .Truncate(table.TableName, table.Schema)
                    .EndCommandText()
                    .WithTransaction(sqlTransaction)
                    .ExecuteNonQueryAsync(ct)
                    .ConfigureAwait(false);
            }

            // Use the collection as a data reader
            using var reader = items.ToDataReader(table);

            // bulk inseret using the streaming operation.
            await bulkOperation.WriteToServerAsync(reader, ct).ConfigureAwait(false);

            // commit the transaction if successful
            if (isLocalTransaction)
                await sqlTransaction.CommitAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            // Rollback the local transaction
            if (isLocalTransaction)
                await sqlTransaction.RollbackAsync(ct).ConfigureAwait(false);

            throw;
        }
        finally
        {
            // dispose the local transaction
            if (isLocalTransaction)
                await sqlTransaction.DisposeAsync().ConfigureAwait(false);
        }

        var rowsCopiedField = typeof(SqlBulkCopy).TypeInfo().Fields
            .FirstOrDefault(c => !c.IsPublic && c.FieldType == typeof(int) && c.Name.Equals("_rowsCopied", StringComparison.Ordinal));

        if (rowsCopiedField is not null)
        {
            if (rowsCopiedField.GetValue(bulkOperation) is int actualRowsCopied)
                onRowsCopied.Invoke(bulkOperation, new(actualRowsCopied));
        }

        return Interlocked.Read(ref rowsCopiedCount);
    }
}
