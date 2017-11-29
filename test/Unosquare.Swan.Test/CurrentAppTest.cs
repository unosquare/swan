namespace Unosquare.Swan.Test
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class CurrentAppTest
    {
        [Test]
        public void IsSingleInstanceTest()
        {
            Assert.IsTrue(Runtime.IsTheOnlyInstance);
        }

        [Test]
        public void IsOsDifferentUnknownTest()
        {
            Assert.AreNotEqual(Runtime.OS, Swan.OperatingSystem.Unknown, $"Retrieving a OS: {Runtime.OS}");
        }

        [Test]
        public void IsUsingMonoRuntimeTest()
        {
            Assert.AreEqual(Type.GetType("Mono.Runtime") != null, Runtime.IsUsingMonoRuntime);
        }
        
        [Test]
        public void GetLocalStorageTest()
        {
            Assert.IsNotEmpty(Runtime.LocalStoragePath, $"Retrieving a local storage path: {Runtime.LocalStoragePath}");
        }
        
        [Test]
        public void GetEntryAssemblyDirectoryTest()
        {
            Assert.IsNotNull(Runtime.EntryAssemblyDirectory);
        }
    }
}
