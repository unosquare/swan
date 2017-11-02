using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.ExtensionsReflectionTest
{
    [TestFixture]
    public class GetDefault
    {
        [TestCase(null, typeof(Fish))]
        [TestCase(0, typeof(int))]
        public void WithType_ReturnsDefaultValue(object expected, Type input)
        {
            Assert.AreEqual(expected, input.GetDefault(), $"Get default type of {input}");
        }
    }

    [TestFixture]
    public class IsCollection
    {
        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(int))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsCollection(), $"Get IsCollection value of {input}");
        }
    }

    [TestFixture]
    public class IsClass
    {
        [TestCase(true, typeof(Fish))]
        [TestCase(false, typeof(int))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsClass(), $"Get IsClass value of {input}");
        }
    }

    [TestFixture]
    public class IsAbstract
    {
        [TestCase(true, typeof(IAnimal))]
        [TestCase(false, typeof(int))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsAbstract(), $"Get IsAbstract value of {input}");
        }
    }

    [TestFixture]
    public class IsInterface
    {
        [TestCase(true, typeof(IAnimal))]
        [TestCase(false, typeof(int))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsInterface(), $"Get IsAbstract value of {input}");
        }
    }

    [TestFixture]
    public class IsPrimitive
    {
        [TestCase(true, typeof(int))]
        [TestCase(false, typeof(string))]
        [TestCase(false, typeof(Fish))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsPrimitive(), $"Get IsPrimitive value of {input}");
        }
    }

    [TestFixture]
    public class IsGenericType
    {
        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(Fish))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsGenericType(), $"Get IsGenericType value of {input}");
        }
    }

    [TestFixture]
    public class IsGenericParameter
    {
        [Test]
        public void WithType_ReturnsABool()
        {
            var genericArguments = typeof(List<Fish>).GetGenericArguments();
            Assert.AreEqual(false, genericArguments[0].IsGenericParameter(), "Get IsGenericParameter value");
        }
    }

    [TestFixture]
    public class IsDefined
    {
        [TestCase(true, typeof(Clown))]
        [TestCase(false, typeof(Shark))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            var members = input.GetMembers();
            Assert.AreEqual(expected, members[0].IsDefined(typeof(AttributeMock), false), $"Get IsDefined value of {input}");
        }
    }

    [TestFixture]
    public class GetCustomAttributes
    {
        [TestCase(0, typeof(Clown))]
        [TestCase(1, typeof(Shark))]
        public void WithType_ReturnsCustomAttributes(int expected, Type input)
        {
            var attributes = input.GetCustomAttributes(typeof(AttributeMock), false);
            
            Assert.GreaterOrEqual(expected, attributes.Length, $"Get GetCustomAttributes length of {input}");
        }
    }

    [TestFixture]
    public class IsGenericTypeDefinition
    {
        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(string))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            if(input.IsGenericType())
                input = input.GetGenericTypeDefinition();

            Assert.AreEqual(expected, input.IsGenericTypeDefinition(), $"Get IsGenericTypeDefinition value of {input}");
        }
    }

    [TestFixture]
    public class BaseType
    {
        [TestCase(typeof(object), typeof(List<Fish>))]
        [TestCase(typeof(object), typeof(string))]
        public void WithType_ReturnsBaseType(Type expected, Type input)
        {
            Assert.AreEqual(expected, input.BaseType(), $"Get BaseType value of {input}");
        }
    }

    [TestFixture]
    public class IsIEnumerable
    {
        [TestCase(true, typeof(IEnumerable<Fish>))]
        [TestCase(false, typeof(string))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsIEnumerable(), $"Get IsIEnumerable value of {input}");
        }
    }

    [TestFixture]
    public class GetAllTypes
    {
        public void WithNullAssembly_ThrowsArgumentNullException()
        {
            Assembly assembly = null;

            Assert.Throws<IgnoreException>(() =>
                assembly.GetAllTypes()
            );
        }
    }
}
