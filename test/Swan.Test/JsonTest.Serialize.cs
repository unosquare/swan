namespace Swan.Test;

using Formatters;
using Mocks;
using System.Text.Json;

[TestFixture]
public class ToJson : JsonTest
{
    [Test]
    public void CheckJsonFormat_ValidatesIfObjectsAreEqual() => Assert.AreEqual(BasicStr, BasicJson.GetDefault().JsonSerialize());

    [Test]
    public void NullObjectAndEmptyString_ValidatesIfTheyAreEquals() => Assert.AreEqual(string.Empty, NullObj.JsonSerialize());
}

[TestFixture]
public class Serialize : JsonTest
{
    [Test]
    public void WithStringArray_ReturnsArraySerialized() => Assert.AreEqual(BasicAStr, DefaultStringList.JsonSerialize());

    [Test]
    public void WithStringsArrayAndWeakReference_ReturnsArraySerialized()
    {
        var instance = BasicJson.GetDefault();
        var reference = new List<WeakReference> { new(instance) };
        var data = reference.JsonSerialize();

        var originalObject = SampleFamily.Create(false);
        var originalObjectJson = originalObject.JsonSerialize(true);

        var typedObject = originalObjectJson.JsonDeserialize<SampleFamily>();
        var typedObjectJson = typedObject.JsonSerialize(true);

        var dynamicObject = originalObjectJson.JsonDeserialize();
        var dynamicObjectJson = (dynamicObject as object)?.JsonSerialize(true);

        var dictionary = dynamicObject as IDictionary<string, object?>;

        Assert.IsTrue(originalObject.Members.First().Value.Id == typedObject?.Members.First().Value.Id);
        Assert.IsTrue(originalObject.Members.First().Value.Id == dynamicObject?.Members.Dad.Id);
        Assert.IsTrue(originalObjectJson == typedObjectJson);
        Assert.IsTrue(originalObjectJson == dynamicObjectJson);
        // Assert.IsTrue(data.StartsWith("{ \"$circref\":"));
    }

    [Test]
    public void WithNumericArray_ReturnsArraySerialized() => Assert.AreEqual(NumericAStr, NumericArray.JsonSerialize());

    [Test]
    public void WithBasicObjectWithArray_ReturnsObjectWithArraySerialized() => Assert.AreEqual(BasicAObjStr, BasicAObj.JsonSerialize());

    [Test]
    public void WithArrayOfObjects_ReturnsArrayOfObjectsSerialized()
    {
        var data = (new List<BasicJson>
        {
            BasicJson.GetDefault(),
            BasicJson.GetDefault(),
        }).JsonSerialize();

        Assert.IsNotNull(data);
        Assert.AreEqual($"[{BasicStr},{BasicStr}]", data);
    }

    [Test]
    public void AdvObject_ReturnsAdvObjectSerialized() => Assert.AreEqual(AdvStr, AdvObj.JsonSerialize());

    [Test]
    public void AdvObjectArray_ReturnsAdvObjectArraySerialized() => Assert.AreEqual(AdvAStr, AdvAObj.JsonSerialize());

    [TestCase("1", 1)]
    [TestCase("1", 1F)]
    [TestCase("\"string\"", "string")]
    [TestCase("true", true)]
    [TestCase("false", false)]
    [TestCase("", null)]
    [TestCase("", default)]
    public void WithPrimitive_ReturnsStringValue(string expected, object actual) => Assert.AreEqual(expected, actual.JsonSerialize());

    [Test]
    public void WithDateTest_ReturnsDateTestSerialized()
    {
        var obj = new DateTimeJson { Date = new DateTime(2010, 1, 1) };
        var data = obj.JsonSerialize();

        Assert.IsNotNull(data);
        Assert.AreEqual(
            $"{{\"Date\":\"{obj.Date.Value:s}\"}}",
            data,
            "Date must be formatted as ISO");

        var dict = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(data);

        Assert.IsNotNull(dict);
        Assert.IsTrue(dict.ContainsKey("Date"));
        Assert.AreEqual(obj.Date, dict["Date"]);

        var objDeserialized = JsonSerializer.Deserialize<DateTimeJson>(data);

        Assert.IsNotNull(objDeserialized);
        Assert.AreEqual(obj.Date, objDeserialized.Date);
    }

    [Test]
    public void WithStructureArray_ReturnsStructureArraySerialized()
    {
        var result = new[] { new SampleStruct { Value = 1, Name = "A" }, new SampleStruct { Value = 2, Name = "B" } };
        var data = result.JsonSerialize();
        Assert.AreEqual(ArrayStruct, data);
    }

