using NUnit.Framework;
using Swan.Core.Extensions;
using Swan.Extensions;
using Swan.Test.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Swan.Test.JsonTests
{
    [TestFixture]
    public class ToJson : JsonTest
    {
        [Test]
        public void CheckJsonFormat_ValidatesIfObjectsAreEqual()
        {
            Assert.AreEqual(BasicStr, BasicJson.GetDefault().ToJson(false));
        }

        [Test]
        public void CheckJsonFormat_ValidatesIfObjectsAreNotEqual()
        {
            Assert.AreNotEqual(BasicStr, BasicJson.GetDefault().ToJson());
        }

        [Test]
        public void NullObjectAndEmptyString_ValidatesIfTheyAreEquals()
        {
            Assert.AreEqual(string.Empty, NullObj.ToJson());
        }
    }

    [TestFixture]
    public class Serialize : JsonTest
    {
        [Test]
        public void WithStringArray_ReturnsArraySerialized()
        {
            Assert.AreEqual(BasicAStr, DefaultStringList.ToJson());
        }

        [Test]
        public void WithStringsArrayAndWeakReference_ReturnsArraySerialized()
        {
            var instance = BasicJson.GetDefault();
            var reference = new List<WeakReference> { new(instance) };

            var data = Json.Serialize(instance, false, null, false, null, null, reference, JsonSerializerCase.None);

            Assert.IsTrue(data.StartsWith("{ \"$circref\":"));
        }

        [Test]
        public void WithNumericArray_ReturnsArraySerialized()
        {
            Assert.AreEqual(NumericAStr, NumericArray.ToJson());
        }

        [Test]
        public void WithBasicObjectWithArray_ReturnsObjectWithArraySerialized()
        {
            Assert.AreEqual(BasicAObjStr, Json.Serialize(BasicAObj));
        }

        [Test]
        public void WithArrayOfObjects_ReturnsArrayOfObjectsSerialized()
        {
            var data = (new List<BasicJson>
            {
                BasicJson.GetDefault(),
                BasicJson.GetDefault(),
            }).ToJson();

            Assert.IsNotNull(data);
            Assert.AreEqual($"[{BasicStr},{BasicStr}]", data);
        }

        [Test]
        public void AdvObject_ReturnsAdvObjectSerialized()
        {
            Assert.AreEqual(AdvStr, AdvObj.ToJson());
        }

        [Test]
        public void AdvObjectArray_ReturnsAdvObjectArraySerialized()
        {
            Assert.AreEqual(AdvAStr, AdvAObj.ToJson());
        }

        [TestCase("1", 1)]
        [TestCase("1", 1F)]
        [TestCase("\"string\"", "string")]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("null", null)]
        [TestCase("null", default)]
        public void WithPrimitive_ReturnsStringValue(string expected, object actual)
        {
            Assert.AreEqual(expected, actual.ToJson());
        }

        [Test]
        public void WithDateTest_ReturnsDateTestSerialized()
        {
            var obj = new DateTimeJson { Date = new DateTime(2010, 1, 1) };
            var data = obj.ToJson();

            Assert.IsNotNull(data);
            Assert.AreEqual(
                $"{{\"Date\": \"{obj.Date.Value:s}\"}}",
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

            Assert.AreEqual(ArrayStruct, result.ToJson());
        }

        [Test]
        public void WithEmptyClass_ReturnsEmptyClassSerialized()
        {
            Assert.AreEqual("{ }", (new EmptyJson()).ToJson());
        }

        [Test]
        public void WithStructure_ReturnsStructureSerialized()
        {
            Assert.AreEqual("{\"Value\": 1,\"Name\": \"DefaultStruct\"}", DefaultStruct.ToJson());
        }

        [Test]
        public void WithObjEnumString_ReturnsObjectSerialized()
        {
            Assert.AreEqual("{\"Id\": 0,\"MyEnum\": 3}", (new ObjectEnum()).ToJson());
        }
    }

    [TestFixture]
    public class SerializeOnly : JsonTest
    {
        [Test]
        public void WithObject_ReturnsObjectSerialized()
        {
            var includeNames = new[]
            {
                nameof(BasicJson.StringData),
                nameof(BasicJson.IntData),
                nameof(BasicJson.NegativeInt),
            };

            var dataSerialized = Json.SerializeOnly(BasicJson.GetDefault(), false, includeNames);

            Assert.AreEqual(
                "{\"StringData\": \"string,\\r\\ndata\\\\\",\"IntData\": 1,\"NegativeInt\": -1}",
                dataSerialized);
        }

        [Test]
        public void WithString_ReturnsString()
        {
            Assert.AreEqual("\"\\b\\t\\f\\u0000\"", Json.SerializeOnly("\b\t\f\0", true, null));
        }

        [Test]
        public void WithEmptyString_ReturnsEmptyString()
        {
            var dataSerialized = string.Empty.ToJson();

            Assert.AreEqual("\"\"", dataSerialized);
        }

        [Test]
        public void WithType_ReturnsString()
        {
            var dataSerialized = typeof(string).ToJson();

            Assert.AreEqual("\"System.String\"", dataSerialized);
        }

        [Test]
        public void WithEmptyEnumerable_ReturnsEmptyArrayLiteral()
        {
            var emptyEnumerable = Enumerable.Empty<int>();

            var dataSerialized = emptyEnumerable.ToJson();

            Assert.AreEqual("[ ]", dataSerialized);
        }

        [Test]
        public void WithEmptyDictionary_ReturnsEmptyObjectLiteral()
        {
            var dataSerialized = Json.SerializeOnly(new Dictionary<string, string>(), true, null);

            Assert.AreEqual("{ }", dataSerialized);
        }

        [Test]
        public void WithDictionaryOfDictionaries_ReturnsString()
        {
            var persons = new Dictionary<string, Dictionary<int, string>>
            {
                {"A", new Dictionary<int, string>()},
                {"B", DefaultDictionary },
            };

            var dataSerialized = Json.SerializeOnly(persons, false, null);

            Assert.AreEqual("{\"A\": { },\"B\": {\"1\": \"A\",\"2\": \"B\",\"3\": \"C\",\"4\": \"D\",\"5\": \"E\"}}",
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

            var dataSerialized = wordDictionary.ToJson();

            Assert.AreEqual("{\"A\": [[ ],[\"A\",\"B\",\"C\"]]}", dataSerialized);
        }
    }

    [TestFixture]
    public class SerializeExcluding : JsonTest
    {
        [Test]
        public void WithObject_ReturnsObjectSerializedExcludingProps()
        {
            var excludeNames = new[]
            {
                nameof(BasicJson.StringData),
                nameof(BasicJson.IntData),
                nameof(BasicJson.NegativeInt),
            };

            var dataSerialized = Json.SerializeExcluding(BasicJson.GetDefault(), false, excludeNames);

            Assert.AreEqual(
                "{\"DecimalData\": 10.33,\"BoolData\": true,\"StringNull\": null}",
                dataSerialized);
        }

        [Test]
        public void WithJsonProperty_ReturnsObjectSerializedExcludingProps()
        {
            var dataSerialized = (new JsonPropertySample() { Data = "Data", IgnoredData = "Ignored" }).ToJson();

            Assert.AreEqual(
                "{\"data\": \"Data\"}",
                dataSerialized);
        }

        [Test]
        public void WithInnerJsonProperty_ReturnsObjectSerializedExcludingProps()
        {
            var dataSerialized = Json.Serialize(new InnerJsonPropertySample()
            {
                Data = "Data",
                IgnoredData = "Ignored",
                Inner = new JsonPropertySample() { Data = "Data", IgnoredData = "Ignored" },
            });

            Assert.AreEqual(
                "{\"data\": \"Data\",\"Inner\": {\"data\": \"Data\"}}",
                dataSerialized);
        }

        [Test]
        public void WithInnerJsonProperty_ReturnsObjectSerializedWithoutExcludingRepeatedProps()
        {
            var data = new JsonIngorePropertySample
            {
                Id = "22332",
                Name = "Yeyo",
                Inner = new JsonIngoreNestedPropertySample
                {
                    Id = "AESD",
                    Data = 44,
                },
            };

            var dataSerialized = Json.Serialize(data);

            Assert.AreEqual(
                "{\"name\": \"Yeyo\",\"inner\": {\"id\": \"AESD\",\"data\": 44}}",
                dataSerialized);
        }

        [Test]
        public void WithInnerJsonProperty_ReturnsObjectSerializedWithNestedExcluededNames()
        {
            var data = new JsonIngorePropertySample
            {
                Id = "22332",
                Name = "Yeyo",
                Inner = new JsonIngoreNestedPropertySample
                {
                    Id = "AESD",
                    Data = 44,
                },
            };

            var dataSerialized = Json.SerializeExcluding(data, false, nameof(JsonIngoreNestedPropertySample.Data));

            Assert.AreEqual(
                "{\"name\": \"Yeyo\",\"inner\": {\"id\": \"AESD\"}}",
                dataSerialized);
        }
    }
}