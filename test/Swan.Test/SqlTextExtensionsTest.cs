namespace Swan.Test;

using Mocks;

[TestFixture]

public class SqlTextExtensionsTest
{
    [Test]
    public void AppendWordJustTheSELECTWordWhenFieldsIsEmpty()
    {
        var fields = Array.Empty<string>();
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.BeginCommandText().Select(fields).EndCommandText();

        Assert.AreEqual("SELECT", command.CommandText);
    }

    [Test]
    public void AppendWordSelectAndSeparateFieldsInCommandText()
    {
        var fields = new[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.BeginCommandText().Select(fields).EndCommandText();

        Assert.AreEqual("SELECT [Field1], [Field2], [Field3]", command.CommandText);
    }

    [Test]
    public void ProvidingATableCreatesASelectFieldsFromTableFormatCommanText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();

        var command = conn.BeginCommandText().Select(table).EndCommandText();

        Assert.AreEqual(
            "SELECT [ProjectId], [Name], [ProjectType], [CompanyId], [IsActive], [StartDate], [EndDate], [ProjectScope] FROM [Projects]",
            command.CommandText);
    }

    [Test]
    public void ProvidingATableColumnsCreatesASelectFieldsWithOutFromTableFormatCommandText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        var command = conn.BeginCommandText().Select(table.Columns).EndCommandText();

        Assert.AreEqual(
            "SELECT [ProjectId], [Name], [ProjectType], [CompanyId], [IsActive], [StartDate], [EndDate], [ProjectScope]",
            command.CommandText);
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
        var fields = new[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Fields(fields).EndCommandText();

        Assert.AreEqual("[Field1], [Field2], [Field3]", command.CommandText);
    }

    [Test]
    public void FromWhenNoTableOrSchemaIsGivenReturnJustTheWordFrom()
    {
        var fields = new[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().From().EndCommandText();

        Assert.AreEqual("FROM", command.CommandText);
    }

    [Test]
    public void FromWhenTableNameGivenReturnFromTableFormat()
    {
        var fields = new[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().From("TableName").EndCommandText();

        Assert.AreEqual("FROM [TableName]", command.CommandText);
    }

    [Test]
    public void FromWhenTableNameGivenReturnFromSchemaTableFormat()
    {
        var fields = new[] { "Field1", "Field2", "Field3" };
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().From("TableName", "SchemaName").EndCommandText();

        Assert.AreEqual("FROM [SchemaName].[TableName]", command.CommandText);
    }

    [Test]
    public void ProvidingATableReturnsFromFormat()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var table = conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteTableCommand();
        var command = conn.BeginCommandText().From(table).EndCommandText();

        Assert.AreEqual("FROM [Projects]", command.CommandText);
    }

    [Test]
    public void InsertWhenTableIsNullOrWhiteSpace()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().InsertInto("").EndCommandText();

        Assert.AreEqual("INSERT INTO", command.CommandText);
    }

    [Test]
    public void InsertWhenTableIsGiven()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().InsertInto("TableName").EndCommandText();

