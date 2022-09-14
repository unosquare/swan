namespace Swan.Test;

using NUnit.Framework;
using Swan.Data.Context;
using Swan.Test.Mocks;

[TestFixture]
public class TableContextTest
{
    [Test]
    public void BuildCommandMethodsReturnInsertUpdateDeleteSelectQuerySyntax()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        var tran = conn.BeginTransaction();

        TableContext context = new TableContext(conn, table);

        var insertCommand = context.BuildInsertCommand(tran);
        var updateCommand = context.BuildUpdateCommand(tran);
        var deleteCommand = context.BuildDeleteCommand(tran);
        var selectCommand = context.BuildSelectCommand(tran);

        Assert.AreEqual("INSERT INTO [Projects] ( [Name], [ProjectType], [CompanyId], [IsActive], [StartDate], [EndDate], [ProjectScope] ) VALUES ( $Name, $ProjectType, $CompanyId, $IsActive, $StartDate, $EndDate, $ProjectScope )",
            insertCommand.CommandText);
        Assert.AreEqual("UPDATE [Projects] SET [Name] = $Name, [ProjectType] = $ProjectType, [CompanyId] = $CompanyId, [IsActive] = $IsActive, [StartDate] = $StartDate, [EndDate] = $EndDate, [ProjectScope] = $ProjectScope WHERE [ProjectId] = $ProjectId",
            updateCommand.CommandText);
        Assert.AreEqual("DELETE FROM [Projects] WHERE [ProjectId] = $ProjectId",
            deleteCommand.CommandText);
        Assert.AreEqual("SELECT [ProjectId], [Name], [ProjectType], [CompanyId], [IsActive], [StartDate], [EndDate], [ProjectScope] FROM [Projects] WHERE [ProjectId] = $ProjectId",
            selectCommand.CommandText);
    }
}
