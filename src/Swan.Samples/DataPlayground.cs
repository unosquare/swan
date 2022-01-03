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
        conn.TestSampleInsert();
    }

    private static void TestSampleCommandSource(this IDbConnection connection)
    {
        var output = connection.BeginCommand()
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
    }

    private static void TestSimpleQuery(this IDbConnection connection)
    {
        var sx = connection.Query("SELECT * FROM Projects WHERE ProjectId BETWEEN @P1 AND @P2 ORDER BY ProjectId",
            new { P1 = 600, P2 = 700 })
            .ToList();

        Console.WriteLine($"output contains {sx.Count} records. The last item is named '{sx.Last().Name}'");
    }

    private static void TestSampleInsert(this IDbConnection connection)
    {
        connection.Provider().WithDefaultCommandTimeout(TimeSpan.FromSeconds(10));
        var tableContext = connection.Table("Projects");
        using var tran = connection.BeginTransaction();

        var insertCommand = tableContext.Insert(new
        {
            Name = "Dummy Project",
            ProjectType = 1,
            CompanyId = 61,
            IsActive = true,
            StartDate = DateTime.UtcNow,
            EndDate = default(DateTime?),
            ProjectScope = "This is some dummy insert"
        }).WithTransaction(tran);
        insertCommand.CommandText = $"{insertCommand.CommandText}; SELECT SCOPE_IDENTITY();";
        if (insertCommand.TryPrepare())
        {
            Console.WriteLine($"Command Prepared:\r\n  {insertCommand.CommandText}");
            var result = insertCommand.Query().ToList();
            Console.WriteLine($"");
        }
        else
        {
            Console.WriteLine("Unable to prepare command :(");
        }
        tran.Rollback();
    }

    private static void DumpToConsole(this IDbDataParameter p0)
    {
        Console.WriteLine($"{p0.ParameterName} {p0.DbType} PREC: {p0.Precision} SC: {p0.Scale} SZ: {p0.Size} {p0.IsNullable} {p0.Direction} {p0.Value}");
    }
}

