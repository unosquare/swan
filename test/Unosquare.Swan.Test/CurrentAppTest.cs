using NUnit.Framework;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class CurrentAppTest
    {
        [Test]
        public void IsSingleInstanceTest()
        {
            Assert.IsTrue(CurrentApp.IsSingleInstance);
        }

        [Test]
        public void IsOsDifferentUnknownTest()
        {
            Assert.AreNotEqual(CurrentApp.OS, Os.Unknown, $"Retrieving a OS: {CurrentApp.OS}");
        }

        [Test]
        public void GetLocalStorageTest()
        {
            Assert.IsNotEmpty(CurrentApp.LocalStoragePath, $"Retrieving a local storage path: {CurrentApp.LocalStoragePath}");
        }

        [Test]
        public void GetProcessTest()
        {
            Assert.IsNotNull(CurrentApp.Process);
            Assert.AreEqual(CurrentApp.Process.ProcessName,
#if NET452
                "dotnet-test-nunit"
#else
                "dotnet"
#endif
                );
        }
        
        [Test]
        public void GetEntryAssemblyTest()
        {
            Assert.IsNotNull(CurrentApp.EntryAssembly);
            Assert.IsTrue(CurrentApp.EntryAssembly.FullName.StartsWith("dotnet-test-nunit"));
        }

        [Test]
        public void GetEntryAssemblyDirectoryTest()
        {
            Assert.IsNotNull(CurrentApp.EntryAssemblyDirectory);
            // TODO: What else?
        }
    }
}
