namespace Swan.Test;

using NUnit.Framework;
using Swan.Gizmos;
using Swan.Test.Mocks;

[TestFixture]
public class AsyncLazyTest
{
    static AsyncLazy<LargeObject>? lazyLargeObject;

    static async Task<LargeObject> InitLargeObject()
    {   
        Random random = new Random();
        await Task.Delay(100);
        return new LargeObject(random.Next());
    }

    [Test]
    public async Task WithLargObject_getsValueAsync()
    {
        lazyLargeObject = new AsyncLazy<LargeObject>(InitLargeObject);

        var large = await lazyLargeObject.GetValueAsync();

        Assert.IsTrue(lazyLargeObject.IsValueCreated);
    }
}
