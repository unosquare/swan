namespace Swan.Data.Context;

/// <summary>
/// Represents table structure information bound to a particular connection
/// and from which you can issue table specific CRUD commands.
/// </summary>
public partial class TableContext : ITableContext
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableContext"/> class.
    /// </summary>
    /// <param name="connection">The connection to associate this context to.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The name of the schema.</param>
    public TableContext(DbConnection connection, string tableName, string? schema = null)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        TableSchema = LoadTableSchema(connection, tableName, schema);
        Connection = connection;
    }

    /// <inheritdoc />
    public DbCommand BuildInsertCommand(DbTransaction? transaction = null)
    {
        var insertColumns = InsertableColumns;
        var columnNames = insertColumns.Select(c => c.Name).ToArray();

        var command = new CommandSource(Connection)
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
    public DbCommand BuildUpdateCommand(DbTransaction? transaction = null)
    {
        var settableFields = UpdateableColumns.Select(c => c.Name).ToArray();
        var keyFields = KeyColumns.Select(c => c.Name).ToArray();

        var keyPairs = string.Join(" AND ", keyFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));
        var setPairs = string.Join(", ", settableFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));
        var updateText = $"UPDATE {Provider.QuoteTable(TableName, Schema)} SET {setPairs} WHERE {keyPairs}";

        return new CommandSource(Connection, updateText)
            .EndCommandText()
            .DefineParameters(UpdateableColumns.Union(KeyColumns))
            .WithTransaction(transaction);
    }

}
