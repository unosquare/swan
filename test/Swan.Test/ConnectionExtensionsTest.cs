namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Swan.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[TestFixture]
public class ConnectionExtensionsTest
{
    /// <summary>
    /// Represents a record that maps to the dbo.Projects table.
    /// </summary>
    [Table("Projects", Schema = "dbo")]
    public record Project
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Project" /> class.
        /// </summary>
        public Project() { /* placeholder */ }

        /// <summary>
        /// Gets or sets a value for Project Id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(nameof(ProjectId), Order = 0)]
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets a value for Name.
        /// </summary>
        [MaxLength(100)]
        [Column(nameof(Name), Order = 1)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a value for Project Type.
        /// </summary>
        [Column(nameof(ProjectType), Order = 2)]
        public ProjectTypes ProjectType { get; set; }

        /// <summary>
        /// Gets or sets a value for Company Id.
        /// </summary>
        [Column(nameof(CompanyId), Order = 3)]
        public int? CompanyId { get; set; }

        /// <summary>
        /// Gets or sets a value for Is Active.
        /// </summary>
        [Column(nameof(IsActive), Order = 4)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value for Start Date.
        /// </summary>
        [Column(nameof(StartDate), Order = 5)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets a value for End Date.
        /// </summary>
        [Column(nameof(EndDate), Order = 6)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value for Project Scope.
        /// </summary>
        [MaxLength(2147483647)]
        [Column(nameof(ProjectScope), Order = 7)]
        public string? ProjectScope { get; set; }
    }

    public enum ProjectTypes
    {
        Boring,
        Exciting
    }


    [Test]
    public async Task CreateProjectsTableAndInsertOneRowAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.TableBuilder<Project>("Projects").ExecuteDdlCommandAsync();

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

        Assert.AreEqual(project?.Name, "Project ONE");
    }
    
    [Test]
    public void CreateProjectsTableAndInsertOneRow()
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

        Assert.AreEqual(project?.Name, "Project ONE");
    }

    [Test]
    public void OpenConnectionAndGetItsProvidersCreateListTablesCommand()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var provider = conn.Provider();
        var command = provider.CreateListTablesCommand(conn).CommandText;

        Assert.AreEqual(command, "SELECT name AS [Name], '' AS [Schema] FROM (SELECT * FROM sqlite_schema UNION ALL SELECT * FROM sqlite_temp_schema) WHERE type= 'table' ORDER BY name");
    }

    [Test]
    public void CreateTablesAndGetTableNames()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.TableBuilder<Project>("ProjectsTableOne").ExecuteDdlCommand();
        conn.TableBuilder<Project>("ProjectsTableTwo").ExecuteDdlCommand();
        conn.TableBuilder<Project>("ProjectsTableThree").ExecuteDdlCommand();

        var tableNames = conn.GetTableNames();
       
        Assert.IsTrue(tableNames.Any());
        Assert.IsTrue(tableNames.Count == 3);
    }

    [Test]
    public async Task CreateTablesAndGetTableNamesAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.TableBuilder<Project>("ProjectsTableOne").ExecuteDdlCommandAsync();
        await conn.TableBuilder<Project>("ProjectsTableTwo").ExecuteDdlCommandAsync();
        await conn.TableBuilder<Project>("ProjectsTableThree").ExecuteDdlCommandAsync();

        var tableNames = conn.GetTableNamesAsync();

        Assert.IsTrue(tableNames.Result.Any());
        Assert.IsTrue(tableNames.Result.Count == 3);
    }

    [Test]
    public async Task CreateProjectsTableAndInsertsQueryAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.TableBuilder<Project>("Projects").ExecuteDdlCommandAsync();

        var execution = await conn.
            ExecuteNonQueryAsync($"Insert into Projects (CompanyId, EndDate, IsActive, Name, ProjectScope, ProjectType, StartDate)" +
            " values " +
            $" (1,'{DateTime.Now}','{true}','Project ONE','My Scope', '{ProjectTypes.Exciting}','{DateTime.Now.AddMonths(-1)}');");

        Assert.IsTrue(execution == 1);
    }

    [Test]
    public void CreateProjectsTableAndInsertsQuery()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var execution = conn.
            ExecuteNonQuery($"Insert into Projects (CompanyId, EndDate, IsActive, Name, ProjectScope, ProjectType, StartDate)" +
            " values " +
            $" (1,'{DateTime.Now}','{true}','Project ONE','My Scope', '{ProjectTypes.Exciting}','{DateTime.Now.AddMonths(-1)}');");

        Assert.IsTrue(execution == 1);
    }

    [Test]
    public async Task CreateProjectsTableAndInsertOneRowAndExecuteScalarAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.TableBuilder<Project>("Projects").ExecuteDdlCommandAsync();

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
        conn.TableBuilder<Project>("Projects").ExecuteDdlCommand();

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

        var scalar = conn.ExecuteScalar("select Name, ProjectScope, ProjectType from Projects");

        Assert.AreEqual(scalar, "Project ONE");
    }
}
