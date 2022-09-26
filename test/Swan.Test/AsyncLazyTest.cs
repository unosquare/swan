namespace Swan.Test;

using NUnit.Framework;
using Swan.Gizmos;
using Swan.Test.Mocks;

[TestFixture]
public class AsyncLazyTest
{
    static async Task<LargeObject> InitLargeObject()
    { 
        var large = new LargeObject(Environment.CurrentManagedThreadId);
        return large;
    }

    static async void ThreadProc(object state)
    {
        var large = await lazyLargeObject.GetValueAsync();

        lock (large)
        {
            large.Data[0] = Environment.CurrentManagedThreadId;
        }
    }

    static AsyncLazy<LargeObject>? lazyLargeObject;

    [Test]
    public void WithLargObject_getsValueAsync()
    {
        lazyLargeObject = new AsyncLazy<LargeObject>(InitLargeObject);

        Thread[] threads = new Thread[3];
        for (int i = 0; i < 3; i++)
        {
            threads[i] = new Thread(ThreadProc);
            threads[i].Start();
        }
        
        foreach (Thread t in threads)
        {
            t.Join();
        }

        Assert.IsTrue(lazyLargeObject.IsValueCreated);
    }
}
