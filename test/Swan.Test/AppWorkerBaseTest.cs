namespace Swan.Test;

using Mocks;
using NUnit.Framework;
using Threading;

[TestFixture]
public class AppWorkerBaseTest
{
    [Test]
    public async Task CanStartAndStopTest()
    {
        var mock = new AppWorkerMock();
        var exit = false;
        mock.OnExit = () => exit = true;
        Assert.AreEqual(WorkerState.Created, mock.WorkerState);
        await mock.StartAsync();
        Assert.AreEqual(WorkerState.Waiting, mock.WorkerState);
        await mock.StopAsync();
        Assert.AreEqual(WorkerState.Stopped, mock.WorkerState);

        Assert.IsTrue(mock.ExitBecauseCancellation, "Exit because cancellation");
        Assert.IsTrue(exit, "Exit event was fired");
    }

    [Test]
    public async Task WorkingTest()
    {
        var mock = new AppWorkerMock();
        await mock.StartAsync();

        // Mock increase count by one every 100 ms, wait a little bit
        await Task.Delay(TimeSpan.FromSeconds(1));
        Assert.GreaterOrEqual(mock.Count, 5);
    }

    [Test]
    public async Task AppWorkerExceptionTest()
    {
        var mock = new AppWorkerMock();
        await mock.StartAsync();

        // Mock increase count by one every 100 ms, wait a little bit
        await Task.Delay(TimeSpan.FromSeconds(2));

        Assert.AreEqual(WorkerState.Waiting, mock.WorkerState);
        Assert.IsFalse(mock.ExitBecauseCancellation, "The AppWorker doesn't exit because cancellation");
        Assert.IsNotNull(mock.Exception, "The AppWorker had an exception");
    }
}
