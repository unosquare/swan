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
        var output = conn.StartCommand()
            .Select().Fields().From("Projects").Where()
            .Field("ProjectId").IsBetween().Parameter("p1").And().Parameter("p2")
            .OrderBy("ProjectId")
            .Limit(10, 20)
            .FinishCommand()
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
        foreach (var col in table.Columns)
        {
            Console.WriteLine($"{col.Name,-16} {col.ProviderDataType,-16} {col.DataType,-16} {col.IsKey,-6} {col.IsAutoIncrement,-6}");
        }

        using var schemaCommand = conn.StartCommand().Select().Fields().From("Projects").Where().AppendText("1 = 2").FinishCommand();
        using var schemaReader = schemaCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
        foreach (var row in schemaReader.GetSchemaTable().Query())
        {
            Console.WriteLine(row);
        }
    }

}

