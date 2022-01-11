namespace Swan.Data.Extensions;

/// <summary>
/// Provides shortcut methods to generate and execute commands
/// using table-aware contexts.
/// </summary>
public static class TableExtensions
{
    /// <summary>
    /// Generates a SELECT statement listing all the fields.
    /// You are supposed to continue this command text.
    /// </summary>
    /// <param name="table">The table context.</param>
    /// <returns>The command text.</returns>
    public static CommandSource BeginSelectText(this ITableContext table) => table is null
        ? throw new ArgumentNullException(nameof(table))
        : table.Columns is null
        ? throw new ArgumentException("Table connection must be set.", nameof(table))
        : new CommandSource(table.Connection).Select(table.Columns.Select(c => c.Name).ToArray()).From(table.TableName, table.Schema);

    public static DbCommand SelectAll(this ITableContext table) =>
        table.BeginSelectText().EndCommandText();

    public static DbCommand SelectByKey(this ITableContext table, object? key = default)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        var keyColumns = table.Columns.Where(c => c.IsKey).ToArray();
        var command = table
            .BeginSelectText().Where()
            .FieldsAndParameters(
                keyColumns.Select(c => c.Name).ToArray(), itemSeparator: "AND")
            .EndCommandText()
            .DefineParameters(keyColumns);

        if (key != null)
        {
            if (keyColumns.Length == 1 && key.GetType().TypeInfo().IsBasicType)
                command.SetParameter(keyColumns[0].Name, key);
            else
                command.SetParameters(key);
        }

        return command;
    }

    /// <summary>
    /// Generates a command that inserts a signle record into the specified table.
    /// It also defines de parameters according to the column schema.
    /// You can optionally pass an object to set parameter values.
    /// </summary>
    /// <param name="table">The table context.</param>
    /// <param name="param">The optional object where parameters will be read from.</param>
    /// <returns>The generated command.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DbCommand Insert(this ITableContext table, object? param = default)
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
            .EndCommandText()
            .DefineParameters(insertColumns)
            .SetParameters(param);

        return command;
    }
}
