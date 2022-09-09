namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Swan.Data;
using Swan.Data.Extensions;
using Swan.Data.Schema;
using static Swan.Test.Mocks.ProjectRecord;

[TestFixture]

public class SqlTextExtensionsTest
{
    [Test]
    public void SelectFieldsWhenCommandSourceIsNullThrowsException()
    {
        var fields = new string[] { };
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Select(fields));
    }

    [Test]
    public void AppendWordJustTheSELECTWordWhenFieldsIsEmpty()
    {
        var fields = new string[] { };
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.BeginCommandText().Select(fields).EndCommandText();

        Assert.AreEqual("SELECT", command.CommandText);
    }

    [Test]
    public void AppendWordSelectAndSeparateFieldsInCommandText()
    {
        var fields = new string[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.BeginCommandText().Select(fields).EndCommandText();

        Assert.AreEqual("SELECT [Field1], [Field2], [Field3]", command.CommandText);
    }

    [Test]
    public void SelectTableWhenTableIsNullThrowsException()
    {
        IDbTableSchema table = null;
        var conn = new SqliteConnection("Data Source=:memory:");

        Assert.Throws<ArgumentNullException>(() => conn.BeginCommandText().Select(table).EndCommandText());
    }

    [Test]
    public void ProvidingATableCreatesASelectFieldsFromTableFormatCommanText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();
        var table = conn.Table<Project>("Projects");

        var command = conn.BeginCommandText().Select(table).EndCommandText();

        Assert.AreEqual("SELECT [ProjectId], [Name], [ProjectType], [CompanyId], [IsActive], [StartDate], [EndDate], [ProjectScope] FROM [Projects]",
            command.CommandText);
    }

    [Test]
    public void SelectColumnsWhenColumnsIsNullThrowsException()
    {
        IReadOnlyList<IDbColumnSchema> Columns = null;
        var conn = new SqliteConnection("Data Source=:memory:");

        Assert.Throws<ArgumentNullException>(() => conn.BeginCommandText().Select(Columns).EndCommandText());
    }

    [Test]
    public void ProvidingATableColumnsCreatesASelectFieldsWithOutFromTableFormatCommanText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();
        var table = conn.Table<Project>("Projects");

        var command = conn.BeginCommandText().Select(table.Columns).EndCommandText();

        Assert.AreEqual("SELECT [ProjectId], [Name], [ProjectType], [CompanyId], [IsActive], [StartDate], [EndDate], [ProjectScope]",
            command.CommandText);
    }

    [Test]
    public void FieldWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Field(""));
    }

    [Test]
    public void FieldsWhenItemIsEmptyReturnsEmptyCommandText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.BeginCommandText().Field("").EndCommandText();

        Assert.AreEqual("", command.CommandText);
    }

    [Test]
    public void FieldsWhenIsNotEmptyAppendsItemToCommandText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.BeginCommandText().Field("Field1").EndCommandText();

        Assert.AreEqual("[Field1]", command.CommandText);
    }

    [Test]
    public void FieldsWhenCommandSourceIsNullThrowsException()
    {
        var fields = new string[] { };
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Fields(fields));
    }

    [Test]
    public void FieldsWhenFieldsIsEmptyReturnsAsterisks()
    {
        var fields = new string[] { };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Fields(fields).EndCommandText();

        Assert.AreEqual("*", command.CommandText);
    }

    [Test]
    public void FieldsWhenFieldsIsNotEmptyReturnsThemFormatted()
    {
        var fields = new string[] {"Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Fields(fields).EndCommandText();

        Assert.AreEqual("[Field1], [Field2], [Field3]", command.CommandText);
    }

    [Test]
    public void FromWhenCommandSourceIsNullThrowsException()
    {
        var fields = new string[] { };
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.From());
    }

    [Test]
    public void FromWhenNoTableOrSchemaIsGivenReturnJustTheWordFrom()
    {
        var fields = new string[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().From().EndCommandText();

        Assert.AreEqual("FROM", command.CommandText);
    }

    [Test]
    public void FromWhenTableNameGivenReturnFromTableFormat()
    {
        var fields = new string[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().From("TableName").EndCommandText();

        Assert.AreEqual("FROM [TableName]", command.CommandText);
    }

    [Test]
    public void FromWhenTableNameGivenReturnFromSchemaTableFormat()
    {
        var fields = new string[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().From("TableName","SchemaName").EndCommandText();

        Assert.AreEqual("FROM [SchemaName].[TableName]", command.CommandText);
    }

    [Test]
    public void FromTableWhenTableIsNullThrowsException()
    {
        IDbTableSchema table = null;
        var conn = new SqliteConnection("Data Source=:memory:");

        Assert.Throws<ArgumentNullException>(() => conn.BeginCommandText().From(table).EndCommandText());
    }

    [Test]
    public void ProvidingATableReturnsFromFormat()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();
        var table = conn.Table<Project>("Projects");

        var command = conn.BeginCommandText().From(table).EndCommandText();

        Assert.AreEqual("FROM [Projects]", command.CommandText);
    }

    [Test]
    public void InsertWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.InsertInto());
    }

    [Test]
    public void TableWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Table("TableName"));
    }

    [Test]
    public void WhereWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Where(""));
    }

    [Test]
    public void IsBetweenWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.IsBetween());
    }

    [Test]
    public void OrWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Or());
    }

    [Test]
    public void AndWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.And());
    }

    [Test]
    public void ParameterWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Parameter(""));
    }

    [Test]
    public void ParametersWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Parameters(new string[] { "Parameter1", "Parameter2" }));
    }

    [Test]
    public void FieldsAndParametersWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.FieldsAndParameters(new string[] { "Parameter1", "Parameter2" }));
    }

    [Test]
    public void OrderByWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.OrderBy(new string[] { "Parameter1", "Parameter2" }));
    }

    [Test]
    public void LimitWhenCommandSourceIsNullThrowsException()
    {
        CommandSource command = null;

        Assert.Throws<ArgumentNullException>(() => command.Limit());
    }
}

