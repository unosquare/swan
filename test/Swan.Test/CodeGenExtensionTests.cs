namespace Swan.Test;

using Mocks;

[TestFixture]
public class CodeGenExtensionTest
{
    [Test]
    public void CreateTableAndGetItsPocoCode()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.TableBuilder<Project>("Projects").ExecuteTableCommand();
        var pocoCode = table.GeneratePocoCode();

        Assert.IsTrue(pocoCode.Contains($"[Table(\"{table.TableName}\")]"));

        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[0].ColumnName})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[1].ColumnName})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[2].ColumnName})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[3].ColumnName})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[4].ColumnName})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[5].ColumnName})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[6].ColumnName})"));
        Assert.IsTrue(pocoCode.Contains($"Column(nameof({table.Columns[7].ColumnName})"));
    }

    [Test]
    public void CreateTableWithSchemaAndEntityNameAndGetItsPocoCode()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.TableBuilder<Project>("Projects", "Main").ExecuteTableCommand();
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
        var table = conn.TableBuilder<ProjectNoColumns>("ProjectsNoColumns");
        table.RemoveColumn("ProjectId");

        Assert.Throws<InvalidOperationException>(() => table.GeneratePocoCode());
    }
}
