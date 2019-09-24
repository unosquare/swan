namespace Swan.Test.JsonTests
{
    using System.Linq;
    using Formatters;
    using Mocks;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

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
            Assert.AreEqual(BasicAStr, Json.Serialize(DefaultStringList));
        }

        [Test]
        public void WithStringsArrayAndWeakReference_ReturnsArraySerialized()
        {
            var instance = BasicJson.GetDefault();
            var reference = new List<WeakReference> {new WeakReference(instance)};

            var data = Json.Serialize(instance, false, null, false, null, null, reference, JsonSerializerCase.None);

            Assert.IsTrue(data.StartsWith("{ \"$circref\":"));
        }

        [Test]
        public void WithNumericArray_ReturnsArraySerialized()
        {
            Assert.AreEqual(NumericAStr, Json.Serialize(NumericArray));
        }

        [Test]
        public void WithBasicObjectWithArray_ReturnsObjectWithArraySerialized()
        {
            Assert.AreEqual(BasicAObjStr, Json.Serialize(BasicAObj));
        }

        [Test]
        public void WithArrayOfObjects_ReturnsArrayOfObjectsSerialized()
        {
            var data = Json.Serialize(new List<BasicJson>
            {
                BasicJson.GetDefault(),
                BasicJson.GetDefault(),
            });

            Assert.IsNotNull(data);
            Assert.AreEqual($"[{BasicStr},{BasicStr}]", data);
        }

        [Test]
        public void AdvObject_ReturnsAdvObjectSerialized()
        {
            Assert.AreEqual(AdvStr, Json.Serialize(AdvObj));
        }

        [Test]
        public void AdvObjectArray_ReturnsAdvObjectArraySerialized()
        {
            Assert.AreEqual(AdvAStr, Json.Serialize(AdvAObj));
        }

        [TestCase("1", 1)]
        [TestCase("1", 1F)]
        [TestCase("string", "string")]
        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("null", null)]
        [TestCase("null", default)]
        public void WithPrimitive_ReturnsStringValue(string expected, object actual)
        {
            Assert.AreEqual(expected, Json.Serialize(actual));
        }

        [Test]
        public void WithDateTest_ReturnsDateTestSerialized()
        {
            var obj = new DateTimeJson {Date = new DateTime(2010, 1, 1)};
            var data = Json.Serialize(obj);

            Assert.IsNotNull(data);
            Assert.AreEqual(
                $"{{\"Date\": \"{obj.Date.Value:s}\"}}", 
                data,
                "Date must be formatted as ISO");

            var dict = Json.Deserialize<Dictionary<string, DateTime>>(data);

            Assert.IsNotNull(dict);
            Assert.IsTrue(dict.ContainsKey("Date"));
            Assert.AreEqual(obj.Date, dict["Date"]);

            var objDeserialized = Json.Deserialize<DateTimeJson>(data);

            Assert.IsNotNull(objDeserialized);
            Assert.AreEqual(obj.Date, objDeserialized.Date);
        }

        [Test]
        public void WithStructureArray_ReturnsStructureArraySerialized()
        {
            var result = new[] {new SampleStruct {Value = 1, Name = "A"}, new SampleStruct {Value = 2, Name = "B"}};

            Assert.AreEqual(ArrayStruct, Json.Serialize(result));
        }

        [Test]
        public void WithEmptyClass_ReturnsEmptyClassSerialized()
        {
            Assert.AreEqual("{ }", Json.Serialize(new EmptyJson()));
        }

        [Test]
        public void WithStructure_ReturnsStructureSerialized()
        {
            Assert.AreEqual("{\"Value\": 1,\"Name\": \"DefaultStruct\"}", Json.Serialize(DefaultStruct));
        }

        [Test]
        public void WithObjEnumString_ReturnsObjectSerialized()
        {
            Assert.AreEqual("{\"Id\": 0,\"MyEnum\": 3}", Json.Serialize(new ObjectEnum()));
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
            var dataSerialized = Json.SerializeOnly(string.Empty, true, null);

            Assert.AreEqual("\"\"", dataSerialized);
        }

        [Test]
        public void WithType_ReturnsString()
        {
            var dataSerialized = Json.SerializeOnly(typeof(string), true, null);

            Assert.AreEqual("\"System.String\"", dataSerialized);
        }

        [Test]
        public void WithEmptyEnumerable_ReturnsEmptyArrayLiteral()
        {
            var emptyEnumerable = Enumerable.Empty<int>();

            var dataSerialized = Json.SerializeOnly(emptyEnumerable, true, null);

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

            var dataSerialized = Json.SerializeOnly(wordDictionary, false, null);

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
    }
}
