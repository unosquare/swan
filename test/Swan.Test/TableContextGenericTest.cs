namespace Swan.Test;

using NUnit.Framework;
using Mocks;

[TestFixture]
public class TableContextGenericTest
{
    private static readonly Project Project = new()
    {
        CompanyId = 1,
        EndDate = DateTime.Now,
        IsActive = true,
        Name = "Project ONE",
        ProjectScope = "My Scope",
        ProjectType = ProjectTypes.Exciting,
        StartDate = DateTime.Now.AddMonths(-1)
    };

    private static readonly Project ProjectUpdated = new()
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

    private static readonly List<Project> Projects = new()
    {
        new()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        },
        new()
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

    private static readonly List<Project> ProjectsUpdated = new()
    {
        new()
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
        new()
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

    private static readonly Project ProjectToDelete = new()
    {
        ProjectId = 1
    };

    private static readonly List<Project> ProjectsToDelete = new()
    {
        new()
        {
            ProjectId = 1
        },
        new()
        {
            ProjectId = 2
        }
    };

    [Test]
    public void FirstOrDefaultFromTableReturnsOneRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        table.InsertOne(Project);

        var result = table.FirstOrDefault();

        Assert.AreEqual(Project.Name, result?.Name);
    }

    [Test]
    public async Task FirstOrDefaultAsyncFromTableReturnsOneRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        await table.InsertOneAsync(Project);

        var result = await table.FirstOrDefaultAsync();

        Assert.AreEqual(Project.Name, result?.Name);
    }

    [Test]
    public void InsertOneAndInsertOneAsyncWhenItemIsNullThrowsException()
    {
        List<Project>? nullProjectList = null;
        Project? nullProject = null;

        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

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
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        var result = table.InsertOne(Project);

        Assert.AreEqual(Project.Name, result?.Name);
    }

    [Test]
    public async Task InsertOneAsyncRowReturnsSameRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        var result = await table.InsertOneAsync(Project);

        Assert.AreEqual(Project.Name, result?.Name);
    }

    [Test]
    public void InsertManyReturnsCountOfRowsInserted()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        var projectsInserted = table.InsertMany(Projects);

        Assert.AreEqual(Projects.Count, projectsInserted);
    }

    [Test]
    public async Task InsertManyAsyncReturnsCountOfRowsInserted()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        var projectsInserted = await table.InsertManyAsync(Projects);

        Assert.AreEqual(Projects.Count, projectsInserted);
    }

    [Test]
    public void UpdateOneReturnsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        table.InsertOne(Project);

        var projecstUpdated = table.UpdateOne(ProjectUpdated);

        Assert.AreEqual(1, projecstUpdated);
    }

    [Test]
    public async Task UpdateOneAsyncReturnsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        table.InsertOne(Project);

        var projecstUpdated = await table.UpdateOneAsync(ProjectUpdated);

        Assert.AreEqual(1, projecstUpdated);
    }

    [Test]
    public void UpdateManyReturnsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

        table.InsertMany(Projects);
        var updatedCount = table.UpdateMany(ProjectsUpdated);
        var updatedTable = table.Query().ToList();

        Assert.AreEqual(ProjectsUpdated.Count, updatedCount);
        Assert.AreEqual(ProjectsUpdated[0].Name, updatedTable[0].Name);
        Assert.AreEqual(ProjectsUpdated[1].Name, updatedTable[1].Name);
    }

    [Test]
    public async Task UpdateManyAsyncReturnsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

        table.InsertMany(Projects);
        var updatedCount = await table.UpdateManyAsync(ProjectsUpdated);
        var updatedTable = await table.QueryAsync().ToListAsync();

        Assert.AreEqual(ProjectsUpdated.Count, updatedCount);
        Assert.AreEqual(ProjectsUpdated[0].Name, updatedTable[0]?.Name);
        Assert.AreEqual(ProjectsUpdated[1].Name, updatedTable[1]?.Name);
    }

    [Test]
    public void DeleteOneChecksIfIsDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

        table.InsertOne(Project);

        table.DeleteOne(ProjectToDelete);
        var count = table.Query().ToList().Count;

        Assert.AreEqual(0, count);
    }

    [Test]
    public async Task DeleteOneAsyncChecksIfIsDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        table.InsertOne(Project);

        await table.DeleteOneAsync(ProjectToDelete);
        var count = table.Query().ToList().Count;

        Assert.AreEqual(0, count);
    }

    [Test]
    public void DeleteManyChecksIfAreDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

        table.InsertMany(Projects);
        var deleted = table.DeleteMany(ProjectsToDelete);
        var remaining = table.Query().ToList().Count;

        Assert.AreEqual(Projects.Count - deleted, remaining);
    }

    [Test]
    public async Task DeleteManyAsyncChecksIfAreDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        await table.InsertManyAsync(Projects);
        var deleted = await table.DeleteManyAsync(ProjectsToDelete);
        var remaining = table.Query().ToList().Count;

        Assert.AreEqual(Projects.Count - deleted, remaining);
    }
}
