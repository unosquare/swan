﻿namespace Unosquare.Swan.Test.TypeCacheTest
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using Reflection;

    public abstract class TypeCacheTest
    {
        protected static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();
    }

    [TestFixture]
    public class Contains : TypeCacheTest
    {
        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TypeCache.Contains(null));
        }

        [Test]
        public void WithInvalidType_ReturnsFalse()
        {
            var contains = TypeCache.Contains<string>();

            Assert.IsFalse(contains);
        }
    }

    [TestFixture]
    public class Retrieve : TypeCacheTest
    {
        [Test]
        public void WithFactoryReturnsNull_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() =>
                TypeCache.Retrieve(typeof(string), () => null));
        }

        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TypeCache.Retrieve(null, () => null));
        }

        [Test]
        public void WithNullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TypeCache.Retrieve(typeof(string), null));
        }
    }
}