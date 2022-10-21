namespace Swan.Test;

using Mocks;

[TestFixture]
public class CodeGenExtensionTest
{
    [Test]
    public void CreateTableAndGetItsPocoCode()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        var pocoCode = table.GeneratePocoCode();

        Assert.IsTrue(pocoCode.Contains($"[Table(\"{table.TableName}\")]"));

        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[0].Name})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[1].Name})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[2].Name})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[3].Name})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[4].Name})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[5].Name})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[6].Name})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[7].Name})"));
    }

    [Test]
    public void CreateTableWithSchemaAndEntityNameAndGetItsPocoCode()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.TableBuilder<Project>("Projects", "Main").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects", "Main");
        var pocoCode = table.GeneratePocoCode("Project");

        Assert.IsTrue(pocoCode.Contains($"[Table(\"{table.TableName}\", Schema = \"Main\")]"));
    }

    [Test]
    public void TryGetPocoCodeWhenTableIsNull()
    {
        Data.Context.ITableContext? table = null;

        Assert.Throws<ArgumentNullException>(() => table.GeneratePocoCode());
    }

    [Test]
    public void TryGetPocoCodeWhenTableHasNoColumns()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.TableBuilder<ProjectNoColumns>("ProjectsNoColumns").ExecuteDdlCommand();

        var table = conn.Table("ProjectsNoColumns").ToTableBuilder();
        table.RemoveColumn("ProjectId");

        Assert.Throws<InvalidOperationException>(() => table.GeneratePocoCode());
    }
}
