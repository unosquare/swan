namespace Swan.Test.ExtensionsReflectionTest;

using NUnit.Framework;
using Reflection;
using Mocks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

public enum FirstEnum
{
    One,
    Two,
    Three
}

public enum SecondEnum
{
    Eleven,
    Twelve,
    Thirteen
}

public class ParseCompatible
{
    public double Value { get; set; }

    public static ParseCompatible Parse(string input, IFormatProvider format)
    {
        if (typeof(double).TypeInfo().TryParse(input, out var innerValue))
            return new() { Value = (double)innerValue! };

        throw new FormatException("Bad double format");
    }
}

public class TryParseCompatible
{
    public double Value { get; set; }

    public static bool TryParse(string input, IFormatProvider format, out TryParseCompatible? value)
    {
        value = default;
        if (!typeof(double).TypeInfo().TryParse(input, out var innerValue))
            return false;

        value = new() { Value = (double)innerValue };
        return true;

    }
}

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
public class TryParse
{
    [Test]
    public void TryParseSucceeds()
    {
        const string numericStringValue = "4.68e1";

        var result = typeof(bool).TypeInfo().TryParse(numericStringValue, out var v);
        Assert.IsTrue(result && (bool)v!);

        result = typeof(bool?).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && (bool)v!);

        result = typeof(byte).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && (byte)v! == 47);

        result = typeof(byte?).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && (byte)v! == 47);

        result = typeof(sbyte).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && (sbyte)v! == 47);

        result = typeof(sbyte?).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && (sbyte?)v! == 47);

        result = typeof(double).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && (double)v! == 46.8d);

        result = typeof(decimal?).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && (decimal)v! == 46.8M);

        result = typeof(float?).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && (float?)v! == 46.8f);

        result = typeof(IPAddress).TypeInfo().TryParse("192.168.1.1", out v);
        Assert.IsTrue(result && ((IPAddress)v!).ToString() == "192.168.1.1");

        result = typeof(TryParseCompatible).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && ((TryParseCompatible)v).Value == 46.8d);

        result = typeof(ParseCompatible).TypeInfo().TryParse(numericStringValue, out v);
        Assert.IsTrue(result && ((ParseCompatible)v).Value == 46.8d);
    }

    [Test]
    public void TryChangeTypeSucceeds()
    {
        var result = TypeManager.TryChangeType("1", typeof(SecondEnum?), out var v);
        Assert.IsTrue(result && (SecondEnum)v! == SecondEnum.Twelve);

        result = TypeManager.TryChangeType("1", typeof(SecondEnum), out v);
        Assert.IsTrue(result && (SecondEnum)v! == SecondEnum.Twelve);

        result = TypeManager.TryChangeType(1, typeof(SecondEnum?), out v);
        Assert.IsTrue(result && (SecondEnum)v! == SecondEnum.Twelve);

        result = TypeManager.TryChangeType(1, typeof(SecondEnum), out v);
        Assert.IsTrue(result && (SecondEnum)v! == SecondEnum.Twelve);

        result = TypeManager.TryChangeType("twelve", typeof(SecondEnum?), out v);
        Assert.IsTrue(result && (SecondEnum)v! == SecondEnum.Twelve);

        result = TypeManager.TryChangeType("twelve", typeof(SecondEnum), out v);
        Assert.IsTrue(result && (SecondEnum)v! == SecondEnum.Twelve);

        result = TypeManager.TryChangeType((FirstEnum?)FirstEnum.Two, typeof(SecondEnum?), out v);
        Assert.IsTrue(result && (SecondEnum)v! == SecondEnum.Twelve);

        result = TypeManager.TryChangeType(FirstEnum.Two, typeof(SecondEnum), out v);
        Assert.IsTrue(result && (SecondEnum)v! == SecondEnum.Twelve);

        result = TypeManager.TryChangeType(3.1416M, typeof(double), out v);
        Assert.IsTrue(result && (double)v! == 3.1416d);

        result = TypeManager.TryChangeType(null, typeof(double?), out v);
        Assert.IsTrue(result && (double?)v is null);
    }
}

[TestFixture]
public class GetMethod : TestFixtureBase
{
    private const string MethodName = nameof(MethodCacheMock.GetMethodTest);
    private readonly Type _type = typeof(MethodCacheMock);
    private readonly Type[] _genericTypes = { typeof(Task<string>) };
    private readonly Type[] _parameterTypes = { typeof(string) };

    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;

    [Test]
    public void WithValidParams_ReturnsAnObject()
    {
        var method = _type.GetMethod(Flags, MethodName, _genericTypes, _parameterTypes);

        Assert.AreEqual(method.ToString(),
            "System.Threading.Tasks.Task`1[System.Threading.Tasks.Task`1[System.String]] GetMethodTest[Task`1](System.String)");
    }

    [Test]
    public void WithNullSourceType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            NullType.GetMethod(Flags, MethodName, _genericTypes, _parameterTypes));
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
            _type.GetMethod(Flags, MethodName, null, _parameterTypes));
    }

    [Test]
    public void WithNullParameterTypes_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _type.GetMethod(Flags, MethodName, _genericTypes, null));
    }
}
