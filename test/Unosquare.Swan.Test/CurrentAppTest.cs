using System;
using NUnit.Framework;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class CurrentAppTest
    {
        [Test]
        public void IsSingleInstanceTest()
        {
            Assert.IsTrue(CurrentApp.IsTheOnlyInstance);
        }

        [Test]
        public void IsOsDifferentUnknownTest()
        {
            Assert.AreNotEqual(CurrentApp.OS, OperatingSystem.Unknown, $"Retrieving a OS: {CurrentApp.OS}");
        }

        [Test]
        public void IsUsingMonoRuntimeTest()
        {
            Assert.AreEqual(Type.GetType("Mono.Runtime") != null, CurrentApp.IsUsingMonoRuntime);
        }
        
        [Test]
        public void GetAssemblyAttributesTest()
        {
            Assert.AreEqual("NUnit Software", CurrentApp.CompanyName);
            Assert.AreEqual("dotnet_test_nunit", CurrentApp.ProductName);
            Assert.AreEqual("NUnit is a trademark of NUnit Software", CurrentApp.ProductTrademark);
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
