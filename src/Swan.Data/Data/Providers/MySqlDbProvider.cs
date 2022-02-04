namespace Swan.Data.Providers;

internal class MySqlDbProvider : DbProvider
{
    public override IDbTypeMapper TypeMapper { get; } = new MySqlTypeMapper();

    public override string QuotePrefix { get; } = "`";

    public override string QuoteSuffix { get; } = "`";

    public override DbCommand CreateListTablesCommand(DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        var database = connection.Database;
        return connection
            .BeginCommandText($"SELECT `table_name` AS `Name`, '' AS `Schema` FROM `information_schema`.`tables` WHERE `table_schema` = {QuoteParameter(nameof(database))}")
            .EndCommandText()
            .SetParameter(nameof(database), database);
    }

    public override string? GetColumnDdlString(IDbColumnSchema column) => column is null
        ? throw new ArgumentNullException(nameof(column))
        : !TypeMapper.TryGetProviderTypeFor(column.DataType, out var providerType)
        ? default
        : column.IsIdentity && column.DataType.TypeInfo().IsNumeric
        ? $"{QuoteField(column.Name),16} {providerType} NOT NULL AUTO_INCREMENT"
        : base.GetColumnDdlString(column);

    public override bool TryGetSelectLastInserted(IDbTableSchema table, [MaybeNullWhen(false)] out string? commandText)
    {
        commandText = null;
        if (table.IdentityKeyColumn is null || table.KeyColumns.Count != 1)
            return false;

        var quotedFields = string.Join(", ", table.Columns.Select(c => QuoteField(c.Name)));
        var quotedTable = QuoteTable(table.TableName, table.Schema);
        var quotedKeyField = QuoteField(table.IdentityKeyColumn.Name);

        commandText = $"SELECT {quotedFields} FROM {quotedTable} WHERE {quotedKeyField} = LAST_INSERT_ID() LIMIT 1";
        return true;
    }
}
