namespace Swan.Data;

/// <summary>
/// Represents a table and schema that is bound to a specific connection.
/// </summary>
public interface ITableContext : IDbTableSchema, IConnected
{
    /// <summary>
    /// Builds a command and its parameters that can be used to insert
    /// records into this table.
    /// </summary>
    /// <param name="transaction">An optional transaction.</param>
    /// <returns>The command.</returns>
    IDbCommand BuildInsertCommand(IDbTransaction? transaction = null)
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

    IDbCommand BuildUpdateCommand(IDbTransaction? transaction = null)
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
