namespace Swan.Test;

using NUnit.Framework;
using Swan.Test.Mocks;

[TestFixture]
public class TableContextTest
{
    [Test]
    public void BuildCommandMethodsReturnInsertUpdateDeleteSelectQuerySyntax()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var builder = conn.EnsureConnected().TableBuilder<Project>("Projects");
        builder.ExecuteTableCommand();

        var table = conn.Table<Project>("Projects");
        var tran = conn.BeginTransaction();

        var insertCommand = table.BuildInsertCommand(tran);
        var updateCommand = table.BuildUpdateCommand(tran);
        var deleteCommand = table.BuildDeleteCommand(tran);
        var selectCommand = table.BuildSelectCommand(tran);

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
