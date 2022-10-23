namespace Swan.Data.SqlBulkOps;

/// <summary>
/// Provides bulk updates methods for <see cref="ITableContext"/> with a connected <see cref="SqlConnection"/>.
/// </summary>
/// 
public static class BulkUpdateExtensions
{
    /// <summary>
    /// Performs a bulk update operation on the provided table.
    /// </summary>
    /// <param name="table">The table context.</param>
    /// <param name="items">The collection to update. Items must have key column values set.</param>
    /// <param name="transaction">An optional external transaction. If not provided, the transaction commit and rollback will be handled internally.</param>
    /// <param name="timeoutSeconds">Seconds before the operation times out. 0 for indefinite wait time.</param>
    /// <param name="batchSize">The number of rows to be processed at a time. Value will be clamped between 10 and 10000.</param>
    /// <param name="notifyAfter">Notification callback every number of rows. Value will be clamped between 10 and 1000.</param>
    /// <param name="notifyCallback">The action callback triggered upon a notification event.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>The total number of rows that were updated.</returns>
    public static async Task<long> BulkUpdateAsync(this ITableContext table,
        IEnumerable items,
        DbTransaction? transaction = default,
        int timeoutSeconds = Constants.InfiniteTimeoutSeconds,
        int batchSize = Constants.DefaultBatchSize,
        int notifyAfter = Constants.DefaultNotifyAfter,
        Action<ITableContext, long>? notifyCallback = default,
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

        // Generate a temporary table name
        var tempTableName = $"#{table.TableName}_{DateTime.UtcNow.Ticks}";

        // configure the bulk copy operation
        using var bulkOperation = new SqlBulkCopy(connection, bulkCopyOptions, sqlTransaction)
        {
            BatchSize = batchSize.Clamp(Constants.MinBatchSize, Constants.MaxBatchSize),
            DestinationTableName = table.Provider.QuoteTable(tempTableName, string.Empty),
            EnableStreaming = true,
            BulkCopyTimeout = timeoutSeconds.ClampMin(Constants.InfiniteTimeoutSeconds),
            NotifyAfter = notifyCallback is not null
                ? notifyAfter.Clamp(Constants.MinNotifyAfter, Constants.MaxNotifyAfter)
                : Constants.DefaultNotifyAfter
        };

        // Prepare and wire up the rows copied event for notification and row count updates.
        long rowsCopiedCount = default;
        int rowsUpdatedCount = default;

        // local event handler
        void onRowsCopied(object s, SqlRowsCopiedEventArgs e)
        {
            Interlocked.Exchange(ref rowsCopiedCount, e.RowsCopied);
            notifyCallback?.Invoke(table, e.RowsCopied);
        }

        bulkOperation.SqlRowsCopied += onRowsCopied;

        // Execute the bulk update operation.
        try
        {
            // Create the temporary table
            var tempTable = connection.TableBuilder(tempTableName, string.Empty, table.Columns);
            await tempTable.ExecuteDdlCommandAsync(sqlTransaction, ct).ConfigureAwait(false);

            // Build the column mappings
            foreach (var column in tempTable.Columns)
                bulkOperation.ColumnMappings.Add(column.Name, column.Name);

            // Use the collection as a data reader
            using var reader = items.ToDataReader(tempTable);

            // bulk insert into temp table using the streaming operation.
            await bulkOperation.WriteToServerAsync(reader, ct).ConfigureAwait(false);

            // now run the updates
            var updateCommandText = HelperExtensions.BuildBulkUpdateCommandText(tempTable, table, table.Provider);
            rowsUpdatedCount = await connection.ExecuteNonQueryAsync(updateCommandText, transaction: sqlTransaction, ct: ct).ConfigureAwait(false);

            // delete the temporary table
            var dropCommandText = $"DROP TABLE {tempTable.QuoteTable()}";
            _ = await connection.ExecuteNonQueryAsync(dropCommandText, transaction: sqlTransaction, ct: ct).ConfigureAwait(false);

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

        if (bulkOperation.TryGetRowsCopied(out var actualRowsCopied) && actualRowsCopied != Interlocked.Read(ref rowsCopiedCount))
            onRowsCopied(bulkOperation, new(actualRowsCopied));

        return rowsUpdatedCount;
    }
}
