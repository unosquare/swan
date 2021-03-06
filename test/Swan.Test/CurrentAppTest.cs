﻿using NUnit.Framework;
using System;

namespace Swan.Test
{
    [TestFixture]
    public class CurrentAppTest
    {
        [Test]
        public void IsSingleInstanceTest()
        {
            Assert.IsTrue(SwanRuntime.IsTheOnlyInstance);
        }

        [Test]
        public void IsUsingMonoRuntimeTest()
        {
            Assert.AreEqual(Type.GetType("Mono.Runtime") != null, SwanRuntime.IsUsingMonoRuntime);
        }

        [Test]
        public void GetLocalStorageTest()
        {
            Assert.IsNotEmpty(SwanRuntime.LocalStoragePath, $"Retrieving a local storage path: {SwanRuntime.LocalStoragePath}");
        }

        [Test]
        public void GetEntryAssemblyDirectoryTest()
        {
            Assert.IsNotNull(SwanRuntime.EntryAssemblyDirectory);
        }
    }
}