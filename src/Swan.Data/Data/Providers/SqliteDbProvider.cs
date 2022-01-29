namespace Swan.Data.Providers;

internal class SqliteDbProvider : DbProvider
{
    public override string ParameterPrefix { get; } = "$";

    public override IDbTypeMapper TypeMapper { get; } = new SqliteTypeMapper();

    public override Func<IDbColumnSchema> ColumnSchemaFactory { get; } = () => new SqliteColumn();

    public override DbCommand CreateListTablesCommand(DbConnection connection)
    {
        return connection is null
            ? throw new ArgumentNullException(nameof(connection))
            : connection
            .BeginCommandText("SELECT name AS [Name], '' AS [Schema] FROM (SELECT * FROM sqlite_schema UNION ALL SELECT * FROM sqlite_temp_schema) WHERE type= 'table' ORDER BY name")
            .EndCommandText();
    }

    public override string? GetColumnDdlString(IDbColumnSchema column)
    {
        return column is null
            ? throw new ArgumentNullException(nameof(column))
            : column.IsIdentity && column.DataType.TypeInfo().IsNumeric
            ? $"{QuoteField(column.Name),16} INTEGER PRIMARY KEY"
            : base.GetColumnDdlString(column);
    }

    public override bool TryGetSelectLastInserted(IDbTableSchema table, [MaybeNullWhen(false)] out string? commandText)
    {
        commandText = null;
        if (table.IdentityKeyColumn is null || table.KeyColumns.Count != 1)
            return false;

        var quotedFields = string.Join(", ", table.Columns.Select(c => QuoteField(c.Name)));
        var quotedTable = QuoteTable(table.TableName, table.Schema);
        var quotedKeyField = QuoteField(table.IdentityKeyColumn.Name);

        commandText = $"SELECT {quotedFields} FROM {quotedTable} WHERE _rowid_ = last_insert_rowid() LIMIT 1";
        return true;
    }
}
