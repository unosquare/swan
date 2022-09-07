namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Swan.Data.Extensions;
using System.Data;
using System.Data.Common;
using static Swan.Test.Mocks.ProjectRecord;

[TestFixture]
public class QueryExtensionsTest
{
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

        DbCommand command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";

        var result = command.Query<Project>().ToList();
        var result2 = command.Query().ToList();

        Assert.AreEqual(result[0].Name, "Project ONE");
        Assert.AreEqual(result2[0].Name, "Project ONE");
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

        DbCommand command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";
        var result = command.FirstOrDefault<Project>();
        var result2 = command.FirstOrDefault();

        Assert.AreEqual(result.Name, "Project ONE");
        Assert.AreEqual(result2.Name, "Project ONE");
    }

    [Test]
    public void CreateDbConnectionToExecuteQuery()
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

        var result = conn.Query<Project>("Select * from Projects;").ToList();
        var result2 = conn.Query("Select * from Projects;").ToList();


        Assert.AreEqual(result[0].Name, "Project ONE");
        Assert.AreEqual(result2[0].Name, "Project ONE");
    }

    [Test]
    public void CreateDbConnectionToExecuteFirstOrDefault()
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

        var result = conn.FirstOrDefault<Project>("Select * from Projects;");
        var result2 = conn.FirstOrDefault("Select * from Projects;");

        Assert.AreEqual(result?.Name, "Project ONE");
        Assert.AreEqual(result2?.Name, "Project ONE");
    }

    [Test]
    public void CreateDataTableToExecuteQuery()
    {
        DataTable table = new DataTable();

        table.Columns.Add("CompanyId");
        table.Columns.Add("EndDate");
        table.Columns.Add("IsActive");
        table.Columns.Add("Name");
        table.Columns.Add("ProjectScope");
        table.Columns.Add("ProjectType");
        table.Columns.Add("StartDate");

        DataRow row = table.NewRow();
        row["CompanyId"] = 1;
        row["EndDate"] = DateTime.Now;
        row["IsActive"] = true;
        row["Name"] = "Project ONE";
        row["ProjectScope"] = "My Scope";
        row["ProjectType"] = ProjectTypes.Exciting;
        row["StartDate"] = DateTime.Now.AddMonths(-1);
        table.Rows.Add(row);

        var result = table.Query().ToList();
        var result2 = table.Query<Project>().ToList();

        Assert.AreEqual(result[0].Name, "Project ONE");
        Assert.AreEqual(result2[0].Name, "Project ONE");
    }

    [Test]
    public void ExecuteQueryWhenCommandIsNull()
    {
        DbCommand command = null;

        Assert.Throws<ArgumentNullException>(() => command.Query<Project>().ToList());
        Assert.Throws<ArgumentNullException>(() => command.Query().ToList());
    }

    [Test]
    public void ExecuteQueryWhenCommandConnectionIsNull()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        DbCommand command = conn.CreateCommand();

        command.Connection = null;

        Assert.Throws<ArgumentException>(() => command.Query<Project>().ToList());
        Assert.Throws<ArgumentException>(() => command.Query().ToList());
    }

    [Test]
    public void ExecuteQueryWhenConnectionIsNull()
    {
        SqliteConnection conn = null;

        Assert.Throws<ArgumentNullException>(() => conn.Query<Project>("Select * from Projects;"));
    }

    [Test]
    public void ExecuteQueryWhenSqlTextIsNullOrWhiteSpace()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        Assert.Throws<ArgumentNullException>(() => conn.Query<Project>(""));
    }

    [Test]
    public void ExecuteQueryWhenTableIsNull()
    {
        DataTable table = null;

        Assert.Throws<ArgumentNullException>(() => table.Query<Project>().ToList());
        Assert.Throws<ArgumentNullException>(() => table.Query().ToList());
    }
}
