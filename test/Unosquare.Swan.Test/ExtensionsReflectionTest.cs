namespace Unosquare.Swan.Test.ExtensionsReflectionTest
{
    using NUnit.Framework;
    using System.Reflection;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Networking;
    using Mocks;

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
            Assert.AreEqual(expected, input.IsCollection(), $"Get IsCollection value of {input}");
        }

        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullType.IsCollection());
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
            if (input.IsGenericType())
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
    public class IsIEnumerable : TestFixtureBase
    {
        [TestCase(true, typeof(IEnumerable<Fish>))]
        [TestCase(false, typeof(string))]
        public void WithType_ReturnsABool(bool expected, Type input)
        {
            Assert.AreEqual(expected, input.IsIEnumerable(), $"Get IsIEnumerable value of {input}");
        }

        [Test]
        public void WithNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullType.IsIEnumerable());
        }
    }

    [TestFixture]
    public class ToFormattedString
    {
        private readonly PropertyInfoMock _mock = new PropertyInfoMock { BirthDate = new DateTime(2018, 1, 1) };

        [Test]
        public void WithPropertyNullWithoutDefaultValue_ReturnsStringEmpty()
        {
            Assert.AreEqual(
                string.Empty,
                typeof(PropertyInfoMock).GetProperty(nameof(PropertyInfoMock.Name)).ToFormattedString(_mock));
        }

        [Test]
        public void WithPropertyNullWithDefaultValue_ReturnDefaultValue()
        {
            Assert.AreEqual(
                "Unknown",
                typeof(PropertyInfoMock).GetProperty(nameof(PropertyInfoMock.Alias)).ToFormattedString(_mock));
        }

        [Test]
        public void WithIntPropertyNotNullWithFormat_ReturnFormattedValue()
        {
            Assert.AreEqual(
                _mock.Age.ToString("P"),
                typeof(PropertyInfoMock).GetProperty(nameof(PropertyInfoMock.Age)).ToFormattedString(_mock));
        }
        
        [Test]
        public void WithDatePropertyNotNullWithFormat_ReturnFormattedValue()
        {
            Assert.AreEqual(
                _mock.BirthDate.ToString("YYYY"),
                typeof(PropertyInfoMock).GetProperty(nameof(PropertyInfoMock.BirthDate)).ToFormattedString(_mock));
        }
    }

    [TestFixture]
    public class GetAllTypes
    {
        [Test]
        public void WithNullAssembly_ThrowsArgumentNullException()
        {
            Assembly assembly = null;

            Assert.Throws<ArgumentNullException>(() => assembly.GetAllTypes());
        }

        [Test]
        public void WithAssembly_ReturnsTypeObjects()
        {
            var assembly = typeof(string).Assembly();

            var data = assembly.GetAllTypes();

            Assert.AreEqual("System.Type[]", data.ToString());
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

    public abstract class GetCacheMethodInfo : TestFixtureBase
    {
        public PropertyInfo PublicProperty { get; } = typeof(Controller).GetProperty(nameof(Controller.Animal));

        public PropertyInfo NonPublicProperty { get; } = typeof(Controller).GetProperty(nameof(Controller.IsReadonly));
    }

    [TestFixture]
    public class GetCacheGetMethod : GetCacheMethodInfo
    {
        [Test]
        public void PublicPropertyOnlyPublicFlag_ReturnsGetMethodInfo()
        {
            Assert.IsNotNull(PublicProperty.GetCacheGetMethod());
        }

        [Test]
        public void PublicPropertyNoPublicFlag_ReturnsGetMethodInfo()
        {
            Assert.IsNotNull(PublicProperty.GetCacheGetMethod(true));
        }

        [Test]
        public void NonPublicPropertyOnlyPublicFlag_ReturnsGetMethodInfo()
        {
            Assert.IsNull(NonPublicProperty.GetCacheGetMethod());
        }

        [Test]
        public void NonPublicPropertyNoPublicFlag_ReturnsGetMethodInfo()
        {
            Assert.IsNotNull(NonPublicProperty.GetCacheGetMethod(true));
        }

        [Test]
        public void CallTwice_ReturnsCache()
        {
            PublicProperty.GetCacheGetMethod();

            Assert.IsNotNull(PublicProperty.GetCacheGetMethod());
        }
    }

    [TestFixture]
    public class GetCacheSetMethod : GetCacheMethodInfo
    {
        [Test]
        public void PublicPropertyOnlyPublicFlag_ReturnsSetMethodInfo()
        {
            Assert.IsNotNull(PublicProperty.GetCacheSetMethod());
        }

        [Test]
        public void PublicPropertyNoPublicFlag_ReturnsSetMethodInfo()
        {
            Assert.IsNotNull(PublicProperty.GetCacheSetMethod(true));
        }

        [Test]
        public void NonPublicPropertyNoPublicFlag_ReturnsSetMethodInfo()
        {
            Assert.IsNotNull(NonPublicProperty.GetCacheSetMethod(true));
        }
    }
}