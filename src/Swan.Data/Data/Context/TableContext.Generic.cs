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
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional table schema.</param>
    public TableContext(DbConnection connection, string tableName, string? schema = null)
        : base(connection, tableName, schema)
    {
        // placeholder
    }

    /// <inheritdoc />
    public virtual Func<IDataRecord, T> Deserializer { get; set; } = new((r) => r.ParseObject<T>());

    /// <inheritdoc />
    public virtual IEnumerable<T> Query(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default)
    {
        var command = new CommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        return command.Query(Deserializer, CommandBehavior.Default);
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

        using var command = BuildInsertCommand(transaction).SetParameters(item);

        if (!AppendSelectBackTo(command))
        {
            _ = command.ExecuteNonQuery();
            return default;
        }

        return command.FirstOrDefault(Deserializer);
    }

    /// <inheritdoc />
    public virtual async Task<T?> InsertOneAsync(T item, DbTransaction? transaction = default, CancellationToken ct = default)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var command = BuildInsertCommand(transaction).SetParameters(item);

        try
        {
            if (!AppendSelectBackTo(command))
            {
                _ = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                return default;
            }

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

    /// <summary>
    /// Appends appropriate sql statements to the command in order to retrieve the last inserted
    /// record to the table.
    /// </summary>
    /// <param name="command">The command to append the select-back statement to.</param>
    /// <returns>True if select-backs are supported, false otherwise.</returns>
    private bool AppendSelectBackTo(DbCommand command)
    {
        if (Provider.Kind == ProviderKind.Unknown || IdentityKeyColumn is null || KeyColumns.Count != 1)
            return false;

        var quotedFields = string.Join(", ", Columns.Select(c => Provider.QuoteField(c.Name)));
        var quotedTable = Provider.QuoteTable(TableName, Schema);
        var quotedKeyField = Provider.QuoteField(IdentityKeyColumn.Name);

        switch (Provider.Kind)
        {
            case ProviderKind.SqlServer:
                command.AppendText($"; SELECT TOP 1 {quotedFields} FROM {quotedTable} WHERE {quotedKeyField} = SCOPE_IDENTITY();");
                return true;
            case ProviderKind.Sqlite:
                command.AppendText($"; SELECT {quotedFields} FROM {quotedTable} WHERE _rowid_ = last_insert_rowid() LIMIT 1;");
                return true;
            case ProviderKind.MySql:
                command.AppendText($"; SELECT {quotedFields} FROM {quotedTable} WHERE {quotedKeyField} = LAST_INSERT_ID() LIMIT 1;");
                return true;
            default:
                return false;
        }
    }
}
