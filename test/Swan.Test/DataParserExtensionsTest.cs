﻿namespace Swan.Test;

using Mocks;
using System.Data;

[TestFixture]
public class DataParserExtensionsTest
{
    [Test]
    public void WhenRecordIsNullThrowsException()
    {
        IDataRecord? record = null;
        DataRow? row = null;
        Assert.Throws<ArgumentNullException>(() => record.ParseObject<Project>());
        Assert.Throws<ArgumentNullException>(() => record.ParseExpando());
        Assert.Throws<ArgumentNullException>(() => row.ToDataRecord().ParseObject(null));
    }

    [Test]
    public void WhenTypeIsNullThrowsException()
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

        var command = conn.CreateCommand();
        command.CommandText = "Select * from Projects;";

        var reader = command.ExecuteReader();

        var result = new Project();

        reader.Read();
        Assert.Throws<ArgumentNullException>(() => reader.ParseObject(null));
    }

    [Test]
    public void ParseObjetcWhenNullValueInColumnReturnItsTypeDefault()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

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

        var result = new Project();

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
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

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
        var table = new DataTable();

        table.Columns.Add("ProjectId");
        table.Columns.Add("CompanyId");
        table.Columns.Add("EndDate");
        table.Columns.Add("IsActive");
        table.Columns.Add("Name");
        table.Columns.Add("ProjectScope");
        table.Columns.Add("ProjectType");
        table.Columns.Add("StartDate");

        var row = table.NewRow();
        row["ProjectId"] = 1;
        row["CompanyId"] = null;
        row["EndDate"] = DateTime.Now;
        row["IsActive"] = true;
        row["Name"] = "Project ONE";
        row["ProjectScope"] = "My Scope";
        row["ProjectType"] = ProjectTypes.Exciting;
        row["StartDate"] = DateTime.Now.AddMonths(-1);
        table.Rows.Add(row);

        Assert.Throws<ArgumentNullException>(() => table.Rows[0].ToDataRecord().ParseObject(null));
    }

    [Test]
    public void ExpandoDataRowWhenNullValueInColumnReturnItsTypeDefault()
    {
        var table = new DataTable();

        table.Columns.Add("ProjectId");
        table.Columns.Add("CompanyId");
        table.Columns.Add("EndDate");
        table.Columns.Add("IsActive");
        table.Columns.Add("Name");
        table.Columns.Add("ProjectScope");
        table.Columns.Add("ProjectType");
        table.Columns.Add("StartDate");

        var row = table.NewRow();
        row["ProjectId"] = 1;
        row["CompanyId"] = null;
        row["EndDate"] = DateTime.Now;
        row["IsActive"] = true;
        row["Name"] = "Project ONE";
        row["ProjectScope"] = "My Scope";
        row["ProjectType"] = ProjectTypes.Exciting;
        row["StartDate"] = DateTime.Now.AddMonths(-1);
        table.Rows.Add(row);

        var ex = table.Rows[0].ToDataRecord().ParseExpando().ToList();

        Assert.IsTrue(ex[1].Key == "CompanyId" && ex[1].Value == null);
    }
}
