namespace Swan.Test;

using NUnit.Framework;
using System.Data;
using Swan.Data.Extensions;
using static Swan.Test.Mocks.ProjectRecord;
using Microsoft.Data.Sqlite;

[TestFixture]
public class DataParserExtensionsTest
{
    [Test]
    public void WhenRecordIsNullThrowsException()
    {
        IDataRecord record = null;
        DataRow row = null;
        Assert.Throws<ArgumentNullException>(() => record.ParseObject<Project>(null));
        Assert.Throws<ArgumentNullException>(() => record.ParseExpando());
        Assert.Throws<ArgumentNullException>(() => row.ParseExpando());
        Assert.Throws<ArgumentNullException>(() => row.ParseObject(null));

    }

    [Test]
    public void WhenTypeIsNullThrowsException()
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

        var command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";

        var reader = command.ExecuteReader();

        Project result = new Project();

        reader.Read();
        Assert.Throws<ArgumentNullException>(() => reader.ParseObject(null));
    }

    [Test]
    public void ParseObjetcWhenNullValueInColumnReturnItsTypeDefault()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        var project = table.InsertOne(new()
        {
            CompanyId = null,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        });

        var command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";

        var reader = command.ExecuteReader();

        Project result = new Project();

        while (reader.Read())
        {
            result = reader.ParseObject<Project>();
        }

        Assert.AreEqual(result.Name, "Project ONE");
        Assert.IsNull(result.CompanyId);
    }

    [Test]
    public void ExpandoWhenNullValueInColumnReturnItsTypeDefault()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        var project = table.InsertOne(new()
        {
            CompanyId = null,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        });

        var command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";

        var reader = command.ExecuteReader();
        reader.Read();
        var ex = reader.ParseExpando().ToList();

        Assert.IsTrue(ex[3].Key == "CompanyId" && ex[3].Value == null);
    }

    [Test]
    public void ParseObjectWhenNullValueInColumnReturnItsTypeDefault()
    {
        DataTable table = new DataTable();

        table.Columns.Add("ProjectId");
        table.Columns.Add("CompanyId");
        table.Columns.Add("EndDate");
        table.Columns.Add("IsActive");
        table.Columns.Add("Name");
        table.Columns.Add("ProjectScope");
        table.Columns.Add("ProjectType");
        table.Columns.Add("StartDate");

        DataRow row = table.NewRow();
        row["ProjectId"] = 1;
        row["CompanyId"] = null;
        row["EndDate"] = DateTime.Now;
        row["IsActive"] = true;
        row["Name"] = "Project ONE";
        row["ProjectScope"] = "My Scope";
        row["ProjectType"] = ProjectTypes.Exciting;
        row["StartDate"] = DateTime.Now.AddMonths(-1);
        table.Rows.Add(row);

        Assert.Throws<ArgumentNullException>(() => table.Rows[0].ParseObject(null));
    }

    [Test]
    public void ExpandoDataRowWhenNullValueInColumnReturnItsTypeDefault()
    {
        DataTable table = new DataTable();

        table.Columns.Add("ProjectId");
        table.Columns.Add("CompanyId");
        table.Columns.Add("EndDate");
        table.Columns.Add("IsActive");
        table.Columns.Add("Name");
        table.Columns.Add("ProjectScope");
        table.Columns.Add("ProjectType");
        table.Columns.Add("StartDate");

        DataRow row = table.NewRow();
        row["ProjectId"] = 1;
        row["CompanyId"] = null;
        row["EndDate"] = DateTime.Now;
        row["IsActive"] = true;
        row["Name"] = "Project ONE";
        row["ProjectScope"] = "My Scope";
        row["ProjectType"] = ProjectTypes.Exciting;
        row["StartDate"] = DateTime.Now.AddMonths(-1);
        table.Rows.Add(row);

        var ex = table.Rows[0].ParseExpando().ToList();

        Assert.IsTrue(ex[1].Key == "CompanyId" && ex[1].Value == null);
    }
}
