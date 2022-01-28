namespace Swan.Data.Providers;

using Swan.Data.Schema;
using System;
using System.Data.Common;

internal class SqlServerDbProvider : DbProvider
{
    public override string DefaultSchemaName { get; } = "dbo";

    public override Func<IDbColumnSchema> ColumnSchemaFactory { get; } = () => new SqlServerColumn();

    public override DbCommand GetListTablesCommand(DbConnection connection)
    {
        return connection is null
            ? throw new ArgumentNullException(nameof(connection))
            : connection
            .BeginCommandText("SELECT [TABLE_NAME] AS [Name], [TABLE_SCHEMA] AS [Schema] FROM [INFORMATION_SCHEMA].[TABLES]")
            .EndCommandText();
    }

}

