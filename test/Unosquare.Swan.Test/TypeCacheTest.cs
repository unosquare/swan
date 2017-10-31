using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unosquare.Swan.Reflection;

namespace Unosquare.Swan.Test.TypeCacheTest
{
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
                TypeCache.Contains(null)
            );
        }

        [Test]
        public void WithInvalidType_ReturnsFalse()
        {
            var arc = TypeCache.Contains<string>();
            
            Assert.IsFalse(arc);
        }
    }

    [TestFixture]
    public class Retrieve : TypeCacheTest
    {
        [Test]
        public void WithFactoryReturnsNull_ThrowsKeyNotFoundException()
        {
            PropertyInfo[] properties;

            Assert.Throws<KeyNotFoundException>(() =>
                properties = TypeCache.Retrieve(typeof(String), () =>
                {
                    return null;
                })
            );
        }

        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TypeCache.Retrieve(null, () => {
                    return null;
                } )
            );
        }

        [Test]
        public void WithNullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TypeCache.Retrieve(typeof(String), null)
            );
        }
    }

    public class GetAllPropertiesFunc : TypeCacheTest
    {
        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                PropertyTypeCache.GetAllPropertiesFunc(null)
            );
        }
    }

    public class GetAllPublicPropertiesFunc : TypeCacheTest
    {
        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                PropertyTypeCache.GetAllPublicPropertiesFunc(null)
            );
        }
    }

    public class GetAllFieldsFunc : TypeCacheTest
    {
        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                FieldTypeCache.GetAllFieldsFunc(null)
            );
        }
    }

}
