namespace Swan.Data.Providers;

internal class SqliteDbProvider : DbProvider
{
    public override string ParameterPrefix { get; } = "$";

    public override IDbTypeMapper TypeMapper { get; } = new SqliteTypeMapper();
}
