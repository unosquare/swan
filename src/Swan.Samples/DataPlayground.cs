namespace Swan.Samples;

using Microsoft.Data.Sqlite;
using Swan.Data.Extensions;
using Swan.Logging;
using Swan.Platform;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal static class DataPlayground
{
    private const string ConnectionString = "data source=.;initial catalog=unocorp-timecore;Integrated Security=true;";

    public static async Task BasicExample()
    {
        var liteName = typeof(SqliteConnection).FullName;

        // Create a connection as usual.
        using var connection = new SqlConnection(ConnectionString);

        // You can configure the default timeout for commands created using the SWAN API.
        connection.Provider().WithDefaultCommandTimeout(TimeSpan.FromSeconds(10));
        
        //var conn = new SqliteConnection("Data Source=hello.db");
        // var tableNames = await conn.TableNames();
        await connection.TestSampleInsertButBetter();
    }

    public static async Task AsyncQuerying()
    {
        const string commandText = ""; // "WHERE ProjectId BETWEEN @P1 AND @P2 ORDER BY ProjectId";
        using var connection = new SqlConnection(ConnectionString);
        var cts = new CancellationTokenSource();
        var items = connection.Table<Project>("Projects").QueryAsync(
            commandText, new { P1 = 600, P2 = 700 }, default, cts.Token);

        var count = 0;
        try
        {
            await foreach (var item in items)
            {
                $"{item}".Info();
                count++;

                if (count == 4)
                {
                    Terminal.Write("Press 'c' to cancel. Any other key to continue: ");
                    var key = Terminal.ReadKey(true);
                    Terminal.WriteLine();
                    if (key.Key == ConsoleKey.C)
                        cts.Cancel();
                }

            }
        }
        catch (TaskCanceledException)
        {
            $"Task was cancelled".Warn();
        }

        $"Records retrieved: {count}".Info();
        
        // Terminal.ReadKey(true);
        Terminal.Flush();
    }

    private static void TestSampleCommandSource(this DbConnection connection)
    {
        var output = connection.BeginCommandText()
            .Select().Fields().From("Projects").Where()
            .Field("ProjectId").IsBetween().Parameter("p1").And().Parameter("p2")
            .OrderBy("ProjectId")
            .Limit(10, 20)
            .EndCommandText()
            .SetParameter("p1", 600, DbType.String)
            .SetParameter("p2", 1500)
            .Query()
            .ToList();

        $"output contains {output.Count} records. Te first item is named '{output[0].Name}'".Info();
    }

    private static void TestSimpleQuery(this DbConnection connection)
    {
        var sx = connection.Query("SELECT * FROM Projects WHERE ProjectId BETWEEN @P1 AND @P2 ORDER BY ProjectId",
            new { P1 = 600, P2 = 700 })
            .ToList();

        $"output contains {sx.Count} records. The last item is named '{sx.Last().Name}'".Info();
    }

    private static async Task TestSampleInsertButBetter(this DbConnection connection)
    {
        // Now, instead of doing all that stuff manually, if we play with
        // typical game rules, we can do stuff in a much simpler way :)
        var projects = connection.Table<Project>("Projects");

        // We'll use a transaction in this example. We won't actually insert anything.
        // since we will be rolling back the transaction.
        using var tran = await connection.BeginTransactionAsync();

        // Create a dummy record
        var dummyProject = new Project(
            default, "Dummy Project", ProjectTypes.Exciting, 61, false, DateTime.UtcNow, default, "Dummy Scope");

        var items = new Project[]
        {
            dummyProject with { Name = "Dummy 1", ProjectType = ProjectTypes.Boring },
            dummyProject with { Name = "Dummy 2", ProjectType = ProjectTypes.Exciting },
            dummyProject with { Name = "Dummy 3", ProjectType = ProjectTypes.Exciting },
        };

        foreach (var item in items)
        {
            var inserted = await projects.InsertOneAsync(item, tran);
            $"ADDED:\r\n{inserted}".Info();
        }

        // We won't actually insert anything. We'll rollback the transaction.
        tran.Rollback();
        Terminal.Flush();
    }

    private static void TestSampleDdl(this IDbConnection connection)
    {

    }

    private record Project(
        int ProjectId,
        string? Name,
        ProjectTypes ProjectType,
        int? CompanyId,
        bool IsActive,
        DateTime? StartDate,
        DateTime? EndDate,
        string? ProjectScope
    )
    {
        /// <summary>
        /// Parameter-less constructor
        /// </summary>
        public Project()
            : this(default, default, default, default, default, default, default, default) { }
    }

    private enum ProjectTypes
    {
        Boring,
        Exciting
    }
}

