namespace Swan.Data.Context;

using System.Data.Common;

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
    public DbConnection Connection { get; }

    /// <inheritdoc />
    public DbProvider Provider { get; }

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
    public DbCommand BuildTableCommand(DbTransaction? transaction = null) =>
        Provider.CreateTableDdlCommand(Connection, this).WithTransaction(transaction);

    /// <inheritdoc />
    public ITableContext ExecuteTableCommand(DbTransaction? transaction = null)
    {
        Connection.EnsureConnected();
        using var command = BuildTableCommand(transaction);
        _ = command.ExecuteNonQuery();

        var schema = Load(Connection, TableName, Schema, transaction);
        return new TableContext(Connection, schema);
    }

    /// <inheritdoc />
    public async Task<ITableContext> ExecuteTableCommandAsync(DbTransaction? transaction = null, CancellationToken ct = default)
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
}
