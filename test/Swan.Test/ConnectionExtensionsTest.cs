namespace Swan.Test;

using Mocks;

[TestFixture]
public class ConnectionExtensionsTest
{
    [Test]
    public async Task CreateProjectsTableAndInsertOneRowAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.TableBuilder<Project>("Projects").ExecuteTableCommandAsync();

        var table = await conn.TableAsync<Project>("Projects");

        var project = await table.InsertOneAsync(new()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        });

        Assert.AreEqual(project?.Name, "Project ONE");
    }

    [Test]
    public void CreateProjectsTableAndInsertOneRow()
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

        Assert.AreEqual(project.Name, "Project ONE");
    }

    [Test]
    public void OpenConnectionAndGetItsProvidersCreateListTablesCommand()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var provider = conn.Provider();
        var command = provider.CreateListTablesCommand(conn).CommandText;

        Assert.AreEqual(command,
            "SELECT name AS [Name], '' AS [Schema] FROM (SELECT * FROM sqlite_schema UNION ALL SELECT * FROM sqlite_temp_schema) WHERE type= 'table' ORDER BY name");
    }

    [Test]
    public void CreateTablesAndGetTableNames()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        _ = conn.TableBuilder<Project>("ProjectsTableOne").ExecuteTableCommand();
        _ = conn.TableBuilder<Project>("ProjectsTableTwo").ExecuteTableCommand();
        _ = conn.TableBuilder<Project>("ProjectsTableThree").ExecuteTableCommand();

        var tableNames = conn.GetTableNames();

        Assert.IsTrue(tableNames.Any());
        Assert.IsTrue(tableNames.Count == 3);
    }

    [Test]
    public async Task CreateTablesAndGetTableNamesAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.TableBuilder<Project>("ProjectsTableOne").ExecuteTableCommandAsync();
        await conn.TableBuilder<Project>("ProjectsTableTwo").ExecuteTableCommandAsync();
        await conn.TableBuilder<Project>("ProjectsTableThree").ExecuteTableCommandAsync();

        var tableNames = conn.GetTableNamesAsync();

        Assert.IsTrue(tableNames.Result.Any());
        Assert.IsTrue(tableNames.Result.Count == 3);
    }

    [Test]
    public async Task CreateProjectsTableAndInsertsQueryAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.TableBuilder<Project>("Projects").ExecuteTableCommandAsync();

        var execution = await conn.ExecuteNonQueryAsync(
            "Insert into Projects (CompanyId, EndDate, IsActive, Name, ProjectScope, ProjectType, StartDate)" +
            " values " +
            $" (1,'{DateTime.Now}','{true}','Project ONE','My Scope', '{ProjectTypes.Exciting}','{DateTime.Now.AddMonths(-1)}');");

        Assert.IsTrue(execution == 1);
    }

    [Test]
    public void CreateProjectsTableAndInsertsQuery()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.TableBuilder<Project>("Projects").ExecuteTableCommand();

        var execution = conn.ExecuteNonQuery(
            "Insert into Projects (CompanyId, EndDate, IsActive, Name, ProjectScope, ProjectType, StartDate)" +
            " values " +
            $" (1,'{DateTime.Now}','{true}','Project ONE','My Scope', '{ProjectTypes.Exciting}','{DateTime.Now.AddMonths(-1)}');");

        Assert.IsTrue(execution == 1);
    }

    [Test]
    public async Task CreateProjectsTableAndInsertOneRowAndExecuteScalarAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.TableBuilder<Project>("Projects").ExecuteTableCommandAsync();

        var table = conn.TableAsync<Project>("Projects");
        var project = await table.Result.InsertOneAsync(new()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        });

        var scalar = await conn.ExecuteScalarAsync("select Name, ProjectScope, ProjectType from Projects");

        Assert.AreEqual(scalar, "Project ONE");
    }

    [Test]
    public void CreateProjectsTableAndInsertOneRowAndExecuteScalar()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.TableBuilder<Project>("Projects").ExecuteTableCommand();

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

        var scalar = conn.ExecuteScalar("select Name from Projects");

        Assert.AreEqual(scalar, "Project ONE");
    }

    [Test]
    public void CreateProjectsTableAndGetItsKeyColumnName()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.TableBuilder<Project>("Projects").ExecuteTableCommand();
        var keys = table.KeyColumns;

        Assert.AreEqual(keys.FirstOrDefault().ColumnName, "ProjectId");
    }

    [Test]
    public async Task CreateProjectsTableAndGetItsKeyColumnNameAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = await conn.TableBuilder<Project>("ProjectsAsync").ExecuteTableCommandAsync();
        var keys = table.KeyColumns;

        Assert.AreEqual(keys.FirstOrDefault().ColumnName, "ProjectId");
    }

    [Test]
    public void WhenConnectionIsNullThrowsException()
    {
        SqliteConnection? conn = null;

        Assert.Throws<ArgumentNullException>(() => conn.Provider());
        Assert.Throws<ArgumentNullException>(() => conn.TableBuilder<Project>("Projects").ExecuteTableCommand());
        Assert.Throws<ArgumentNullException>(() => conn.Table("Projects"));
        Assert.Throws<ArgumentNullException>(() => conn.Table<Project>("Projects"));
        Assert.Throws<ArgumentNullException>(() => conn.GetTableNames());
        Assert.ThrowsAsync<ArgumentNullException>(() => conn.GetTableNamesAsync());
        Assert.Throws<ArgumentNullException>(() => conn.EnsureConnected());
        Assert.Throws<ArgumentNullException>(() => conn.ExecuteNonQuery(""));
        Assert.ThrowsAsync<ArgumentNullException>(() => conn.ExecuteNonQueryAsync(""));
        Assert.Throws<ArgumentNullException>(() => conn.ExecuteScalar(""));
        Assert.ThrowsAsync<ArgumentNullException>(() => conn.ExecuteScalarAsync(""));
    }
}
