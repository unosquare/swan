using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unosquare.Swan;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsReflectionTest
    {
        [TestCase(null, typeof(Fish))]
        [TestCase(0, typeof(int))]
        public void GetDefaultTest(object expected, Type input)
        {            
            Assert.AreEqual(expected, input.GetDefault(), $"Get default type of {input}");
        }

        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(int))]
        public void IsCollectionTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsCollection(), $"Get IsCollection value of {input}");
        }

        [TestCase(true, typeof(Fish))]
        [TestCase(false, typeof(int))]
        public void IsClassTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsClass(), $"Get IsClass value of {input}");
        }

        [TestCase(true, typeof(IAnimal))]
        [TestCase(false, typeof(int))]
        public void IsAbstractTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsAbstract(), $"Get IsAbstract value of {input}");
        }

        [TestCase(true, typeof(IAnimal))]
        [TestCase(false, typeof(int))]
        public void IsInterfaceTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsInterface(), $"Get IsAbstract value of {input}");
        }

        [TestCase(true, typeof(int))]
        [TestCase(false, typeof(string))]
        public void IsPrimitiveTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsPrimitive(), $"Get IsPrimitive value of {input}");
        }

        [TestCase(true, typeof(int))]
        [TestCase(false, typeof(Fish))]
        public void IsValueTypeTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsPrimitive(), $"Get IsValueType value of {input}");
        }

        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(Fish))]
        public void IsGenericTypeTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsGenericType(), $"Get IsGenericType value of {input}");
        }

        [Test]
        public void IsGenericParameterTest()
        {
            var genericArguments = typeof(List<Fish>).GetGenericArguments();
            Assert.AreEqual(false, genericArguments[0].IsGenericParameter(), "Get IsGenericParameter value");
        }

        [TestCase(true, typeof(Clown))]
        [TestCase(false, typeof(Shark))]
        public void IsDefinedTest(bool expected, Type input)
        {
            var members = input.GetMembers();
            Assert.AreEqual(expected, members[0].IsDefined(typeof(AttributeMock), false), $"Get IsDefined value of {input}");
        }

        [TestCase(0, typeof(Clown))]
        [TestCase(1, typeof(Shark))]
        public void GetCustomAttributesTest(int expected, Type input)
        {
            var attributes = input.GetCustomAttributes(typeof(AttributeMock), false);
            Assert.GreaterOrEqual(expected, attributes.Length, $"Get GetCustomAttributes length of {input}");
        }

        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(string))]
        public void IsGenericTypeDefinitionTest(bool expected, Type input)
        {
            if (input.IsGenericType()) input = input.GetGenericTypeDefinition();

            Assert.AreEqual(expected, input.IsGenericTypeDefinition(), $"Get IsGenericTypeDefinition value of {input}");
        }

        [TestCase(typeof(Object), typeof(List<Fish>))]
        [TestCase(typeof(Object), typeof(string))]
        public void BaseTypeTest(Type expected, Type input)
        {
            Assert.AreEqual(expected, input.BaseType(), $"Get BaseType value of {input}");
        }

        [TestCase(true, typeof(IEnumerable<Fish>))]
        [TestCase(false, typeof(string))]
        public void IsIEnumerableTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsIEnumerable(), $"Get IsIEnumerable value of {input}");
        }
    }
}
