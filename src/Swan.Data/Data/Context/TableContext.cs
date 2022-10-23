namespace Swan.Data.Context;

using System.Collections.Generic;

/// <summary>
/// Represents table structure information bound to a particular connection
/// and from which you can issue table specific CRUD commands.
/// </summary>
internal partial class TableContext : DbTableSchema, ITableContext, ITableBuilder
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableContext"/> class.
    /// </summary>
    /// <param name="connection">The connection to associate this context to.</param>
    /// <param name="schema">The table schema information.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public TableContext(DbConnection connection, IDbTableSchema schema)
        : base(schema)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Provider = connection.Provider();
    }

    /// <inheritdoc />
    public DbConnection Connection { get; }

    /// <inheritdoc />
    public DbProvider Provider { get; }

    /// <inheritdoc />
    public virtual DbCommand BuildInsertCommand(DbTransaction? transaction = default)
    {
        var insertColumns = InsertableColumns;
        var columnNames = insertColumns.Select(c => c.Name).ToArray();

        var command = new DbCommandSource(Connection)
            .InsertInto(TableName, Schema)
            .AppendText("(")
            .Fields(columnNames)
            .AppendText(") VALUES (")
            .Parameters(columnNames)
            .AppendText(")")
            .EndCommandText()
            .DefineParameters(insertColumns);

        if (transaction != null)
            command.Transaction = transaction;

        return command;
    }

    /// <inheritdoc />
    public virtual DbCommand BuildUpdateCommand(DbTransaction? transaction = default)
    {
        var settableFields = UpdateableColumns.Select(c => c.Name).ToArray();
        var keyFields = KeyColumns.Select(c => c.Name).ToArray();

        var keyPairs = string.Join(" AND ",
            keyFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));
        var setPairs = string.Join(", ",
            settableFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));
        var commandText = $"UPDATE {Provider.QuoteTable(TableName, Schema)} SET {setPairs} WHERE {keyPairs}";

        return new DbCommandSource(Connection, commandText)
            .EndCommandText()
            .DefineParameters(UpdateableColumns.Union(KeyColumns))
            .WithTransaction(transaction);
    }

    /// <inheritdoc />
    public virtual DbCommand BuildDeleteCommand(DbTransaction? transaction = default)
    {
        var keyFields = KeyColumns.Select(c => c.Name).ToArray();
        var keyPairs = string.Join(" AND ",
            keyFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));
        var commandText = $"DELETE FROM {Provider.QuoteTable(TableName, Schema)} WHERE {keyPairs}";

        return new DbCommandSource(Connection, commandText)
            .EndCommandText()
            .DefineParameters(KeyColumns)
            .WithTransaction(transaction);
    }

    /// <inheritdoc />
    public virtual DbCommand BuildSelectCommand(DbTransaction? transaction = default)
    {
        var keyFields = KeyColumns.Select(c => c.Name).ToArray();
        var keyPairs = string.Join(" AND ",
            keyFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));

        return new DbCommandSource(Connection)
            .Select(this).Where(keyPairs)
            .EndCommandText()
            .DefineParameters(KeyColumns)
            .WithTransaction(transaction);
    }

    /// <inheritdoc />
    public virtual DbCommand BuildTableCommand(DbTransaction? transaction = null) =>
        Provider.CreateTableDdlCommand(Connection, this).WithTransaction(transaction);

    /// <inheritdoc />
    public virtual ITableContext ExecuteTableCommand(DbTransaction? transaction = null)
    {
        Connection.EnsureConnected();
        using var command = BuildTableCommand(transaction);
        _ = command.ExecuteNonQuery();

        var schema = Load(Connection, TableName, Schema, transaction);
        return new TableContext(Connection, schema);
    }

    /// <inheritdoc />
    public virtual async Task<ITableContext> ExecuteTableCommandAsync(DbTransaction? transaction = null, CancellationToken ct = default)
    {
        await Connection.EnsureConnectedAsync(ct);
        await using var command = BuildTableCommand(transaction);
        _ = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        var schema = await LoadAsync(Connection, TableName, Schema, transaction, ct).ConfigureAwait(false);
        return new TableContext(Connection, schema);
    }

    /// <inheritdoc />
    ITableBuilder ITableBuilder.AddColumn(IDbColumnSchema column)
    {
        AddColumn(column);
        return this;
    }

    /// <inheritdoc />
    ITableBuilder ITableBuilder.RemoveColumn(string columnName)
    {
        RemoveColumn(columnName);
        return this;
    }

    /// <inheritdoc />
    public virtual IEnumerable Query(Func<IDataRecord, object> deserializer, string? trailingSql = null, object? param = null, DbTransaction? transaction = null)
    {
        var command = new DbCommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        return command.Query(deserializer);
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<object> QueryAsync(Func<IDataRecord, object> deserializer, string? trailingSql = null, object? param = null, DbTransaction? transaction = null, CancellationToken ct = default)
    {
        var command = new DbCommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        var enumerable = command.QueryAsync(deserializer, CommandBehavior.Default, ct);
        enumerable.ConfigureAwait(false);

        return enumerable;
    }

    /// <inheritdoc />
    public virtual object? FirstOrDefault(Func<IDataRecord, object> deserializer, string? trailingSql = null, object? param = null, DbTransaction? transaction = null)
    {
        var command = new DbCommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        return command.FirstOrDefault(deserializer);
    }

    /// <inheritdoc />
    public virtual async Task<object?> FirstOrDefaultAsync(Func<IDataRecord, object> deserializer, string? trailingSql = null, object? param = null, DbTransaction? transaction = null, CancellationToken ct = default)
    {
        var command = new DbCommandSource(Connection)
            .Select(this).AppendText(trailingSql).EndCommandText()
            .WithTransaction(transaction)
            .SetParameters(param);

        return await command.FirstOrDefaultAsync(deserializer, ct: ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual object? InsertOne(Func<IDataRecord, object> deserializer, object item, DbTransaction? transaction = null)
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
        return command.FirstOrDefault(deserializer);
    }

    /// <inheritdoc />
    public virtual async Task<object?> InsertOneAsync(Func<IDataRecord, object> deserializer, object item, DbTransaction? transaction = null, CancellationToken ct = default)
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
            return await command.FirstOrDefaultAsync(deserializer, CommandBehavior.SingleRow, ct).ConfigureAwait(false);
        }
        finally
        {
            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual int InsertMany(IEnumerable items, DbTransaction? transaction = null)
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
    public virtual async Task<int> InsertManyAsync(IEnumerable items, DbTransaction? transaction = null, CancellationToken ct = default)
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
    public virtual int UpdateOne(object item, DbTransaction? transaction = null)
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
    public virtual async Task<int> UpdateOneAsync(object item, DbTransaction? transaction = null, CancellationToken ct = default)
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
    public int UpdateMany(IEnumerable items, DbTransaction? transaction = null)
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
    public virtual async Task<int> UpdateManyAsync(IEnumerable items, DbTransaction? transaction = null, CancellationToken ct = default)
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
    public virtual int DeleteOne(object item, DbTransaction? transaction = null)
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
    public virtual async Task<int> DeleteOneAsync(object item, DbTransaction? transaction = null, CancellationToken ct = default)
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
    public virtual int DeleteMany(IEnumerable items, DbTransaction? transaction = null)
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
    public virtual async Task<int> DeleteManyAsync(IEnumerable items, DbTransaction? transaction = null, CancellationToken ct = default)
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
