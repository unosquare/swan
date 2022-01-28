namespace Swan.Data.Providers;

internal class MySqlDbProvider : DbProvider
{
    public override IDbTypeMapper TypeMapper { get; } = new MySqlTypeMapper();

    public override string QuotePrefix { get; } = "`";

    public override string QuoteSuffix { get; } = "`";

    public override DbCommand GetListTablesCommand(DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        connection.EnsureIsValid();
        var database = connection.Database;

        return connection
            .BeginCommandText($"SELECT `table_name` AS `Name`, '' AS `Schema` FROM `information_schema`.`tables` WHERE `table_schema` = {QuoteParameter(nameof(database))}")
            .EndCommandText()
            .SetParameter(nameof(database), database);
    }
}
