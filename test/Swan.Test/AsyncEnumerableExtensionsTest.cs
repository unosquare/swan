namespace Swan.Test;

using Mocks;

[TestFixture]

public class AsyncEnumerableExtensionsTest
{
    private static async IAsyncEnumerable<Project> FetchItems()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        await conn.Table<Project>("Projects").InsertOneAsync(new()
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

    private static async IAsyncEnumerable<Project> FetchItemsEmpty()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.EnsureConnected().TableBuilder<Project>("Projects").ExecuteDdlCommand();

        var result = conn.QueryAsync<Project>("Select * from Projects;");

        await foreach (var item in result)
            yield return item;
    }

    [Test]
    public async Task GetAllItemsTakeFirst()
    {
        // Arrange
        var ct = new CancellationToken();

        // Act
        var items =
            FetchItems();
        var response = await items.FirstOrDefaultAsync(ct);

        // Assert
        Assert.IsTrue(response != null);
    }

    [Test]
    public async Task FirstOrDefaultItemsIsNull()
    {
        // Arrange
        var ct = new CancellationToken();
        IAsyncEnumerable<int>? items = null;

        // Act
        var response = await items.FirstOrDefaultAsync(ct);

        // Assert
        Assert.IsTrue(response == 0);
    }

    [Test]
    public async Task FirstOrDefaultItemsIsEmpty()
    {
        // Arrange
        var ct = new CancellationToken();

        var items =
            // Act
            FetchItemsEmpty();
        var response = await items.FirstOrDefaultAsync(ct);

        // Assert
        Assert.IsTrue(response is null);
    }

    [Test]
    public async Task GetAllItemsCountEqualsTen()
    {
        // Arrange
        var ct = new CancellationToken();

        var items =
            // Act
            FetchItems();
        var response = await items.ToListAsync(ct);

        // Assert
        Assert.IsTrue(response.Count == 1);
    }

    [Test]
    public async Task ToListAsyncItemsIsNull()
    {
        // Arrange
        var ct = new CancellationToken();
        IAsyncEnumerable<int>? items = null;

        // Act
        var response = await items.ToListAsync(ct);

        // Assert
        Assert.IsTrue(response.Count == 0);
    }

    [Test]
    public async Task ToListAsyncItemsIsEmpty()
    {
        // Arrange
        var ct = new CancellationToken();

        var items =
            // Act
            FetchItemsEmpty();
        var response = await items.ToListAsync(ct);

        // Assert
        Assert.IsTrue(response.Count == 0);
    }
}
