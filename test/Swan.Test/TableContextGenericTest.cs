namespace Swan.Test;

using NUnit.Framework;
using Swan.Test.Mocks;

[TestFixture]
public class TableContextGenericTest
{
    [Test]
    public void FirstOrDefaultFromTableReturnsOneRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

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

        var result = table.FirstOrDefault();

        Assert.AreEqual("Project ONE",result?.Name);
    }

    [Test]
    public async Task FirstOrDefaultAsyncFromTableReturnsOneRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

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

        var result = await table.FirstOrDefaultAsync();

        Assert.AreEqual("Project ONE",result?.Name);
    }

    [Test]
    public void InsertOneAndInsertOneAsyncWhenItemIsNullThrowsException()
    {
        List<Project> projectList = null;
        Project project = null;

        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        Assert.Throws<ArgumentNullException>(() => table.InsertOne(project));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertOneAsync(project));

        Assert.Throws<ArgumentNullException>(() => table.InsertMany(projectList));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertManyAsync(projectList));

        Assert.Throws<ArgumentNullException>(() => table.UpdateOne(project));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateOneAsync(project));

        Assert.Throws<ArgumentNullException>(() => table.UpdateMany(projectList));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateManyAsync(projectList));

        Assert.Throws<ArgumentNullException>(() => table.DeleteOne(project));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteOneAsync(project));

        Assert.Throws<ArgumentNullException>(() => table.DeleteMany(projectList));
        Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteManyAsync(projectList));
    }

    [Test]
    public void InsertOneRowReturnsSameRow()
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

        Assert.AreEqual("Project ONE", project?.Name);
    }

    [Test]
    public async Task InsertOneAsyncRowReturnsSameRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

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

        Assert.AreEqual("Project ONE", project?.Name);
    }

    [Test]
    public void InsertManyReturnsCountOfRowsInserted()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        List<Project> projects = new List<Project>
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

        var projectsInserted = table.InsertMany(projects);

        Assert.AreEqual(2, projectsInserted);
    }

    [Test]
    public async Task InsertManyAsyncReturnsCountOfRowsInserted()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        List<Project> projects = new List<Project>
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

        var projectsInserted = await table.InsertManyAsync(projects);

        Assert.AreEqual(projects.Count, projectsInserted);
    }

    [Test]
    public void UpdateOneRetrunsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

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

        Project project = new Project()
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

        var projecstUpdated = table.UpdateOne(project);

        Assert.AreEqual(1, projecstUpdated);
    }

    [Test]
    public async Task UpdateOneAsyncRetrunsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

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

        Project project = new Project()
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

        var projectUpdated = await table.UpdateOneAsync(project);

        Assert.AreEqual(1, projectUpdated);
    }

    [Test]
    public void UpdateManyRetrunsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        List<Project> projects = new List<Project>
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

        List<Project> projectsUpdated = new List<Project>
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

        table.InsertMany(projects);
        var updatedCount = table.UpdateMany(projectsUpdated);
        var updatedTable = table.Query().ToList();

        Assert.AreEqual(projectsUpdated.Count, updatedCount);
        Assert.AreEqual("Project ONE Updated", updatedTable[0].Name);
        Assert.AreEqual("Project TWO Updated", updatedTable[1].Name);
    }

    [Test]
    public async Task UpdateManyAsyncRetrunsNumberOfRowsAffected()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        List<Project> projects = new List<Project>
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

        List<Project> projectsUpdated = new List<Project>
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

        table.InsertMany(projects);
        var updatedCount = await table.UpdateManyAsync(projectsUpdated);
        var updatedTable = await table.QueryAsync().ToListAsync();

        Assert.AreEqual(projectsUpdated.Count, updatedCount);
        Assert.AreEqual("Project ONE Updated", updatedTable[0]?.Name);
        Assert.AreEqual("Project TWO Updated", updatedTable[1]?.Name);
    }

    [Test]
    public void DeleteOneChecksIfIsDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

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

        Project project = new Project()
        {
            ProjectId = 1
        };

        table.DeleteOne(project);
        var count = table.Query().ToList().Count;

        Assert.AreEqual(0, count);
    }

    [Test]
    public async Task DeleteOneAsyncChecksIfIsDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

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

        Project project = new Project()
        {
            ProjectId = 1
        };

        await table.DeleteOneAsync(project);
        var count = table.Query().ToList().Count;

        Assert.AreEqual(0, count);
    }

    [Test]
    public void DeleteManyChecksIfAreDeletedFromTable()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");

        List<Project> projects = new List<Project>
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

        List<Project> projectsToDelete = new List<Project>
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

        List<Project> projects = new List<Project>
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

        List<Project> projectsToDelete = new List<Project>
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

        await table.InsertManyAsync(projects);
        var deleted = await table.DeleteManyAsync(projectsToDelete);
        var remaining = table.Query().ToList().Count;

        Assert.AreEqual(projects.Count - deleted, remaining);
    }
}
