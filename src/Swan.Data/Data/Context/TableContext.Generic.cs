namespace Swan.Data.Context;

/// <summary>
/// A table context that maps between a given type and a data store.
/// </summary>
/// <typeparam name="T">The type this table context maps to.</typeparam>
public class TableContext<T> : TableContext, ITableContext<T>
    where T : class
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableContext{T}"/> class.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="schema">The table schema information.</param>
    public TableContext(DbConnection connection, IDbTableSchema schema)
        : base(connection, schema)
    {
        // placeholder
    }

    /// <inheritdoc />
    public virtual Func<IDataRecord, T> Deserializer { get; set; } = r => r.ParseObject<T>();

    /// <inheritdoc />
    public virtual IEnumerable<T> Query(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default)
    {
        var command = new CommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        return command.Query(Deserializer);
    }

    /// <inheritdoc />
    public virtual async IAsyncEnumerable<T> QueryAsync(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var command = new CommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        var enumerable = command.QueryAsync(Deserializer, CommandBehavior.Default, ct);
        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            yield return item;
    }

    /// <inheritdoc />
    public virtual T? FirstOrDefault(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default)
    {
        var command = new CommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        return command.FirstOrDefault(Deserializer);
    }

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default,
        CancellationToken ct = default)
    {
        var command = new CommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        return await command.FirstOrDefaultAsync(deserialize: Deserializer, ct: ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual T? InsertOne(T item, DbTransaction? transaction = null)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        Connection.EnsureConnected();
        using var command = BuildInsertCommand(transaction).SetParameters(item);

        if (!Provider.TryGetSelectLastInserted(this, out var selectBack))
        {
            _ = command.ExecuteNonQuery();
            return default;
        }

        command.AppendText($";\r\n{selectBack};");
        return command.FirstOrDefault(Deserializer);
    }

    /// <inheritdoc />
    public virtual async Task<T?> InsertOneAsync(T item, DbTransaction? transaction = default, CancellationToken ct = default)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        await Connection.EnsureConnectedAsync(ct).ConfigureAwait(false);
        var command = BuildInsertCommand(transaction).SetParameters(item);

        try
        {
            if (!Provider.TryGetSelectLastInserted(this, out var selectBack))
            {
                _ = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                return default;
            }

            command.AppendText($";\r\n{selectBack};");
            return await command.FirstOrDefaultAsync(Deserializer, CommandBehavior.SingleRow, ct).ConfigureAwait(false);
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual int InsertMany(IEnumerable<T> items, DbTransaction? transaction = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        Connection.EnsureConnected();
        var result = 0;
        var command = BuildInsertCommand(transaction);

        try
        {
            command.TryPrepare();

            foreach (var item in items)
            {
                if (item is null)
                    continue;

                command.SetParameters(item);
                result += command.ExecuteNonQuery();
            }
        }
        finally
        {
            command.Parameters?.Clear();
            command.Dispose();
        }

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<int> InsertManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        await Connection.EnsureConnectedAsync(ct).ConfigureAwait(false);
        var result = default(int);
        var command = BuildInsertCommand(transaction);

        try
        {
            await command.TryPrepareAsync(ct).ConfigureAwait(false);

            foreach (var item in items)
            {
                if (item is null)
                    continue;

                command.SetParameters(item);
                result += await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc />
    public virtual int UpdateOne(T item, DbTransaction? transaction = default)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        Connection.EnsureConnected();
        var command = BuildUpdateCommand(transaction).SetParameters(item);

        try
        {
            return command.ExecuteNonQuery();
        }
        finally
        {
            command.Parameters?.Clear();
            command.Dispose();
        }
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateOneAsync(T item, DbTransaction? transaction = null, CancellationToken ct = default)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        await Connection.EnsureConnectedAsync(ct).ConfigureAwait(false);
        var command = BuildUpdateCommand(transaction).SetParameters(item);
        try
        {
            return await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual int UpdateMany(IEnumerable<T> items, DbTransaction? transaction = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        Connection.EnsureConnected();
        var result = 0;
        var command = BuildUpdateCommand(transaction);

        try
        {
            command.TryPrepare();
            foreach (var item in items)
            {
                command.SetParameters(item);
                result += command.ExecuteNonQuery();
            }
        }
        finally
        {
            command.Parameters?.Clear();
            command.Dispose();
        }

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        await Connection.EnsureConnectedAsync(ct).ConfigureAwait(false);
        var result = default(int);
        var command = BuildUpdateCommand(transaction);

        try
        {
            await command.TryPrepareAsync(ct).ConfigureAwait(false);

            foreach (var item in items)
            {
                if (item is null)
                    continue;

                command.SetParameters(item);
                result += await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc />
    public virtual int DeleteOne(T item, DbTransaction? transaction = default)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        Connection.EnsureConnected();
        var command = BuildDeleteCommand(transaction).SetParameters(item);

        try
        {
            return command.ExecuteNonQuery();
        }
        finally
        {
            command.Parameters?.Clear();
            command.Dispose();
        }
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteOneAsync(T item, DbTransaction? transaction = default, CancellationToken ct = default)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        await Connection.EnsureConnectedAsync(ct).ConfigureAwait(false);
        var command = BuildDeleteCommand(transaction).SetParameters(item);
        try
        {
            return await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual int DeleteMany(IEnumerable<T> items, DbTransaction? transaction = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        Connection.EnsureConnected();
        var result = 0;
        var command = BuildDeleteCommand(transaction);

        try
        {
            command.TryPrepare();
            foreach (var item in items)
            {
                if (item is null)
                    continue;

                command.SetParameters(item);
                result += command.ExecuteNonQuery();
            }
        }
        finally
        {
            command.Parameters?.Clear();
            command.Dispose();
        }

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        await Connection.EnsureConnectedAsync(ct).ConfigureAwait(false);
        var result = 0;
        var command = BuildDeleteCommand(transaction);

        try
        {
            await command.TryPrepareAsync(ct).ConfigureAwait(false);
            foreach (var item in items)
            {
                if (item is null)
                    continue;

                command.SetParameters(item);
                result += await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }

        return result;
    }
}
