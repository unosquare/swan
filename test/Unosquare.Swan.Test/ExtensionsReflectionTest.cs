using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
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
            Assert.AreEqual(expected, input.GetDefault(), "Get default of type");
        }

        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(int))]
        public void IsCollectionTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsCollection(), "Get IsCollection value");
        }

        [TestCase(true, typeof(Fish))]
        [TestCase(false, typeof(int))]
        public void IsClassTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsClass(), "Get IsClass value");
        }

        [TestCase(true, typeof(IAnimal))]
        [TestCase(false, typeof(int))]
        public void IsAbstractTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsAbstract(), "Get IsAbstract value");
        }

        [TestCase(true, typeof(IAnimal))]
        [TestCase(false, typeof(int))]
        public void IsInterfaceTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsInterface(), "Get IsAbstract value");
        }

        [TestCase(true, typeof(int))]
        [TestCase(false, typeof(string))]
        public void IsPrimitiveTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsPrimitive(), "Get IsPrimitive value");
        }

        [TestCase(true, typeof(int))]
        [TestCase(false, typeof(Fish))]
        public void IsValueTypeTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsPrimitive(), "Get IsValueType value");
        }

        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(Fish))]
        public void IsGenericTypeTest(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsGenericType(), "Get IsGenericType value");
        }
    }
}
