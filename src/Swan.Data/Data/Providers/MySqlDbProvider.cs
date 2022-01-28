namespace Swan.Data.Providers;

internal class MySqlDbProvider : DbProvider
{
    public override IDbTypeMapper TypeMapper { get; } = new MySqlTypeMapper();
}
