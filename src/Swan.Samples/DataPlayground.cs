namespace Swan.Samples;

using Microsoft.Data.Sqlite;
using Swan.Data;
using System.Data.SqlClient;
using System.Linq;

internal static class DataPlayground
{
    public static void BasicExample()
    {
        var liteName = typeof(SqliteConnection).FullName;

        using var conn = new SqlConnection("data source=.;initial catalog=unocorp-timecore;Integrated Security=true;");
        //var conn = new SqliteConnection("Data Source=hello.db");
        // var tableNames = await conn.TableNames();
        var output = conn.StartCommand()
            .Select().FieldNames().From("Projects").Where()
            .FieldName("ProjectId").IsBetween().ParameterName("p1").And().ParameterName("p2")
            .OrderBy("ProjectId")
            .Limit(10, 20)
            .FinishCommand()
            .AddParameter("p1", 600)
            .AddParameter("p2", 1500)
            .Query()
            .ToList();

        //    var px = await conn.TableCommand("Projects").Take(10).ToListAsync();
    }

}

