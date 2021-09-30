namespace Swan.Test.ExtensionsReflectionTest
{
    using NUnit.Framework;
    using Swan.Reflection;
    using Swan.Test.Mocks;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    [TestFixture]
    public class GetDefault : TestFixtureBase
    {
        [TestCase(null, typeof(Fish))]
        [TestCase(0, typeof(int))]
        public void WithType_ReturnsDefaultValue(object expected, Type input)
        {
            Assert.AreEqual(expected, input.GetDefault(), $"Get default type of {input}");
        }

        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullType.GetDefault());
        }
    }

    [TestFixture]
    public class IsCollection : TestFixtureBase
    {
        [TestCase(true, typeof(List<Fish>))]
        [TestCase(false, typeof(int))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.TypeInfo().IsEnumerable, $"Get IsCollection value of {input}");
        }

        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<NullReferenceException>(() => _ = NullType.GetType().TypeInfo().IsEnumerable);
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
    public class IsIEnumerable : TestFixtureBase
    {
        [TestCase(true, typeof(IEnumerable<Fish>))]
        [TestCase(true, typeof(string))]
        [TestCase(false, typeof(int))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.TypeInfo().IsEnumerable, $"Get IsIEnumerable value of {input}");
        }
    }

    [TestFixture]
    public class GetAllTypes
    {
        [Test]
        public void WithNullAssembly_ThrowsArgumentNullException()
        {
            Assembly? assembly = null;

            Assert.Throws<ArgumentNullException>(() => assembly.GetAllTypes());
        }

        [Test]
        public void WithAssembly_ReturnsTypeObjects()
        {
            var data = typeof(string).Assembly.GetAllTypes();

            Assert.AreEqual("System.RuntimeType[]", data.ToString());
        }
    }

    [TestFixture]
    public class GetMethod : TestFixtureBase
    {
        private readonly string _methodName = nameof(MethodCacheMock.GetMethodTest);
        private readonly Type _type = typeof(MethodCacheMock);
        private readonly Type[] _genericTypes = { typeof(Task<string>) };
        private readonly Type[] _parameterTypes = { typeof(string) };

        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;

        [Test]
        public void WithValidParams_ReturnsAnObject()
        {
            var method = _type.GetMethod(Flags, _methodName, _genericTypes, _parameterTypes);

            Assert.AreEqual(method.ToString(),
                "System.Threading.Tasks.Task`1[System.Threading.Tasks.Task`1[System.String]] GetMethodTest[Task`1](System.String)");
        }

        [Test]
        public void WithNullSourceType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullType.GetMethod(Flags, _methodName, _genericTypes, _parameterTypes));
        }

        [Test]
        public void WithNullMethodName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _type.GetMethod(Flags, null, _genericTypes, _parameterTypes));
        }

        [Test]
        public void WithNullGenericTypes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _type.GetMethod(Flags, _methodName, null, _parameterTypes));
        }

        [Test]
        public void WithNullParameterTypes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _type.GetMethod(Flags, _methodName, _genericTypes, null));
        }
    }
}