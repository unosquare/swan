namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Swan.Data.Extensions;
using static Swan.Test.Mocks.ProjectRecord;

[TestFixture]
public class CodeGenExtensionTest
{
    [Test]
    public void GenPoco()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        string pocoCode = table.GeneratePocoCode();

        Assert.IsTrue(pocoCode.Contains($"[Table(\"{table.TableName}\")]"));
    }
}
