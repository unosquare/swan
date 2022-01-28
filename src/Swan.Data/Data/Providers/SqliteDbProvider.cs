namespace Swan.Data.Providers;

internal class SqliteDbProvider : DbProvider
{
    public override string ParameterPrefix { get; } = "$";

    public override IDbTypeMapper TypeMapper { get; } = new SqliteTypeMapper();

    public override Func<IDbColumnSchema> ColumnSchemaFactory { get; } = () => new SqliteColumn();

    public override DbCommand GetListTablesCommand(DbConnection connection)
    {
        return connection is null
            ? throw new ArgumentNullException(nameof(connection))
            : connection
            .BeginCommandText("SELECT name AS [Name], '' AS [Schema] FROM (SELECT * FROM sqlite_schema UNION ALL SELECT * FROM sqlite_temp_schema) WHERE type= 'table' ORDER BY name")
            .EndCommandText();
    }
}
