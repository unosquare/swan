namespace Swan.Test;

using Mocks;
using System.Data.Common;

[TestFixture]
public class QueryExtensionsAsyncTest
{
    [Test]
    public void CreateDbCommandToExecuteQueryAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

        table.InsertOne(new()
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

        var result = command.QueryAsync<Project>().ToListAsync();
        var result2 = command.QueryAsync().ToListAsync();

        Assert.AreEqual(result.Result[0].Name, "Project ONE");
        Assert.AreEqual(result2.Result[0].Name, "Project ONE");
    }

    [Test]
    public void CreateDbCommandToExecuteFirstOrDefaultAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
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
        var result = command.FirstOrDefaultAsync<Project>();
        var result2 = command.FirstOrDefaultAsync();

        Assert.AreEqual(result.Result.Name, "Project ONE");
        Assert.AreEqual(result2.Result.Name, "Project ONE");
    }

    [Test]
    public void CreateDbConnectionToExecuteQueryAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
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

        var result = conn.QueryAsync<Project>("Select * from Projects;").ToListAsync();
        var result2 = conn.QueryAsync("Select * from Projects;").ToListAsync();

        Assert.AreEqual(result.Result[0].Name, "Project ONE");
        Assert.AreEqual(result2.Result[0].Name, "Project ONE");
    }

    [Test]
    public void CreateDbConnectionToExecuteFirstOrDefaultAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

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

        var result = conn.FirstOrDefaultAsync<Project>("Select * from Projects;");
        var result2 = conn.FirstOrDefaultAsync("Select * from Projects;");

        Assert.AreEqual(result.Result.Name, "Project ONE");
        Assert.AreEqual(result2.Result.Name, "Project ONE");
    }

    [Test]
    public void ExecuteQueryAsyncWhenCommandIsNull()
    {
        DbCommand? command = null;

        Assert.ThrowsAsync<ArgumentNullException>(() => command.QueryAsync<Project>().ToListAsync());
        Assert.ThrowsAsync<ArgumentNullException>(() => command.QueryAsync().ToListAsync());
    }

    [Test]
    public void ExecuteQueryAsyncWhenConnectionCommandIsNull()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        DbCommand command = conn.CreateCommand();

        command.Connection = null;

        Assert.ThrowsAsync<ArgumentException>(() => command.QueryAsync<Project>().ToListAsync());
        Assert.ThrowsAsync<ArgumentException>(() => command.QueryAsync().ToListAsync());
    }

    [Test]
    public void ExecuteQueryAsyncWhenConnectionIsNull()
    {
        SqliteConnection? conn = null;

        Assert.ThrowsAsync<ArgumentNullException>(() =>
            conn.QueryAsync<Project>("Select * from Projects;").ToListAsync());
    }

    [Test]
    public void ExecuteQueryAsyncWhenSqlTextIsNullOrWhiteSpace()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        Assert.ThrowsAsync<ArgumentNullException>(() => conn.QueryAsync<Project>("").ToListAsync());
    }
}
