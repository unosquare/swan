namespace Swan.Samples;

using Swan.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

internal static class DataPlayground
{
    public static void BasicExample()
    {
        var conn = new SqlConnection("data source=.;initial catalog=unocorp-timecore;Integrated Security=true;");
        //var conn = new SqliteConnection("Data Source=hello.db");
        // var tableNames = await conn.TableNames();
        using var command = conn.BeginCommand()
            .SelectAllFields().From("Projects").Where()
            .FieldName("ProjectId").IsBetween().ParameterName("p1").And().ParameterName("p2")
            .OrderBy("ProjectId")
            .Limit(10, 20)
            .Materialize()
            .AddParameter("p1", 600)
            .AddParameter("p2", 1500);

        using var reader = command.ExecuteReader();

        var output = new List<dynamic>();
        while (reader.Read())
        {
            output.Add(reader.ReadObject());
        }

        //    var px = await conn.TableCommand("Projects").Take(10).ToListAsync();
    }

}

