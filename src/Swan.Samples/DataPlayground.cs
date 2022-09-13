namespace Swan.Samples;

using Microsoft.Data.Sqlite;
using Swan.Data.Extensions;
using Logging;
using Platform;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

internal static class DataPlayground
{
    private const string ConnectionString = "data source=.;initial catalog=unocorp-timecore;Integrated Security=true;";

    public static async Task BasicExample()
    {
        await SqliteStuff();
        return;
        var liteName = typeof(SqliteConnection).FullName;

        // Create a connection as usual.
        await using var connection = new SqlConnection(ConnectionString);

        var text = connection.TableBuilder<Project>("Projects").CreateDdlCommand().CommandText;

        var names = await connection.GetTableNamesAsync();

        var table = (await connection.TableAsync("Projects")).GeneratePocoCode("Project");

        // You can configure the default timeout for commands created using the SWAN API.
        connection.Provider().WithDefaultCommandTimeout(TimeSpan.FromSeconds(10));

        //var conn = new SqliteConnection("Data Source=hello.db");
        // var tableNames = await conn.TableNames();
        await connection.TestSampleInsertButBetter();
    }

    private static async Task SqliteStuff()
    {
        var conn = new SqliteConnection("Data Source=mydb.sqlite");
        var result = await conn.TableBuilder<Project>("Projects").ExecuteDdlCommandAsync();

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

        Console.WriteLine(project);
    }

    private static async Task TestSampleInsertButBetter(this DbConnection connection)
    {
        // Now, instead of doing all that stuff manually, if we play with
        // typical game rules, we can do stuff in a much simpler way :)
        var projects = connection.Table<Project>("Projects");

        // We'll use a transaction in this example. We won't actually insert anything.
        // since we will be rolling back the transaction.
        await using var tran = await connection.BeginTransactionAsync();

        // Create a dummy record
        var dummyProject = new Project()
        {
            Name = "Dummy",
            CompanyId = 61,
            ProjectScope = "DummyScope"
        };

        var items = new[]
        {
            dummyProject with { Name = "Dummy 1", ProjectType = ProjectTypes.Boring },
            dummyProject with { Name = "Dummy 2", ProjectType = ProjectTypes.Exciting },
            dummyProject with { Name = "Dummy 3", ProjectType = ProjectTypes.Exciting },
        };

        foreach (var item in items)
        {
            var inserted = await projects.InsertOneAsync(item, tran);
            $"ADDED:\r\n{inserted}".Info();
        }

        // We won't actually insert anything. We'll rollback the transaction.
        tran.Rollback();
        Terminal.Flush();
    }

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
}

