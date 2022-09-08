namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Swan.Data.Extensions;
using System.Data;
using static Swan.Test.Mocks.ProjectRecord;

[TestFixture]
public class CommandExtensionsTest
{
    [Test]
    public void PrepareNullCommandReturnsFalse()
    {
        SqliteCommand command = null;
        var prepared = command.TryPrepare();

        Assert.IsFalse(prepared);
    }

    [Test]
    public void PrepareCommandWithConnectionClosedReturnsFalse()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        var prepared = command.TryPrepare();

        Assert.IsFalse(prepared);
    }

    [Test]
    public void PrepareCommandWithConnectionOpenReturnsTrue()
    {
        var conn = new SqliteConnection("Data Source=:memory:").EnsureConnected();
        var command = conn.CreateCommand();
        var prepared = command.TryPrepare();

        Assert.IsTrue(prepared);
    }

    [Test]
    public async Task PrepareNullCommandThrowsExceptionAsync()
    {
        SqliteCommand command = null;

        Assert.ThrowsAsync<ArgumentNullException>(() => command.TryPrepareAsync());
    }

    [Test]
    public async Task PrepareCommandWithConnectionClosedReturnsFalseAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        var prepared = await command.TryPrepareAsync();

        Assert.IsFalse(prepared);
    }

    [Test]
    public async Task PrepareCommandWithConnectionOpenReturnsTrueAsync()
    {
        var conn = new SqliteConnection("Data Source=:memory:").EnsureConnected();
        var command = conn.CreateCommand();
        var prepared = await command.TryPrepareAsync();

        Assert.IsTrue(prepared);
    }

    [Test]
    public void TryFindParameterInANullCommandReturnsExeption()
    {
        SqliteCommand command = null;

        Assert.Throws<ArgumentNullException>(() => command.TryFindParameter("Parameter", out var parameter));
    }

    [Test]
    public void TryFindParameterWhenCommandConnectionIsNullThrowsException()
    {
        var conn = new SqliteConnection();
        var command = conn.CreateCommand();
        command.Connection = null;

        Assert.Throws<ArgumentException>(() => command.TryFindParameter("Parameter", out var parameter));
    }

    [Test]
    public void TryFindParameterWhenNameIsNullOrWhiteSpaceThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();

        Assert.Throws<ArgumentNullException>(() => command.TryFindParameter("", out var parameter));
    }

    [Test]
    public void TryFindParameterWhenNameIsValidButDoesntExistsReturnsFalse()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        var found = command.TryFindParameter("Parameter", out var parameter);

        Assert.IsFalse(found);
    }

    [Test]
    public void TryFindParameterWhenNameIsValidAndExistsReturnsTrue()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        command.Parameters.Add("TestParameter", SqliteType.Text);

        var found = command.TryFindParameter("TestParameter", out var parameter);

        Assert.IsTrue(found);
    }

    [Test]
    public void GetNamedParametersWhenCommandIsNullThrowsException()
    {
        SqliteCommand command = null;

        Assert.Throws<ArgumentNullException>(() => command.GetNamedParameters().Any());
    }

    [Test]
    public void GetNamedParametersReturnsTrueIfAnyExists()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        command.Parameters.Add("TestParameter", SqliteType.Text);

        var found = command.GetNamedParameters().Any();

        Assert.IsTrue(found);
    }

    [Test]
    public void DefineParameterWhenCommandIsNullThrowsException()
    {
        SqliteCommand command = null;

        Assert.Throws<ArgumentNullException>(() => command.DefineParameter("IntParameter", System.Data.DbType.Int32));
    }

    [Test]
    public void DefineParameterWhenNameIsNullOrWhiteSpaceThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();

        Assert.Throws<ArgumentNullException>(() => command.DefineParameter("", System.Data.DbType.Int32));
    }

    [Test]
    public void DefineParameterCreatedOk()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();

        var created = command.DefineParameter("IntParameter", System.Data.DbType.Int32);

        Assert.IsNotNull(created);
    }

    [Test]
    public void DefineParameterWithClrTypeWhenCommandIsNullThrowsException()
    {
        SqliteCommand command = null;

        Assert.Throws<ArgumentNullException>(() => command.DefineParameter("IntParameter", typeof(Int32)));
    }

    [Test]
    public void DefineParameterClrTypeWhenConnectionIsNullThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        command.Connection = null;

        Assert.Throws<ArgumentException>(() => command.DefineParameter("IntParameter", typeof(Int32)));
    }

    [Test]
    public void DefineParameterClrTypeWhenNameIsNullOrWhiteSpaceThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();

        Assert.Throws<ArgumentNullException>(() => command.DefineParameter("", typeof(Int32)));
    }

    [Test]
    public void DefineParameterWhenClrTypeIsNullThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();

        Assert.Throws<ArgumentNullException>(() => command.DefineParameter("IntParameter", null));
    }

    [Test]
    public void DefineParametersWhenCommandIsNullThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var table = conn.Table<Project>("Projects");
        SqliteCommand command = null;

        Assert.Throws<ArgumentNullException>(() => command.DefineParameters(table.Columns));
    }

    [Test]
    public void DefineParametersWorksOk()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();
        var command = conn.CreateCommand();
        var table = conn.Table<Project>("Projects");
        var parameters = command.DefineParameters(table.Columns);

        Assert.IsNotNull(parameters);
    }

    [Test]
    public void SetParameterWithNoSpecificValue()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        var set = command.SetParameter("IntParameter", typeof(Int32));
        
        Assert.IsNotNull(set.Parameters);
    }

    [Test]
    public void SetParameterWhenCommandIsNullThrowsException()
    {
        SqliteCommand command = null;

        Assert.Throws<ArgumentNullException>(() => command.SetParameter("IntParameter", 0, typeof(Int32), 1));
    }

    [Test]
    public void SetParameterWhenNameIsNullOrWhiteSpaceThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();

        Assert.Throws<ArgumentNullException>(() => command.SetParameter("", 0, typeof(Int32), 1));
    }

    [Test]
    public void SetParameterWhenAllreadyExistsUpdateValue()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();


        command.SetParameter("IntParameter", 0, typeof(Int32), 1);
        var firstValue = command.Parameters[0].Value;

        command.SetParameter("IntParameter", 1, typeof(Int32), 1);
        var secondValue = command.Parameters[0].Value;

        Assert.AreNotEqual(firstValue, secondValue);
    }

    [Test]
    public void SetParameterWithDbTypeWhenCommandIsNullThrowsException()
    {
        SqliteCommand command = null;

        Assert.Throws<ArgumentNullException>(() => command.SetParameter("IntParameter", 0, DbType.Int32, 1));
    }

    [Test]
    public void SetParameterWithDbTypeWhenNameIsNullOrWhiteSpaceThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();

        Assert.Throws<ArgumentNullException>(() => command.SetParameter("", 0, DbType.Int32, 1));
    }

    [Test]
    public void SetParameterWithDbTypeWhenAllreadyExistsUpdateValue()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();


        command.SetParameter("IntParameter", 0, DbType.Int32, 1);
        var firstValue = command.Parameters[0].Value;

        command.SetParameter("IntParameter", 1, DbType.Int32, 1);
        var secondValue = command.Parameters[0].Value;

        Assert.AreNotEqual(firstValue, secondValue);     
    }

    [Test]
    public void SetParametersWhenCommandIsNullThrowsException()
    {
        SqliteCommand command = null;
        Project project = new Project();

        Assert.Throws<ArgumentNullException>(() => command.SetParameters(project));
    }

    [Test]
    public void SetParametersWhenCommandConnectionIsNullThrowsException()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        command.Connection = null;
        Project project = new Project();

        Assert.Throws<ArgumentException>(() => command.SetParameters(project));
    }

    [Test]
    public void SetParametersRunsOk()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        var command = conn.CreateCommand();
        
        Project project = new Project()
        {
            CompanyId = 1,
            EndDate = DateTime.Now,
            IsActive = true,
            Name = "Project ONE",
            ProjectScope = "My Scope",
            ProjectType = ProjectTypes.Exciting,
            StartDate = DateTime.Now.AddMonths(-1)
        };

        var cmd = command.SetParameters(project);

        Assert.AreEqual(8, cmd.Parameters.Count);
    }
}
