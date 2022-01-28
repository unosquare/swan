namespace Swan.Data.Providers;

internal class SqlServerDbProvider : DbProvider
{
    public override string DefaultSchemaName { get; } = "dbo";
}

