namespace Swan.Test;

using NUnit.Framework;
using Swan.Gizmos;
using Swan.Test.Mocks;

[TestFixture]
public class AsyncLazyTest
{
    static async Task<LargeObject> InitLargeObject()
    { 
        var large = new LargeObject(Thread.CurrentThread.ManagedThreadId);
        return large;
    }

    static async void ThreadProc(object state)
    {
        var large = await  lazyLargeObject.GetValueAsync();

        lock (large)
        {
            large.Data[0] = Thread.CurrentThread.ManagedThreadId;
        }
    }

    static AsyncLazy<LargeObject> lazyLargeObject = null;

    [Test]
    public void WithLargObject_getsValueAsync()
    {
        lazyLargeObject = new AsyncLazy<LargeObject>(InitLargeObject);

        // Create and start 3 threads, each of which uses LargeObject.
        Thread[] threads = new Thread[3];
        for (int i = 0; i < 3; i++)
        {
            threads[i] = new Thread(ThreadProc);
            threads[i].Start();
        }
        
        // Wait for all 3 threads to finish.
        foreach (Thread t in threads)
        {
            t.Join();
        }

        Assert.IsTrue(lazyLargeObject.IsValueCreated);
    }
}
