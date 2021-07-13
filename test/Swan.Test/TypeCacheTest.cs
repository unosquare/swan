using NUnit.Framework;
using Swan.Reflection;
using System;

namespace Swan.Test.TypeCacheTest
{
    public abstract class TypeCacheTest
    {
        protected static readonly PropertyTypeCache TypeCache = new();
    }

    [TestFixture]
    public class Contains : TypeCacheTest
    {
        [Test]
        public void WithInvalidType_ReturnsFalse()
        {
            Assert.IsFalse(TypeCache.Contains<string>());
        }
    }

    [TestFixture]
    public class Retrieve : TypeCacheTest
    {
        [Test]
        public void WithFactoryReturnsNull_ThrowsKeyNotFoundException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TypeCache.Retrieve(typeof(string), t => null));
        }

        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TypeCache.Retrieve(null, t => null));
        }

        [Test]
        public void WithNullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TypeCache.Retrieve(typeof(string), null));
        }
    }
}