namespace Swan.Data.Context;

/// <summary>
/// A table context that maps between a given type and a data store.
/// </summary>
/// <typeparam name="T">The type this table context maps to.</typeparam>
internal class TableContext<T> : TableContext, ITableContext<T>, ITableBuilder<T>
    where T : class
{
    private readonly ITypeInfo TypeInfo = typeof(T).TypeInfo();

    /// <summary>
    /// Creates a new instance of the <see cref="TableContext{T}"/> class.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="schema">The table schema information.</param>
    public TableContext(DbConnection connection, IDbTableSchema schema)
        : base(connection, schema)
    {
        // setup the default deserializer
        Deserializer = (r) => (r.ParseObject(TypeInfo) as T)!;
    }

    /// <inheritdoc />
    public virtual Func<IDataRecord, T> Deserializer { get; set; }

    /// <inheritdoc />
    public virtual IEnumerable<T> Query(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default) =>
        Query(Deserializer, trailingSql, param, transaction).Cast<T>();

    /// <inheritdoc />
    public virtual IAsyncEnumerable<T> QueryAsync(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default, CancellationToken ct = default)
    {
        var command = new DbCommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        var enumerable = command.QueryAsync(Deserializer, CommandBehavior.Default, ct);
        enumerable.ConfigureAwait(false);
        return enumerable;
    }

    /// <inheritdoc />
    public virtual T? FirstOrDefault(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default) =>
        FirstOrDefault(Deserializer, trailingSql, param, transaction) as T;

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default,
        CancellationToken ct = default) =>
        (await FirstOrDefaultAsync(Deserializer, trailingSql, param, transaction, ct).ConfigureAwait(false)) as T;

    /// <inheritdoc />
    public virtual T? InsertOne(T item, DbTransaction? transaction = null) =>
        InsertOne(Deserializer, item, transaction) as T;

    /// <inheritdoc />
    public virtual async Task<T?> InsertOneAsync(T item, DbTransaction? transaction = default, CancellationToken ct = default) =>
        (await InsertOneAsync(Deserializer, item, transaction, ct).ConfigureAwait(false)) as T;

    /// <inheritdoc />
    public virtual int InsertMany(IEnumerable<T> items, DbTransaction? transaction = default) =>
        base.InsertMany(items, transaction);

    /// <inheritdoc />
    public virtual Task<int> InsertManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default) =>
        base.InsertManyAsync(items, transaction, ct);

    /// <inheritdoc />
    public virtual int UpdateOne(T item, DbTransaction? transaction = default) =>
        base.UpdateOne(item, transaction);

    /// <inheritdoc />
    public virtual Task<int> UpdateOneAsync(T item, DbTransaction? transaction = null, CancellationToken ct = default) =>
        base.UpdateOneAsync(item, transaction, ct);

    /// <inheritdoc />
    public virtual int UpdateMany(IEnumerable<T> items, DbTransaction? transaction = default) =>
        base.UpdateMany(items, transaction);

    /// <inheritdoc />
    public virtual Task<int> UpdateManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default) =>
        base.UpdateManyAsync(items, transaction, ct);

    /// <inheritdoc />
    public virtual int DeleteOne(T item, DbTransaction? transaction = default) =>
        base.DeleteOne(item, transaction);

    /// <inheritdoc />
    public virtual Task<int> DeleteOneAsync(T item, DbTransaction? transaction = default, CancellationToken ct = default) =>
        base.DeleteOneAsync(item, transaction, ct);

    /// <inheritdoc />
    public virtual int DeleteMany(IEnumerable<T> items, DbTransaction? transaction = default) =>
        base.DeleteMany(items, transaction);

    /// <inheritdoc />
    public virtual Task<int> DeleteManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default) =>
        base.DeleteManyAsync(items, transaction, ct);

    /// <inheritdoc />
    ITableContext<T> ITableBuilder<T>.ExecuteTableCommand(DbTransaction? transaction)
    {
        Connection.EnsureConnected();
        using var command = BuildTableCommand(transaction);
        _ = command.ExecuteNonQuery();

        var schema = Load(Connection, TableName, Schema, transaction);
        return new TableContext<T>(Connection, schema);
    }

    /// <inheritdoc />
    async Task<ITableContext<T>> ITableBuilder<T>.ExecuteTableCommandAsync(DbTransaction? transaction, CancellationToken ct)
    {
        await Connection.EnsureConnectedAsync(ct);
        await using var command = BuildTableCommand(transaction);
        _ = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        var schema = await LoadAsync(Connection, TableName, Schema, transaction, ct).ConfigureAwait(false);
        return new TableContext<T>(Connection, schema);
    }

    /// <inheritdoc />
    ITableBuilder<T> ITableBuilder<T>.AddColumn(IDbColumnSchema column)
    {
        AddColumn(column);
        return this;
    }

    /// <inheritdoc />
    ITableBuilder<T> ITableBuilder<T>.RemoveColumn(string columnName)
    {
        RemoveColumn(columnName);
        return this;
    }
}
