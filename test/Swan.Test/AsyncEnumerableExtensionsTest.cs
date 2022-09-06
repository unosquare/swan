namespace Swan.Test;

using Microsoft.Data.Sqlite;
using NUnit.Framework;
using Swan.Data.Extensions;
using static Swan.Test.Mocks.ProjectRecord;

[TestFixture]

public class AsyncEnumerableExtensionsTest
{
    //static async IAsyncEnumerable<int> FetchItems()
    //{
    //    for (int i = 1; i <= 10; i++)
    //    {
    //        await Task.Delay(1000);
    //        yield return i;
    //    }
    //}

    private async IAsyncEnumerable<Project> FetchItems()
    {
        CancellationToken ct = new CancellationToken();
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

        var result = conn.QueryAsync<Project>("Select * from Projects;");
        
        await foreach (var item in result)
            yield return item;
    }

    private async IAsyncEnumerable<Project> FetchItemsEmpty()
    {
        CancellationToken ct = new CancellationToken();
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var result = conn.QueryAsync<Project>("Select * from Projects;");

        await foreach (var item in result)
            yield return item;
    }



    [Test]
    public async Task GetAllItemsTakeFirst()
    {
        //Arrange
        CancellationToken ct = new CancellationToken();
        IAsyncEnumerable<Project> items;

        //Act
        items = FetchItems();
        var response = await items.FirstOrDefaultAsync(ct);

        //Assert
        Assert.IsTrue(response != null);
    }

    [Test]
    public async Task FirstOrDefaultItemsIsNull()
    {
        //Arrange
        CancellationToken ct = new CancellationToken();
        IAsyncEnumerable<int> items = null;

        //Act
        var response = await items.FirstOrDefaultAsync(ct);

        //Assert
        Assert.IsTrue(response == 0);
    }
    
    [Test]
    public async Task FirstOrDefaultItemsIsEmpty()
    {
        //Arrange
        CancellationToken ct = new CancellationToken();
        IAsyncEnumerable<Project> items;

        //Act
        items = FetchItemsEmpty();
        var response = await items.FirstOrDefaultAsync(ct);

        //Assert
        Assert.IsTrue(response is null);
    }

    [Test]
    public async Task GetAllItemsCountEqualsTen()
    {
        //Arrange
        CancellationToken ct = new CancellationToken();
        IAsyncEnumerable<Project> items;

        //Act
        items = FetchItems();
        var response = await items.ToListAsync(ct);

        //Assert
        Assert.IsTrue(response.Count == 1);
    }

    [Test]
    public async Task ToListAsyncItemsIsNull()
    {
        //Arrange
        CancellationToken ct = new CancellationToken();
        IAsyncEnumerable<int> items = null;

        //Act
        var response = await items.ToListAsync(ct);

        //Assert
        Assert.IsTrue(response.Count == 0);
    }

    [Test]
    public async Task ToListAsyncItemsIsEmpty()
    {
        //Arrange
        CancellationToken ct = new CancellationToken();
        IAsyncEnumerable<Project> items;

        //Act
        items = FetchItemsEmpty();
        var response = await items.ToListAsync(ct);

        //Assert
        Assert.IsTrue(response.Count == 0);
    }
}
