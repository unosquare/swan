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
            Assert.IsTrue(Runtime.IsTheOnlyInstance);
        }

        [Test]
        public void IsOsDifferentUnknownTest()
        {
            Assert.AreNotEqual(Runtime.OS, OperatingSystem.Unknown, $"Retrieving a OS: {Runtime.OS}");
        }

        [Test]
        public void IsUsingMonoRuntimeTest()
        {
            Assert.AreEqual(Type.GetType("Mono.Runtime") != null, Runtime.IsUsingMonoRuntime);
        }
        
        [Test]
        public void GetAssemblyAttributesTest()
        {
            Assert.Ignore("Rewrite this");

            Assert.AreEqual("NUnit Software", Runtime.CompanyName);
            Assert.AreEqual("dotnet_test_nunit", Runtime.ProductName);
            Assert.AreEqual("NUnit is a trademark of NUnit Software", Runtime.ProductTrademark);
        }
        
        [Test]
        public void GetLocalStorageTest()
        {
            Assert.IsNotEmpty(Runtime.LocalStoragePath, $"Retrieving a local storage path: {Runtime.LocalStoragePath}");
        }

        [Test]
        public void GetProcessTest()
        {
            Assert.Ignore("Rewrite this");

            Assert.IsNotNull(Runtime.Process);
            Assert.AreEqual(Runtime.Process.ProcessName,
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
            Assert.Ignore("Rewrite this");
            Assert.IsNotNull(Runtime.EntryAssembly);
            Assert.IsTrue(Runtime.EntryAssembly.FullName.StartsWith("testhost"));
        }

        [Test]
        public void GetEntryAssemblyDirectoryTest()
        {
            Assert.IsNotNull(Runtime.EntryAssemblyDirectory);
        }
    }
}
