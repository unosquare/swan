namespace Swan.Test;

using NUnit.Framework;
using Swan.Data.Context;
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

        Assert.AreEqual(result.Name, "Project ONE");
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

        Assert.AreEqual(result.Name, "Project ONE");
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

        Assert.AreEqual(project.Name, "Project ONE");
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

        Assert.AreEqual(project.Name, "Project ONE");
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

        Assert.AreEqual(projectsInserted, 2);
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

        var projectsInserted = table.InsertMany(projects);

        Assert.AreEqual(projectsInserted, 2);
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

        Assert.AreEqual(projecstUpdated, 1);
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

        Assert.AreEqual(projectUpdated, 1);
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

        Assert.AreEqual(updatedCount, 2);
        Assert.AreEqual(updatedTable[0].Name, "Project ONE Updated");
        Assert.AreEqual(updatedTable[1].Name, "Project TWO Updated");
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

        Assert.AreEqual(updatedCount, 2);
        Assert.AreEqual(updatedTable[0].Name, "Project ONE Updated");
        Assert.AreEqual(updatedTable[1].Name, "Project TWO Updated");
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
        var count = table.Query().ToList().Count();

        Assert.AreEqual(count, 0);
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
        var count = table.Query().ToList().Count();

        Assert.AreEqual(count, 0);
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
        table.DeleteMany(projectsToDelete);
        var count = table.Query().ToList().Count();

        Assert.AreEqual(count, 0);
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
        await table.DeleteManyAsync(projectsToDelete);
        var count = table.Query().ToList().Count();

        Assert.AreEqual(count, 0);
    }
}
