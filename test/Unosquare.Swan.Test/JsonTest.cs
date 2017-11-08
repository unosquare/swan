using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Reflection;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.JsonTests
{
    public abstract class JsonTest
    {
        protected static readonly AdvJson AdvObj = new AdvJson
        {
            StringData = "string,\r\ndata",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true,
            InnerChild = BasicJson.GetDefault()
        };

        protected const string ArrayStruct = "[{\"Value\": 1,\"Name\": \"A\"},{\"Value\": 2,\"Name\": \"B\"}]";
        
        protected static string BasicStr = "{" + BasicJson.GetControlValue() + "}";

        protected string AdvStr =
            "{\"InnerChild\": " + BasicStr + "," + BasicJson.GetControlValue() + "}";

        protected readonly string[] BasicArray = {"One", "Two", "Three"};
        protected string BasicAStr = "[\"One\",\"Two\",\"Three\"]";

        protected readonly int[] _numericArray = {1, 2, 3};
        protected string _numericAStr = "[1,2,3]";

        protected readonly BasicArrayJson _basicAObj = new BasicArrayJson
        {
            Id = 1,
            Properties = new[] {"One", "Two", "Babu"}
        };

        protected readonly AdvArrayJson _advAObj = new AdvArrayJson
        {
            Id = 1,
            Properties = new[] {BasicJson.GetDefault(), BasicJson.GetDefault()}
        };

        protected string _basicAObjStr = "{\"Id\": 1,\"Properties\": [\"One\",\"Two\",\"Babu\"]}";

        protected string _advAStr = "{\"Id\": 1,\"Properties\": [" + BasicStr + "," + BasicStr + "]}";

        protected readonly List<ExtendedPropertyInfo> _arrayOfObj = new List<ExtendedPropertyInfo>
        {
            new ExtendedPropertyInfo<AppSettingMock>(nameof(AppSettingMock.WebServerPort)),
            new ExtendedPropertyInfo<AppSettingMock>(nameof(AppSettingMock.WebServerHostname))
        };

        protected string _arrayOfObjStr =
                "[{\"Property\": \"WebServerPort\",\"DataType\": \"Int32\",\"Value\": null,\"DefaultValue\": 9898,\"Name\": \"Web Server Port\",\"Description\": \"The port on which the web server listens for requests\",\"GroupName\": \"Administration\"},{\"Property\": \"WebServerHostname\",\"DataType\": \"String\",\"Value\": null,\"DefaultValue\": \"localhost\",\"Name\": \"Web Server Host Name\",\"Description\": \"The hostname to which the web server binds, it can be localhost, a specific IP address or a '+' sign to bind to all IP addresses\",\"GroupName\": \"Administration\"}]"
            ;
    }

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
            object nullObj = null;

            Assert.AreEqual(string.Empty, nullObj.ToJson());
        }
    }

    [TestFixture]
    public class Serialize : JsonTest
    {
        [Test]
        public void StringArray_ReturnsArraySerialized()
        {
            var data = Json.Serialize(BasicArray);

            Assert.IsNotNull(data);
            Assert.AreEqual(BasicAStr, data);
        }

        [Test]
        public void WithStringsArrayAndWeakReference_ReturnsArraySerialized()
        {
            var reference = new List<WeakReference> { new WeakReference(BasicArray)};

            var data = Json.Serialize(BasicArray, false, null, false, null, null, reference);
            
            Assert.AreEqual("{ \"$circref\":", data.Substring(0, 13));
        }

        [Test]
        public void NumericArray_ReturnsArraySerialized()
        {
            var data = Json.Serialize(_numericArray);

            Assert.IsNotNull(data);
            Assert.AreEqual(_numericAStr, data);
        }

        [Test]
        public void BasicObjectWithArray_ReturnsObjectWithArraySerialized()
        {
            var data = Json.Serialize(_basicAObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAObjStr, data);
        }

        [Test]
        public void ArrayOfObjects_ReturnsArrayOfObjectsSerialized()
        {
            var data = Json.Serialize(_arrayOfObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_arrayOfObjStr, data);
        }

        [Test]
        public void AdvObject_ReturnsAdvObjectSerialized()
        {
            var data = Json.Serialize(AdvObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(AdvStr, data);
        }

        [Test]
        public void AdvObjectArray_ReturnsAdvObjectArraySerialized()
        {
            var data = Json.Serialize(_advAObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_advAStr, data);
        }

        [Test]
        public void EmptyObject_ReturnsEmptyObjectSerialized()
        {
            Assert.AreEqual("{ }", Json.Serialize(default(object)));
        }

        [Test]
        public void PrimitiveError_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Json.Serialize(1), "Throws exception serializing primitive");
        }

        [Test]
        public void DateTest_ReturnsDateTestSerialized()
        {
            var obj = new DateTimeJson {Date = new DateTime(2010, 1, 1)};
            var data = Json.Serialize(obj);

            Assert.IsNotNull(data);
            Assert.AreEqual("{\"Date\": \"" + obj.Date.Value.ToString("s") + "\"}", data,
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
            var data = Json.Serialize(result);

            Assert.IsNotNull(data);
            Assert.AreEqual(ArrayStruct, data);
        }

        [Test]
        public void EmptyClass_ReturnsEmptyClassSerialized()
        {
            var data = Json.Serialize(new EmptyJson());

            Assert.IsNotNull(data);
            Assert.AreEqual("{ }", data);
        }

        [Test]
        public void WithStructure_ReturnsStructureSerialized()
        {
            var result = new SampleStruct {Value = 1, Name = "A"};
            var data = Json.Serialize(result);

            Assert.IsNotNull(data);
            Assert.AreEqual("{\"Value\": 1,\"Name\": \"A\"}", data);
        }
    }

    [TestFixture]
    public class Deserialize : JsonTest
    {
        [Test]
        public void WithIncludeNonPublic_ReturnsObjectDeserialized()
        {
            var obj = Json.Deserialize<BasicJson>(BasicStr, false);
            
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.StringData, BasicJson.GetDefault().StringData);
            Assert.AreEqual(obj.IntData, BasicJson.GetDefault().IntData);
            Assert.AreEqual(obj.NegativeInt, BasicJson.GetDefault().NegativeInt);
            Assert.AreEqual(obj.BoolData, BasicJson.GetDefault().BoolData);
            Assert.AreEqual(obj.DecimalData, BasicJson.GetDefault().DecimalData);
            Assert.AreEqual(obj.StringNull, BasicJson.GetDefault().StringNull);
        }

        [Test]
        public void BasicObject_ReturnsObjectDeserialized()
        {
            var obj = Json.Deserialize<BasicJson>(BasicStr);

            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.StringData, BasicJson.GetDefault().StringData);
            Assert.AreEqual(obj.IntData, BasicJson.GetDefault().IntData);
            Assert.AreEqual(obj.NegativeInt, BasicJson.GetDefault().NegativeInt);
            Assert.AreEqual(obj.BoolData, BasicJson.GetDefault().BoolData);
            Assert.AreEqual(obj.DecimalData, BasicJson.GetDefault().DecimalData);
            Assert.AreEqual(obj.StringNull, BasicJson.GetDefault().StringNull);
        }

        [Test]
        public void BasicArray_ReturnsArrayDeserialized()
        {
            var arr = Json.Deserialize<List<string>>(BasicAStr);

            Assert.IsNotNull(arr);
            Assert.AreEqual(string.Join(",", BasicArray), string.Join(",", arr));
        }

        [Test]
        public void BasicObjectWithArray_ReturnsBasicObjectWithArrayDeserialized()
        {
            var data = Json.Deserialize<BasicArrayJson>(_basicAObjStr);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAObj.Id, data.Id);
            Assert.IsNotNull(data.Properties);
            Assert.AreEqual(string.Join(",", _basicAObj.Properties), string.Join(",", data.Properties));
        }

        [Test]
        public void ArrayOfObjects_ReturnsArrayOfObjectsDeserialized()
        {
            var data = Json.Deserialize<List<ExtendedPropertyInfo>>(_basicAObjStr);

            Assert.IsNotNull(data);
        }

        [Test]
        public void AdvObject_ReturnsAdvObjectDeserialized()
        {
            var data = Json.Deserialize<AdvJson>(AdvStr);

            Assert.IsNotNull(data);
            Assert.IsNotNull(data.InnerChild);

            foreach (var obj in new[] {data, data.InnerChild})
            {
                Assert.AreEqual(obj.StringData, AdvObj.StringData);
                Assert.AreEqual(obj.IntData, AdvObj.IntData);
                Assert.AreEqual(obj.NegativeInt, AdvObj.NegativeInt);
                Assert.AreEqual(obj.BoolData, AdvObj.BoolData);
                Assert.AreEqual(obj.DecimalData, AdvObj.DecimalData);
                Assert.AreEqual(obj.StringNull, AdvObj.StringNull);
            }
        }

        [Test]
        public void AdvObjectArray_ReturnsAdvObjectArray()
        {
            var data = Json.Deserialize<AdvArrayJson>(_advAStr);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAObj.Id, data.Id);
            Assert.IsNotNull(data.Properties);

            foreach (var obj in data.Properties)
            {
                Assert.AreEqual(obj.StringData, AdvObj.StringData);
                Assert.AreEqual(obj.IntData, AdvObj.IntData);
                Assert.AreEqual(obj.NegativeInt, AdvObj.NegativeInt);
                Assert.AreEqual(obj.BoolData, AdvObj.BoolData);
                Assert.AreEqual(obj.DecimalData, AdvObj.DecimalData);
                Assert.AreEqual(obj.StringNull, AdvObj.StringNull);
            }
        }

        public void EmptyString_ReturnsNull()
        {
            Assert.IsNull(Json.Deserialize(string.Empty));
        }

        [Test]
        public void EmptyStringWithTypeParam_ReturnsNull()
        {
            Assert.IsNull(Json.Deserialize<BasicJson>(string.Empty));
        }

        [Test]
        public void EmptyPropertyTest_ReturnsNotNullPropertyDeserialized()
        {
            Assert.IsNotNull(Json.Deserialize<BasicJson>("{ \"\": \"value\" }"));
        }

        [Test]
        public void ObjectWithArrayWithData_ReturnsObjectWithArrayWithDataDeserialized()
        {
            var data = Json.Deserialize<ArrayJsonWithInitialData>("{\"Id\": 2,\"Properties\": [\"THREE\"]}");

            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Id);
            Assert.AreEqual(1, data.Properties.Length);
        }

        [Test]
        public void WithJsonProperty_ReturnsPropertiesDeserialized()
        {
            var obj = new JsonPropertySample() {Data = "OK", IgnoredData = "OK"};
            var data = Json.Serialize(obj);

            Assert.IsNotNull(data);
            Assert.AreEqual("{\"data\": \"OK\"}", data);

            var objDeserialized = Json.Deserialize<JsonPropertySample>(data);

            Assert.IsNotNull(objDeserialized);
            Assert.AreEqual(obj.Data, objDeserialized.Data);
        }

        [Test]
        public void WithStructureArray_ReturnsStructureArrayDeserialized()
        {
            var data = Json.Deserialize<SampleStruct>("{\"Value\": 1,\"Name\": \"A\"}");

            Assert.IsNotNull(data);
            Assert.AreEqual(data.Value, 1);
            Assert.AreEqual(data.Name, "A");
        }

        [Test]
        public void WithStructure_ReturnsStructureDeserialized()
        {
            var data = Json.Deserialize<SampleStruct[]>(ArrayStruct);

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.AreEqual(data.First().Value, 1);
            Assert.AreEqual(data.First().Name, "A");
        }

        [Test]
        public void EmptyClass_ReturnsEmptyClassDeserialized()
        {
            Assert.IsNotNull(Json.Deserialize<EmptyJson>("{ }"));
        }
    }

    [TestFixture]
    public class SerializeOnly : JsonTest
    {
        [Test]
        public void WithObject_ReturnsObjectSerialized()
        {
            var includeNames = new []
            {
                nameof(BasicJson.StringData),
                nameof(BasicJson.IntData),
                nameof(BasicJson.NegativeInt)
            };

            var dataSerialized = Json.SerializeOnly(BasicJson.GetDefault(), false, includeNames);

            Assert.AreEqual(
                "{\"StringData\": \"string,\\r\\ndata\",\"IntData\": 1,\"NegativeInt\": -1}",
                dataSerialized);
        }

        [Test]
        public void WithString_ReturnsString()
        {
            var sdsdas = Json.SerializeOnly("\b\t\f\0", true, null);
            
            Assert.AreEqual("\"\\b\\t\\f\\u0000\"", sdsdas);
        }

        [Test]
        public void WithEmptyString_ReturnsEmptyString()
        {
            var dataSerialized = Json.SerializeOnly("", true, null);

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
            var emptyDictionary = new Dictionary<string, string>();

            var dataSerialized = Json.SerializeOnly(emptyDictionary, true, null);
            
            Assert.AreEqual("{ }", dataSerialized);
        }

        [Test]
        public void WithDictionaryOfDictionaries_ReturnsString()
        {
            var persons = new Dictionary<string, Dictionary<string, string>>
                {
                    { "Tyrande", new Dictionary<string, string> {  } },
                    { "Jaina", new Dictionary<string, string> { { "Race", "Human" }, { "Affiliation", "Alliance" } } }
                };

            var dataSerialized = Json.SerializeOnly(persons, false, null);
            
            Assert.AreEqual("{\"Tyrande\": { },\"Jaina\": {\"Race\": \"Human\",\"Affiliation\": \"Alliance\"}}", dataSerialized);
        }

        [Test]
        public void WithDictionaryOfArrays_ReturnsString()
        {
            var wordDictionary =
                new Dictionary<string, string[][]> { { "Horde Capitals", new[] { new string[] { } , new[] {"Orgrimmar", "Thunder Bluff"} } } };

            var dataSerialized = Json.SerializeOnly(wordDictionary, false, null);
            
            Assert.AreEqual("{\"Horde Capitals\": [[ ],[\"Orgrimmar\",\"Thunder Bluff\"]]}", dataSerialized);
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
                nameof(BasicJson.NegativeInt)
            };

            var dataSerialized = Json.SerializeExcluding(BasicJson.GetDefault(), false, excludeNames);

            Assert.AreEqual(
                "{\"DecimalData\": 10.33,\"BoolData\": true,\"StringNull\": null}",
                dataSerialized);
        }
    }
}