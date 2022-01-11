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
                var sequenceValue = $"(SELECT seq FROM sqlite_sequence WHERE name = '{TableName}')";
                command.AppendText($"; SELECT {quotedFields} FROM {quotedTable} WHERE _rowid_ = {sequenceValue} LIMIT 1;");
                return true;
            case ProviderKind.MySql:
                command.AppendText($"; SELECT {quotedFields} FROM {quotedTable} WHERE {quotedKeyField} = LAST_INSERT_ID() LIMIT 1;");
                return true;
            default:
                return false;
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<T> Query(string? trailingSql = default, object? param = default)
    {
        var command = new CommandSource(Connection)
            .Select(this)
            .AppendText(trailingSql)
            .EndCommandText()
            .SetParameters(param);

        return command.Query(Deserializer, CommandBehavior.Default);
    }

    /// <inheritdoc />
    public virtual async IAsyncEnumerable<T> QueryAsync(string? trailingSql = default, object? param = default,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var command = new CommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .SetParameters(param);

        var enumerable = command.QueryAsync(Deserializer, CommandBehavior.Default, ct);
        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            yield return item;
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

        return command.FirstOrDefault(CommandBehavior.SingleRow, Deserializer);
    }

    /// <inheritdoc />
    public virtual async Task<T?> InsertOneAsync(T item, DbTransaction? transaction = null, CancellationToken ct = default)
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

            return await command.FisrtOrDefaultAsync(Deserializer, CommandBehavior.SingleRow, ct).ConfigureAwait(false);
        }
        finally
        {
            command.Parameters.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual int InsertMany(IEnumerable<T> items, DbTransaction? transaction = null)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        var result = 0;
        using var command = BuildInsertCommand(transaction);
        command.TryPrepare(out _);

        foreach (var item in items)
        {
            if (item is null)
                continue;

            command.SetParameters(item);
            result += command.ExecuteNonQuery();
        }

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<int> InsertManyAsync(IEnumerable<T> items, DbTransaction? transaction = null, CancellationToken ct = default)
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
            command.Parameters.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc />
    public virtual int UpdateOne(T item, DbTransaction? transaction = null)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        using var command = BuildUpdateCommand(transaction).SetParameters(item);
        return command.ExecuteNonQuery();
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
            command.Parameters.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public int UpdateMany(IEnumerable<T> items, DbTransaction? transaction = null)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        var result = 0;
        using var command = BuildUpdateCommand(transaction);

        command.TryPrepare();
        foreach (var item in items)
        {
            command.SetParameters(item);
            result += command.ExecuteNonQuery();
        }

        return result;
    }

    /// <inheritdoc />
    public bool TryFind(T key, out T item, DbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public T DeleteOne(T item, DbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public T DeleteMany(IEnumerable<T> items, DbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }
}
