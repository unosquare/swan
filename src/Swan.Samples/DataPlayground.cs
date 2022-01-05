namespace Swan.Samples;

using Microsoft.Data.Sqlite;
using Swan.Data.Extensions;
using Swan.Logging;
using Swan.Platform;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

internal static class DataPlayground
{
    public static void BasicExample()
    {
        var liteName = typeof(SqliteConnection).FullName;

        // Create a connection as usual.
        using var conn = new SqlConnection("data source=.;initial catalog=unocorp-timecore;Integrated Security=true;");

        // You can configure the default timeout for commands created using the SWAN API.
        conn.Provider().WithDefaultCommandTimeout(TimeSpan.FromSeconds(10));

        //var conn = new SqliteConnection("Data Source=hello.db");
        // var tableNames = await conn.TableNames();
        conn.TestSampleInsert();
    }

    private static void TestSampleCommandSource(this IDbConnection connection)
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

    private static void TestSimpleQuery(this IDbConnection connection)
    {
        var sx = connection.Query("SELECT * FROM Projects WHERE ProjectId BETWEEN @P1 AND @P2 ORDER BY ProjectId",
            new { P1 = 600, P2 = 700 })
            .ToList();

        $"output contains {sx.Count} records. The last item is named '{sx.Last().Name}'".Info();
    }

    private static void TestSampleInsert(this IDbConnection connection)
    {
        // Acquire a table context. The table schema is only retrieved once
        // the rest of the times it is cached. This is useful because
        // it lets us build CRUD commands automatically.
        // Please don't have pending transactions on the connection
        // while retrieveing the schema as some providers don't support it.
        var projects = connection.Table("Projects");

        // We'll use a transaction in this example. We won't actually insert anything.
        // since we will be rolling back the transaction.
        using var tran = connection.BeginTransaction();

        // Create a dummy record
        var dummyProject = new Project(
            default, "Dummy Project", ProjectTypes.Exciting, 61, false, DateTime.UtcNow, default, "Dummy Scope");

        // Define the insert command. We show 2 things here:
        // a. Specifying the transaction.
        // b. Adding text to the predefined command text. In this case we return the generated ID.
        var insertCommand = projects.Insert().WithTransaction(tran)
            .AppendText("; SELECT SCOPE_IDENTITY();");

        // Since we are in the table context we can prepare the command
        // because parameters have been defined automatically, specifying types and sizes.
        if (insertCommand.TryPrepare(out var ex))
        {
            // Let's output the command text we are about to execute.
            $"Command Prepared:\r\n  {insertCommand.CommandText}".Info();

            // We'll create a boring project and an exciting one and dump them to the console.
            var types = new ProjectTypes[] { ProjectTypes.Exciting, ProjectTypes.Boring };
            foreach (var type in types)
            {
                // pass the parameters to the prepared command.
                // we use the ortiginal record, copy it, and change the project type accordingly.
                insertCommand.SetParameters(dummyProject with { ProjectType = type });

                // retrieve the automatically generated project id
                var projectId = Convert.ToInt32(insertCommand.ExecuteScalar()!);

                // SWAN.Data is smart enough to determine that the insert id is just a number and since
                // there's only 1 key, we only need to pass a basic value (string, number, guid, etc)
                // to the select by key method.
                var addedProject = projects.SelectByKey(projectId).WithTransaction(tran).Query<Project>().Single();

                // Show the inserted object pulled straight out of the db
                "Project Added:".Info();
                $"{addedProject}".Info();
            }
        }
        else
        {
            $"Unable to prepare command:\r\n  {ex.Message}".Warn();
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

