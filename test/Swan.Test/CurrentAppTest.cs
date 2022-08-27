namespace Swan.Test;

using NUnit.Framework;
using Platform;

[TestFixture]
public class CurrentAppTest
{
    [Test]
    public void IsSingleInstanceTest() => Assert.IsTrue(SwanRuntime.IsTheOnlyInstance);

    [Test]
    public void GetLocalStorageTest() => Assert.IsNotEmpty(SwanRuntime.LocalStoragePath, $"Retrieving a local storage path: {SwanRuntime.LocalStoragePath}");

    [Test]
    public void GetEntryAssemblyDirectoryTest() 
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Windows only test");

        Assert.IsNotNull(SwanRuntime.EntryAssemblyDirectory);
    }
}