        Assert.AreEqual("INSERT INTO [TableName]", command.CommandText);
    }

    [Test]
    public void WhereWhenConditionIsNullOrWhiteSpace()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Where().EndCommandText();

        Assert.AreEqual("WHERE", command.CommandText);
    }

    [Test]
    public void WhereWhenConditionIsGiven()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Where("1 = 1").EndCommandText();

        Assert.AreEqual("WHERE 1 = 1", command.CommandText);
    }

    [Test]
    public void AppendsBetweenToCommandText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().IsBetween().EndCommandText();

        Assert.AreEqual("BETWEEN", command.CommandText);
    }

    [Test]
    public void AppendsOrToCommandText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Or().EndCommandText();

        Assert.AreEqual("OR", command.CommandText);
    }

    [Test]
    public void AppendsAndToCommandText()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().And().EndCommandText();

        Assert.AreEqual("AND", command.CommandText);
    }

    [Test]
    public void WhenParameterIsNullOrWhiteSpace()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Parameter("").EndCommandText();

        Assert.AreEqual(string.Empty, command.CommandText);
    }

    [Test]
    public void WhenParameterIsGiven()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Parameter("ParameterName").EndCommandText();

        Assert.AreEqual("$ParameterName", command.CommandText);
    }

    [Test]
    public void WhenParameterCountIsZero()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var parameters = new string[] { };

        var command = conn.BeginCommandText().Parameters(parameters).EndCommandText();

        Assert.AreEqual(string.Empty, command.CommandText);
    }

    [Test]
    public void WhenParameterCountIsBiggerThanZero()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var parameters = new[] { "Parameter1", "Parameter2" };

        var command = conn.BeginCommandText().Parameters(parameters).EndCommandText();

        Assert.AreEqual("$Parameter1, $Parameter2", command.CommandText);
    }

    [Test]
    public void WhenFieldsParameterCountIsZero()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var fieldsAndParameters = new string[] { };

        var command = conn.BeginCommandText().FieldsAndParameters(fieldsAndParameters).EndCommandText();

        Assert.AreEqual(string.Empty, command.CommandText);
    }

    [Test]
    public void WhenFieldsAndParameterCountIsBiggerThanZero()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var fieldsAndParameters = new[] { "Parameter1", "Parameter2" };

        var command = conn.BeginCommandText().FieldsAndParameters(fieldsAndParameters).EndCommandText();

        Assert.AreEqual("[Parameter1] = $Parameter1 , [Parameter2] = $Parameter2", command.CommandText);
    }

    [Test]
    public void WhenOrderByParameterIsNullOrWhiteSpace()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().OrderBy().EndCommandText();

        Assert.AreEqual("ORDER BY", command.CommandText);
    }

    [Test]
    public void WhenOderByParameterAreGiven()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var parameters = new[] { "Parameter1", "Parameter2" };

        var command = conn.BeginCommandText().OrderBy(parameters).EndCommandText();

        Assert.AreEqual("ORDER BY [Parameter1], [Parameter2]", command.CommandText);
    }

    [Test]
    public void WhenLimitParametersAreDefault()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Limit().EndCommandText();

        Assert.AreEqual(string.Empty, command.CommandText);
    }

    [Test]
    public void WhenLimitParametersAreGiven()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var command = conn.BeginCommandText().Limit(1, 1).EndCommandText();

        Assert.AreEqual(" LIMIT 1 OFFSET 1", command.CommandText);
    }

    [Test]
    public void WhenCommandSourceIsNullThrowsException()
    {
        DbCommandSource? command = null;
        IDbTableSchema? table = null;
        IReadOnlyList<IDbColumnSchema>? columns = null;
        var conn = new SqliteConnection("Data Source=:memory:");
        var fields = Array.Empty<string>();

        Assert.Throws<ArgumentNullException>(() => command.Select(fields));
        Assert.Throws<ArgumentNullException>(() => conn.BeginCommandText().Select(table).EndCommandText());
        Assert.Throws<ArgumentNullException>(() => conn.BeginCommandText().Select(columns).EndCommandText());
        Assert.Throws<ArgumentNullException>(() => command.Field(""));
        Assert.Throws<ArgumentNullException>(() => command.Fields(fields));
        Assert.Throws<ArgumentNullException>(() => command.From());
        Assert.Throws<ArgumentNullException>(() => conn.BeginCommandText().From(table).EndCommandText());
        Assert.Throws<ArgumentNullException>(() => command.InsertInto());
        Assert.Throws<ArgumentNullException>(() => command.Table("TableName"));
        Assert.Throws<ArgumentNullException>(() => command.Where(""));
        Assert.Throws<ArgumentNullException>(() => command.IsBetween());
        Assert.Throws<ArgumentNullException>(() => command.Or());
        Assert.Throws<ArgumentNullException>(() => command.And());
        Assert.Throws<ArgumentNullException>(() => command.Parameter(""));
        Assert.Throws<ArgumentNullException>(() => command.Parameters("Parameter1", "Parameter2"));
        Assert.Throws<ArgumentNullException>(() => command.FieldsAndParameters(new[] { "Parameter1", "Parameter2" }));
        Assert.Throws<ArgumentNullException>(() => command.OrderBy("Parameter1", "Parameter2"));
        Assert.Throws<ArgumentNullException>(() => command.Limit());
    }
}
