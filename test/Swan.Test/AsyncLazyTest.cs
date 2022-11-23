namespace Swan.Test;

using NUnit.Framework;
using Gizmos;
using Mocks;

[TestFixture]
public class AsyncLazyTest
{
    static AsyncLazy<LargeObject>? lazyLargeObject;

    static async Task<LargeObject> InitLargeObject()
    {   
        Random random = new Random();
        await Task.Delay(100);
        return new(random.Next());
    }

    [Test]
    public async Task WithLargObject_getsValueAsync()
    {
        lazyLargeObject = new(InitLargeObject);

        var large = await lazyLargeObject.GetValueAsync();

        Assert.IsTrue(lazyLargeObject.IsValueCreated);
    }
}
