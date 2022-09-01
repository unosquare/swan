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
    public async Task CreateProjectTableAndInsertOneRowAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var result = await conn.TableBuilder<Project>("Projects").ExecuteDdlCommandAsync();

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

        Assert.AreEqual(project.Name, "Project ONE");
    }
    
    [Test]
    public void CreateProjectTableAndInsertOneRow()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var result = conn.TableBuilder<Project>("Projects").ExecuteDdlCommand();

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
    public void OpenAConnectionAndGetItsProvidersCreateListTablesCommand()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var provider = conn.Provider();
        var command = provider.CreateListTablesCommand(conn).CommandText;

        Assert.AreEqual(command, "SELECT name AS [Name], '' AS [Schema] FROM (SELECT * FROM sqlite_schema UNION ALL SELECT * FROM sqlite_temp_schema) WHERE type= 'table' ORDER BY name");
    }
}
