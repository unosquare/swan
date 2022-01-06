namespace Swan.Data;

/// <summary>
/// Represents a table and schema that is bound to a specific connection
/// and that also maps to a specific CLR type.
/// </summary>
/// <typeparam name="T">The CLR type to map the table to.</typeparam>
public interface ITableContext<T> : ITableContext
    where T : class
{
    /// <summary>
    /// Specifies a callback function that turns a <see cref="IDataRecord"/>
    /// into an object of the mapped type. If no deserializer is specified,
    /// a default one will be used.
    /// </summary>
    Func<IDataRecord, T>? Deserializer { get; }

    /// <summary>
    /// Inserts an item of the given type to the database
    /// and if the table has defined a single, auto incremental key
    /// column (identity column), returns the inserted item.
    /// </summary>
    /// <param name="item">The item to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The newly inserted item whenever possible.</returns>
    T? InsertOne(T item, IDbTransaction? transaction = null)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        using var command = BuildInsertCommand(transaction).SetParameters(item);

        if (Provider.Kind == ProviderKind.Unknown || IdentityKeyColumn is null || KeyColumns.Count != 1)
        {
            _ = command.ExecuteNonQuery();
            return default;
        }

        var quotedFields = string.Join(", ", Columns.Select(c => Provider.QuoteField(c.Name)));
        var quotedTable = Provider.QuoteTable(TableName, Schema);
        var quotedKeyField = Provider.QuoteField(IdentityKeyColumn.Name);

        switch (Provider.Kind)
        {
            case ProviderKind.SqlServer:
                command.AppendText($"; SELECT TOP 1 {quotedFields} FROM {quotedTable} WHERE {quotedKeyField} = SCOPE_IDENTITY();");
                break;
            case ProviderKind.Sqlite:
                var sequenceValue = $"(SELECT seq FROM sqlite_sequence WHERE name = '{TableName}')";
                command.AppendText($"; SELECT {quotedFields} FROM {quotedTable} WHERE _rowid_ = {sequenceValue} LIMIT 1;");
                break;
            case ProviderKind.MySql:
                command.AppendText($"; SELECT {quotedFields} FROM {quotedTable} WHERE {quotedKeyField} = LAST_INSERT_ID() LIMIT 1;");
                break;
            default:
                _ = command.ExecuteNonQuery();
                return default;
        }

        return command.Query<T>(CommandBehavior.SingleRow, Deserializer).FirstOrDefault();
    }

    /// <summary>
    /// Inserts a set of records of the given type to the table.
    /// By defualt, this implementation does not represent a bulk insert operation.
    /// </summary>
    /// <param name="items">The items to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of records affected.</returns>
    int InsertMany(IEnumerable<T> items, IDbTransaction? transaction = null)
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

    /// <summary>
    /// Inserts a set of records of the given type to the table.
    /// By defualt, this implementation does not represent a bulk insert operation.
    /// </summary>
    /// <param name="items">The items to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of records affected.</returns>
    async Task<int> InsertManyAsync(IEnumerable<T> items, IDbTransaction? transaction = null, CancellationToken ct = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        var result = 0;
        using var command = BuildInsertCommand(transaction);
        await command.TryPrepareAsync(ct);

        foreach (var item in items)
        {
            if (item is null)
                continue;

            command.SetParameters(item);
            result += command is DbCommand cmd
                ? await cmd.ExecuteNonQueryAsync(ct)
                : command.ExecuteNonQuery();
        }

        return result;
    }

    /// <summary>
    /// Updates a single item. Key values must be correctly set in the passed object.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int UpdateOne(T item, IDbTransaction? transaction = null)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        using var command = BuildUpdateCommand(transaction);
        command.SetParameters(item);

        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Updates 
    /// </summary>
    /// <param name="items"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    int UpdateMany(IEnumerable<T> items, IDbTransaction? transaction = null)
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

    bool TryFind(T key, out T item, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    T DeleteOne(T item, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }

    T DeleteMany(IEnumerable<T> items, IDbTransaction? transaction = null)
    {
        throw new NotImplementedException();
    }
}
