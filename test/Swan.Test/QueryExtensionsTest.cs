namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Swan.Data.Extensions;
using static Swan.Test.Mocks.ProjectRecord;

[TestFixture]
public class QueryExtensionsTest
{
    [Test]
    public void CreateDbCommandToExecuteQueryT()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        var project = table.InsertOne(new()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        });

        System.Data.Common.DbCommand command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";
        var result = command.Query<Project>().ToList();

        Assert.AreEqual(result[0].Name, "Project ONE");
    }

    [Test]
    public void CreateDbCommandToExecuteQuery()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        var project = table.InsertOne(new()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        });


        System.Data.Common.DbCommand command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";
        var result = command.Query().ToList();

        Assert.AreEqual(result[0].Name, "Project ONE");
    }


    [Test]
    public void CreateDbCommandToExecuteFirstOrDefaultT()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        var project = table.InsertOne(new()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        });

        System.Data.Common.DbCommand command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";
        var result = command.FirstOrDefault<Project>();

        Assert.AreEqual(result.Name, "Project ONE");
    }

    [Test]
    public void CreateDbCommandToExecuteFirstOrDefault()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        var project = table.InsertOne(new()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        });

        System.Data.Common.DbCommand command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";
        var result = command.FirstOrDefault();

        Assert.AreEqual(result.Name, "Project ONE");
    }
}
