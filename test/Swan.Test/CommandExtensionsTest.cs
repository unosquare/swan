namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Swan.Data.Extensions;

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
}
