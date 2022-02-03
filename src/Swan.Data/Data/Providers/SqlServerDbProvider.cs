namespace Swan.Data.Providers;

internal class SqlServerDbProvider : DbProvider
{
    public override string DefaultSchemaName { get; } = "dbo";

    public override IDbTypeMapper TypeMapper { get; } = new SqlServerTypeMapper();

    public override Func<IDbColumnSchema> ColumnSchemaFactory { get; } = () => new SqlServerColumn();

    public override DbCommand CreateListTablesCommand(DbConnection connection)
    {
        return connection is null
            ? throw new ArgumentNullException(nameof(connection))
            : connection
            .BeginCommandText("SELECT [TABLE_NAME] AS [Name], [TABLE_SCHEMA] AS [Schema] FROM [INFORMATION_SCHEMA].[TABLES]")
            .EndCommandText();
    }

    public override DbCommand CreateTableDdlCommand(DbConnection connection, IDbTableSchema table)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (!table.Columns.Any())
            throw new InvalidOperationException("Cannot generate DDL code with no provided columns.");

        var schemaName = string.IsNullOrWhiteSpace(table.Schema) ? DefaultSchemaName : table.Schema;
        var quotedTableName = QuoteTable(table.TableName, schemaName);
        var orderedFields = table.Columns.OrderBy(c => c.Ordinal).ThenBy(c => c.Name);
        var builder = new StringBuilder($"IF OBJECT_ID('{quotedTableName}') IS NULL\r\nCREATE TABLE {quotedTableName} (\r\n")
            .Append(string.Join($",\r\n", orderedFields.Select(c => $"    {GetColumnDdlString(c)}").ToArray()))
            .AppendLine("\r\n);");

        return connection
            .BeginCommandText(builder.ToString())
            .EndCommandText();
    }

    public override string? GetColumnDdlString(IDbColumnSchema column) => column is null
        ? throw new ArgumentNullException(nameof(column))
        : !TypeMapper.TryGetProviderTypeFor(column.DataType, out var providerType) 
        ? default
        : column.IsIdentity && column.DataType.TypeInfo().IsNumeric
        ? $"{QuoteField(column.Name),16} {providerType} IDENTITY NOT NULL PRIMARY KEY"
        : base.GetColumnDdlString(column);

    public override bool TryGetSelectLastInserted(IDbTableSchema table, [MaybeNullWhen(false)] out string? commandText)
    {
        commandText = null;
        if (table.IdentityKeyColumn is null || table.KeyColumns.Count != 1)
            return false;

        var quotedFields = string.Join(", ", table.Columns.Select(c => QuoteField(c.Name)));
        var quotedTable = QuoteTable(table.TableName, table.Schema);
        var quotedKeyField = QuoteField(table.IdentityKeyColumn.Name);

        commandText = $"SELECT TOP 1 {quotedFields} FROM {quotedTable} WHERE {quotedKeyField} = SCOPE_IDENTITY()";
        return true;
    }

    public override string GetLimitClause(int skip, int take) =>
        $"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
}

