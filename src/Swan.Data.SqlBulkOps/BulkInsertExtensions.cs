namespace Swan.Data.SqlBulkOps;

using System.Data;

/// <summary>
/// Provides bulk insert methods for <see cref="ITableContext"/> with a connected <see cref="SqlConnection"/>.
/// </summary>
public static class BulkInsertExtensions
{
    /// <summary>
    /// Performs a bulk insert operation on the provided table.
    /// </summary>
    /// <param name="table">The table context.</param>
    /// <param name="dataReader">The data reader to read the data from.</param>
    /// <param name="transaction">An optional external transaction. If not provided, the transaction commit and rollback will be handled internally.</param>
    /// <param name="truncate">If set to true, the target table will be truncated first.</param>
    /// <param name="keepIdentity">If keys for autoincrement values should be kept as directly read from the source.</param>
    /// <param name="timeoutSeconds">Seconds before the operation times out. 0 for indefinite wait time.</param>
    /// <param name="batchSize">The number of rows to be processed at a time. Value will be clamped between 10 and 10000.</param>
    /// <param name="notifyAfter">Notification callback every number of rows. Value will be clamped between 10 and 1000.</param>
    /// <param name="notifyCallback">The action callback triggered upon a notification event.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>
    /// The total number of rows that were inserted.
    /// </returns>
    /// <exception cref="ArgumentNullException">nameof(table)</exception>
    /// <exception cref="ArgumentException">$"The associated table connection is not of the type '{typeof(SqlConnection).FullName}', nameof(table)</exception>
    public static async Task<long> BulkInsertAsync(this ITableContext table,
        IDataReader dataReader,
        IDbTransaction? transaction = default,
        bool truncate = false,
        bool keepIdentity = true,
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
            sqlTransaction = await connection.BeginTransactionAsync(ct) as SqlTransaction ?? throw new InvalidOperationException($"Unable to create transaction of type '{nameof(SqlTransaction)}'");

        // Determine if we had to create the transaction (local) or if it is external.
        // if it is local, then we need to manage the lifecycle of the transaction.
        var isLocalTransaction = transaction is null or not SqlTransaction;

        var isMemoryOptimized = await table.IsMemoryOptimized(sqlTransaction, ct).ConfigureAwait(false);

        // Generate bulk copy options defaults
        var bulkCopyOptions =
            SqlBulkCopyOptions.KeepNulls |
            SqlBulkCopyOptions.CheckConstraints |
            SqlBulkCopyOptions.FireTriggers |
            (keepIdentity
                ? SqlBulkCopyOptions.KeepIdentity
                : SqlBulkCopyOptions.Default) |
            (isMemoryOptimized
                ? SqlBulkCopyOptions.Default
                : SqlBulkCopyOptions.TableLock);

        // configure the bulk copy operation
        using var bulkOperation = new SqlBulkCopy(connection, bulkCopyOptions, sqlTransaction)
        {
            BatchSize = batchSize.Clamp(Constants.MinBatchSize, Constants.MaxBatchSize),
            DestinationTableName = table.Provider.QuoteTable(table.TableName, table.Schema),
            EnableStreaming = true,
            BulkCopyTimeout = timeoutSeconds.ClampMin(Constants.InfiniteTimeoutSeconds),
            NotifyAfter = notifyCallback is not null
                ? notifyAfter.Clamp(Constants.MinNotifyAfter, Constants.MaxNotifyAfter)
                : Constants.DefaultNotifyAfter
        };

        // Prepare and wire up the rows copied event for notification and row count updates.
        long rowsCopiedCount = default;

        // local event handler
        void onRowsCopied(object s, SqlRowsCopiedEventArgs e)
        {
            Interlocked.Exchange(ref rowsCopiedCount, e.RowsCopied);
            notifyCallback?.Invoke(table, e.RowsCopied);
        }

        bulkOperation.SqlRowsCopied += onRowsCopied;

        // Obtain a list of the source columns
        var sourceColumns = new List<string>();
        for (var i = 0; i < dataReader.FieldCount; i++)
        {
            var columnName = dataReader.GetName(i);
            if (string.IsNullOrWhiteSpace(columnName))
                continue;

            sourceColumns.Add(columnName);
        }

        // Map to the target columns matching names (ignore case)
        foreach (var column in table.Columns)
        {
            var sourceColumn = sourceColumns.FirstOrDefault(c => c.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase));
            if (sourceColumn is null)
                continue;

            bulkOperation.ColumnMappings.Add(sourceColumn, column.ColumnName);
        }

        if (bulkOperation.ColumnMappings.Count <= 0)
            throw new ArgumentException("The provided data reader does not contain columns that match to the target.", nameof(dataReader));

        // Execute the bulk insert operation.
        try
        {
            // Execute truncate command if truncate was requested.
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

            // bulk inseret using the streaming operation.
            await bulkOperation.WriteToServerAsync(dataReader, ct).ConfigureAwait(false);

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

        return Interlocked.Read(ref rowsCopiedCount);
    }
}
