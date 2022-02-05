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
    /// <param name="schema">The table schema information.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public TableContext(DbConnection connection, IDbTableSchema schema)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (schema is null)
            throw new ArgumentNullException(nameof(schema));

        TableSchema = schema;
        Connection = connection;
        Provider = connection.Provider();
    }

    /// <inheritdoc />
    public virtual DbCommand BuildInsertCommand(DbTransaction? transaction = default)
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
    public virtual DbCommand BuildUpdateCommand(DbTransaction? transaction = default)
    {
        var settableFields = UpdateableColumns.Select(c => c.Name).ToArray();
        var keyFields = KeyColumns.Select(c => c.Name).ToArray();

        var keyPairs = string.Join(" AND ", keyFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));
        var setPairs = string.Join(", ", settableFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));
        var commandText = $"UPDATE {Provider.QuoteTable(TableName, Schema)} SET {setPairs} WHERE {keyPairs}";

        return new CommandSource(Connection, commandText)
            .EndCommandText()
            .DefineParameters(UpdateableColumns.Union(KeyColumns))
            .WithTransaction(transaction);
    }

    /// <inheritdoc />
    public virtual DbCommand BuildDeleteCommand(DbTransaction? transaction = default)
    {
        var keyFields = KeyColumns.Select(c => c.Name).ToArray();
        var keyPairs = string.Join(" AND ", keyFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));
        var commandText = $"DELETE FROM {Provider.QuoteTable(TableName, Schema)} WHERE {keyPairs}";

        return new CommandSource(Connection, commandText)
            .EndCommandText()
            .DefineParameters(KeyColumns)
            .WithTransaction(transaction);
    }

    /// <inheritdoc />
    public virtual DbCommand BuildSelectCommand(DbTransaction? transaction = default)
    {
        var keyFields = KeyColumns.Select(c => c.Name).ToArray();
        var keyPairs = string.Join(" AND ", keyFields.Select(c => $"{Provider.QuoteField(c)} = {Provider.QuoteParameter(c)}"));

        return new CommandSource(Connection)
            .Select(this).Where(keyPairs)
            .EndCommandText()
            .DefineParameters(KeyColumns)
            .WithTransaction(transaction);
    }

}
