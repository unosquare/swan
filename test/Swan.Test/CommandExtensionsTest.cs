namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;

[TestFixture]
public class CommandExtensionsTest
{
    [Test]
    public void Test1()
    {
        var conn = new SqliteConnection("Data Source=<:Memory:");
        var command = conn.CreateCommand();
        
    }
}
