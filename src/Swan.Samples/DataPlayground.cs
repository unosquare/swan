namespace Swan.Samples;

using Microsoft.Data.Sqlite;
using Swan.Data;
using System;
using System.Collections.Generic;
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

        var schemaQuery = conn.Query("SELECT * FROM [INFORMATION_SCHEMA].[COLUMNS]");
        var columns = new List<DbColumn>(2048);
        foreach (var item in schemaQuery)
        {
            if (item is null)
                continue;

            var c = new DbColumn()
            {
                AllowsDBNull = item.IS_NULLABLE == "YES" ? true : false,
                ColumnName = item.COLUMN_NAME,
                ColumnOrdinal = item.ORDINAL_POSITION,
            };

            columns.Add(c);
        }

        /* -- T-SQL Schema stuff
SELECT * FROM [INFORMATION_SCHEMA].[COLUMNS]

SELECT K.TABLE_SCHEMA, K.TABLE_NAME, K.COLUMN_NAME, C.CONSTRAINT_TYPE, K.CONSTRAINT_NAME,
CASE WHEN COLUMNPROPERTY(object_id(K.TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1 THEN 1 ELSE 0 END AS IS_IDENTITY
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K
ON C.TABLE_NAME = K.TABLE_NAME
AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG
AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA
AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME
AND C.CONSTRAINT_NAME <> 'FOREIGN KEY'
ORDER BY K.TABLE_NAME, C.CONSTRAINT_TYPE, K.CONSTRAINT_NAME
         */
    }

}

