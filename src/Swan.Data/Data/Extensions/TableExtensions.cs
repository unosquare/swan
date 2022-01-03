namespace Swan.Data.Extensions;

/// <summary>
/// Provides shortcut methods to generate and execute commands
/// using table-aware contexts.
/// </summary>
public static class TableExtensions
{
    public static CommandSource BeginSelect(this ITableContext table) => table is null
        ? throw new ArgumentNullException(nameof(table))
        : table.Columns is null
        ? throw new ArgumentException("Table connection must be set.", nameof(table))
        : new CommandSource(table.Connection).Select(table.Columns.Select(c => c.Name).ToArray()).From(table.TableName, table.Schema);

    public static IDbCommand SelectAll(this ITableContext table) =>
        table.BeginSelect().EndCommand();

    public static IDbCommand SelectByKey(this ITableContext table, object key) =>
        table
            .BeginSelect().Where()
            .FieldParameters(
                table.Columns.Where(c => c.IsKey).Select(c => c.Name).ToArray(), itemSeparator: "AND")
            .EndCommand()
            .SetParameters(key);

    public static IDbCommand Insert(this ITableContext table, object? param = default)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        var insertColumns = table.Columns.Where(c => !c.IsAutoIncrement && !c.IsReadOnly).ToArray();
        var columnNames = insertColumns.Select(c => c.Name).ToArray();

        var command = new CommandSource(table.Connection)
            .InsertInto(table.TableName, table.Schema)
            .AppendText("(")
            .Fields(columnNames)
            .AppendText(") VALUES (")
            .Parameters(columnNames)
            .AppendText(")")
            .EndCommand()
            .DefineParameters(insertColumns);

        if (param is not null)
            command.SetParameters(param);

        return command;
    }
}
