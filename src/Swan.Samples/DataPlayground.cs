namespace Swan.Samples;

using Microsoft.Data.Sqlite;
using Swan.Data.Extensions;
using System;
using System.Data;
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
        var output = conn.BeginCommand()
            .Select().Fields().From("Projects").Where()
            .Field("ProjectId").IsBetween().Parameter("p1").And().Parameter("p2")
            .OrderBy("ProjectId")
            .Limit(10, 20)
            .EndCommand()
            .SetParameter("p1", 600, DbType.String)
            .SetParameter("p2", 1500)
            .Query()
            .ToList();
        
        Console.WriteLine($"output contains {output.Count} records. Te first item is named '{output[0].Name}'");
        //    var px = await conn.TableCommand("Projects").Take(10).ToListAsync();

        var sx = conn
            .Query("SELECT * FROM Projects WHERE ProjectId BETWEEN @P1 AND @P2 ORDER BY ProjectId",
                new { P1 = 600, P2 = 1500 })
            .ToList();

        Console.WriteLine($"output contains {sx.Count} records. The tenth item is named '{sx[10].Name}'");

        var table = conn.Table("Projects");
        var r = table.SelectAll().Query().ToList();
        r = table.SelectByKey(new { ProjectId = 687 }).Query().ToList();

        var ins = table.Insert();
        Console.WriteLine($"output contains {r.Count} records. The tenth item is named '{r[0].Name}'");
    }

}

