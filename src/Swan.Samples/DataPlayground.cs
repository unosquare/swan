namespace Swan.Samples;

using Microsoft.Data.Sqlite;
using Swan.Data;
using System;
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
            .Select().Fields().From("Projects").Where()
            .Field("ProjectId").IsBetween().Parameter("p1").And().Parameter("p2")
            .OrderBy("ProjectId")
            .Limit(10, 20)
            .FinishCommand()
            .SetParameter("p1", 600, System.Data.DbType.String)
            .SetParameter("p2", 1500)
            .Query()
            .ToList();
        
        Console.WriteLine($"output contains {output.Count} records. Te first item is named '{output[0].Name}'");
        //    var px = await conn.TableCommand("Projects").Take(10).ToListAsync();

        var sx = conn
            .Query("SELECT * FROM Projects WHERE ProjectId BETWEEN @P1 AND @P2 ORDER BY ProjectId",
                new { P1 = 600, P2 = 1500 })
            .ToList();

        Console.WriteLine($"output contains {sx.Count} records. Te tenth item is named '{sx[10].Name}'");

        var table = new DbTable(conn, "Projects", null);
        foreach (var col in table.Columns)
        {
            Console.WriteLine(col);
        }
    }

}