    [Test]
    public void WithEmptyClass_ReturnsEmptyClassSerialized() => Assert.AreEqual("{}", (new EmptyJson()).JsonSerialize());

    [Test]
    public void WithStructure_ReturnsStructureSerialized() => Assert.AreEqual("{\"Value\":1,\"Name\":\"DefaultStruct\"}", DefaultStruct.JsonSerialize());

    [Test]
    public void WithObjEnumString_ReturnsObjectSerialized() => Assert.AreEqual("{\"Id\":0,\"MyEnum\":3}", (new ObjectEnum()).JsonSerialize());
}

[TestFixture]
public class SerializeOnly : JsonTest
{
    [Test]
    public void WithObject_ReturnsObjectSerialized()
    {
        var dataSerialized = BasicJson.GetDefault().JsonSerialize();

        Assert.AreEqual(
            $"{{{BasicJson.GetControlValue()}}}",
            dataSerialized);
    }

    [Test]
    public void WithString_ReturnsString() => Assert.AreEqual("\"\\b\\t\\f\\u0000\"", "\b\t\f\0".JsonSerialize());

    [Test]
    public void WithEmptyString_ReturnsEmptyString() => Assert.AreEqual("\"\"", string.Empty.JsonSerialize());

    [Test]
    public void WithType_SerializingTypeThrows() => Assert.Throws<InvalidOperationException>(() => typeof(string).JsonSerialize());

    [Test]
    public void WithEmptyEnumerable_ReturnsEmptyArrayLiteral()
    {
        var emptyEnumerable = Enumerable.Empty<int>();

        var dataSerialized = emptyEnumerable.JsonSerialize();

        Assert.AreEqual("[]", dataSerialized);
    }

    [Test]
    public void WithEmptyDictionary_ReturnsEmptyObjectLiteral()
    {
        var dataSerialized = new Dictionary<string, string>().JsonSerialize();

        Assert.AreEqual("{}", dataSerialized);
    }

    [Test]
    public void WithDictionaryOfDictionaries_ReturnsString()
    {
        var persons = new Dictionary<string, Dictionary<int, string>>
        {
            {"A", new Dictionary<int, string>()},
            {"B", DefaultDictionary },
        };

        var dataSerialized = persons.JsonSerialize();

        Assert.AreEqual("{\"A\":{},\"B\":{\"1\":\"A\",\"2\":\"B\",\"3\":\"C\",\"4\":\"D\",\"5\":\"E\"}}",
            dataSerialized);
    }

    [Test]
    public void WithDictionaryOfArrays_ReturnsString()
    {
        var wordDictionary =
            new Dictionary<string, string[][]>
            {
                {"A", new[] {new string[] { }, DefaultStringList.ToArray() }},
            };

        var dataSerialized = wordDictionary.JsonSerialize();

        Assert.AreEqual("{\"A\":[[],[\"A\",\"B\",\"C\"]]}", dataSerialized);
    }
}

[TestFixture]
public class SerializeExcluding : JsonTest
{
    [Test]
    public void WithJsonProperty_ReturnsObjectSerializedExcludingProps()
    {
        var dataSerialized = (new JsonPropertySample { Data = "Data", IgnoredData = "Ignored" }).JsonSerialize();

        Assert.AreEqual(
            "{\"data\":\"Data\"}",
            dataSerialized);
    }

    [Test]
    public void WithInnerJsonProperty_ReturnsObjectSerializedExcludingProps()
    {
        var dataSerialized = (new InnerJsonPropertySample
        {
            Data = "Data",
            IgnoredData = "Ignored",
            Inner = new() { Data = "Data", IgnoredData = "Ignored" },
        }).JsonSerialize();

        Assert.AreEqual(
            "{\"data\":\"Data\",\"Inner\":{\"data\":\"Data\"}}",
            dataSerialized);
    }

    [Test]
    public void WithInnerJsonProperty_ReturnsObjectSerializedWithoutExcludingRepeatedProps()
    {
        var data = new JsonIngorePropertySample
        {
            Id = "22332",
            Name = "Yeyo",
            Inner = new()
            {
                Id = "AESD",
                Data = 44,
            },
        };

        var dataSerialized = data.JsonSerialize();

        Assert.AreEqual(
            "{\"name\":\"Yeyo\",\"inner\":{\"id\":\"AESD\",\"data\":44}}",
            dataSerialized);
    }
}
