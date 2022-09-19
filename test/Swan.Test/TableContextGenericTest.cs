namespace Swan.Test;

using NUnit.Framework;
using Swan.Test.Mocks;

[TestFixture]
public class TableContextGenericTest
{
    private static Project project = new()
    {
        CompanyId = 1,
        EndDate = DateTime.Now,
        IsActive = true,
        Name = "Project ONE",
        ProjectScope = "My Scope",
        ProjectType = ProjectTypes.Exciting,
        StartDate = DateTime.Now.AddMonths(-1)
    };

    private static Project projectUpdated = new()
    {
        ProjectId = 1,
        CompanyId = 1,
        EndDate = DateTime.Now,
        IsActive = true,
        Name = "Project Updated",
        ProjectScope = "My Scope",
        ProjectType = ProjectTypes.Exciting,
        StartDate = DateTime.Now.AddMonths(-1)
    };

    private static List<Project> projects = new()
    {
        new Project()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        },
        new Project()
        {
            CompanyId = 2,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project TWO",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Boring,
            StartDate = DateTime.Now.AddMonths(-1)
        }
    };

    private static List<Project> projectsUpdated = new()
    {
        new Project()
        {
            ProjectId = 1,
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE Updated",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        },
        new Project()
        {
            ProjectId = 2,
            CompanyId = 2,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project TWO Updated",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Boring,
            StartDate = DateTime.Now.AddMonths(-1)
        }
    };

    private static Project projectToDelete = new()
    {
        ProjectId = 1
    };

    private static List<Project> projectsToDelete = new()
    {
        new Project()
        {
            ProjectId = 1
        },
        new Project()
        {
            ProjectId = 2
        }
    };

    [Test]
    public void FirstOrDefaultFromTableReturnsOneRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        table.InsertOne(project);

        var result = table.FirstOrDefault();

        Assert.AreEqual(project.Name, result?.Name);
    }

    [Test]
    public async Task FirstOrDefaultAsyncFromTableReturnsOneRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = await conn.TableAsync<Project>("Projects");

        await table.InsertOneAsync(project);

        var result = await table.FirstOrDefaultAsync();

        Assert.AreEqual(project.Name, result?.Name);
    }

    [Test]
    public void InsertOneAndInsertOneAsyncWhenItemIsNullThrowsException()
    {
        List<Project>? nullProjectList = null;
        Project? nullProject = null;

        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        Assert.Throws<ArgumentNullException>(() => table.InsertOne(nullProject));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertOneAsync(nullProject));

        Assert.Throws<ArgumentNullException>(() => table.InsertMany(nullProjectList));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertManyAsync(nullProjectList));

        Assert.Throws<ArgumentNullException>(() => table.UpdateOne(nullProject));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateOneAsync(nullProject));

        Assert.Throws<ArgumentNullException>(() => table.UpdateMany(nullProjectList));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateManyAsync(nullProjectList));

        Assert.Throws<ArgumentNullException>(() => table.DeleteOne(nullProject));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteOneAsync(nullProject));

        Assert.Throws<ArgumentNullException>(() => table.DeleteMany(nullProjectList));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteManyAsync(nullProjectList));
    }

    [Test]
    public void InsertOneRowReturnsSameRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        var result = table.InsertOne(project);

        Assert.AreEqual(project.Name, result?.Name);
    }

    [Test]
    public async Task InsertOneAsyncRowReturnsSameRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = await conn.TableAsync<Project>("Projects");

        var result = await table.InsertOneAsync(project);

        Assert.AreEqual(project.Name, result?.Name);
    }

    [Test]
    public void InsertManyReturnsCountOfRowsInserted()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        var projectsInserted = table.InsertMany(projects);

        Assert.AreEqual(projects.Count, projectsInserted);
    }

    [Test]
    public async Task InsertManyAsyncReturnsCountOfRowsInserted()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        var projectsInserted = await table.InsertManyAsync(projects);

        Assert.AreEqual(projects.Count, projectsInserted);
    }

    [Test]
    public void UpdateOneRetrunsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        table.InsertOne(project);

        var projecstUpdated = table.UpdateOne(projectUpdated);

        Assert.AreEqual(1, projecstUpdated);
    }

    [Test]
    public async Task UpdateOneAsyncRetrunsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        table.InsertOne(project);

        var projecstUpdated = await table.UpdateOneAsync(projectUpdated);

        Assert.AreEqual(1, projecstUpdated);
    }

    [Test]
    public void UpdateManyRetrunsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        table.InsertMany(projects);
        var updatedCount = table.UpdateMany(projectsUpdated);
        var updatedTable = table.Query().ToList();

        Assert.AreEqual(projectsUpdated.Count, updatedCount);
        Assert.AreEqual(projectsUpdated[0].Name, updatedTable[0].Name);
        Assert.AreEqual(projectsUpdated[1].Name, updatedTable[1].Name);
    }

    [Test]
    public async Task UpdateManyAsyncRetrunsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        table.InsertMany(projects);
        var updatedCount = await table.UpdateManyAsync(projectsUpdated);
        var updatedTable = await table.QueryAsync().ToListAsync();

        Assert.AreEqual(projectsUpdated.Count, updatedCount);
        Assert.AreEqual(projectsUpdated[0].Name, updatedTable[0]?.Name);
        Assert.AreEqual(projectsUpdated[1].Name, updatedTable[1]?.Name);
    }

    [Test]
    public void DeleteOneChecksIfIsDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        table.InsertOne(project);

        table.DeleteOne(projectToDelete);
        var count = table.Query().ToList().Count;

        Assert.AreEqual(0, count);
    }

    [Test]
    public async Task DeleteOneAsyncChecksIfIsDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        table.InsertOne(project);

        await table.DeleteOneAsync(projectToDelete);
        var count = table.Query().ToList().Count;

        Assert.AreEqual(0, count);
    }

    [Test]
    public void DeleteManyChecksIfAreDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        table.InsertMany(projects);
        var deleted = table.DeleteMany(projectsToDelete);
        var remaining = table.Query().ToList().Count;

        Assert.AreEqual(projects.Count - deleted, remaining);
    }

    [Test]
    public async Task DeleteManyAsyncChecksIfAreDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        await table.InsertManyAsync(projects);
        var deleted = await table.DeleteManyAsync(projectsToDelete);
        var remaining = table.Query().ToList().Count;

        Assert.AreEqual(projects.Count - deleted, remaining);
    }
}
