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

}

