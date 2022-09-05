namespace Swan.Test;

using NUnit.Framework;
using Swan.Data.Extensions;

[TestFixture]

public class AsyncEnumerableExtensionsTest
{
    static async IAsyncEnumerable<int> FetchItems()
    {
        for (int i = 1; i <= 10; i++)
        {
            await Task.Delay(1000);
            yield return i;
        }
    }

    [Test]
    public async Task GetAllItemsTakeFirst()
    {
        //Arrange
        CancellationToken ct = new CancellationToken();
        IAsyncEnumerable<int> items;

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
        IAsyncEnumerable<int> items = AsyncEnumerable.Empty<int>();

        //Act
        var response = await items.FirstOrDefaultAsync(ct);

        //Assert
        Assert.IsTrue(response == 0);
    }

    [Test]
    public async Task GetAllItemsCountEqualsTen()
    {
        //Arrange
        CancellationToken ct = new CancellationToken();
        IAsyncEnumerable<int> items;

        //Act
        items = FetchItems();
        var response = await items.ToListAsync(ct);

        //Assert
        Assert.IsTrue(response.Count == 10);
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
        IAsyncEnumerable<int> items = AsyncEnumerable.Empty<int>();

        //Act
        var response = await items.ToListAsync(ct);

        //Assert
        Assert.IsTrue(response.Count == 0);
    }
}
