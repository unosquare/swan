using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Reflection;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class JsonTest
    {
        private static readonly AdvJson AdvObj = new AdvJson
        {
            StringData = "string",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true,
            InnerChild = BasicJson.GetDefault()
        };

        private const string ArrayStruct = "[{\"Value\": 1,\"Name\": \"A\"},{\"Value\": 2,\"Name\": \"B\"}]";

        private const string BasicStrWithoutWrap =
            "\"StringData\": \"string\",\"IntData\": 1,\"NegativeInt\": -1,\"DecimalData\": 10.33,\"BoolData\": true,\"StringNull\": null";

        private const string BasicStr = "{" + BasicStrWithoutWrap + "}";

        private const string AdvStr =
            "{\"InnerChild\": " + BasicStr + "," + BasicStrWithoutWrap + "}";

        private readonly string[] _basicArray = {"One", "Two", "Three"};
        private string _basicAStr = "[\"One\",\"Two\",\"Three\"]";

        private readonly int[] _numericArray = {1, 2, 3};
        private string _numericAStr = "[1,2,3]";

        private readonly BasicArrayJson _basicAObj = new BasicArrayJson
        {
            Id = 1,
            Properties = new[] {"One", "Two", "Babu"}
        };

        private readonly AdvArrayJson _advAObj = new AdvArrayJson
        {
            Id = 1,
            Properties = new[] {BasicJson.GetDefault(), BasicJson.GetDefault()}
        };

        private string _basicAObjStr = "{\"Id\": 1,\"Properties\": [\"One\",\"Two\",\"Babu\"]}";

        private string _advAStr = "{\"Id\": 1,\"Properties\": [" + BasicStr + "," + BasicStr + "]}";

        private readonly List<ExtendedPropertyInfo> _arrayOfObj = new List<ExtendedPropertyInfo>
        {
            new ExtendedPropertyInfo<AppSettingMock>(nameof(AppSettingMock.WebServerPort)),
            new ExtendedPropertyInfo<AppSettingMock>(nameof(AppSettingMock.WebServerHostname))
        };

        private string _arrayOfObjStr =
            "[{\"Property\": \"WebServerPort\",\"DataType\": \"Int32\",\"Value\": null,\"DefaultValue\": 9898,\"Name\": \"Web Server Port\",\"Description\": \"The port on which the web server listens for requests\",\"GroupName\": \"Administration\"},{\"Property\": \"WebServerHostname\",\"DataType\": \"String\",\"Value\": null,\"DefaultValue\": \"localhost\",\"Name\": \"Web Server Host Name\",\"Description\": \"The hostname to which the web server binds, it can be localhost, a specific IP address or a '+' sign to bind to all IP addresses\",\"GroupName\": \"Administration\"}]";

        private struct SampleStruct
        {
            public int Value;
            public string Name;
        }

        [Test]
        public void SerializeBasicObjectTest()
        {
            var data = BasicJson.GetDefault().ToJson(false);

            Assert.IsNotNull(data);
            Assert.AreEqual(BasicStr, data);
        }

        [Test]
        public void DeserializeBasicObjectTest()
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
        public void SerializeStringArrayTest()
        {
            var data = Json.Serialize(_basicArray);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAStr, data);
        }

        [Test]
        public void SerializeNumericArrayTest()
        {
            var data = Json.Serialize(_numericArray);

            Assert.IsNotNull(data);
            Assert.AreEqual(_numericAStr, data);
        }

        [Test]
        public void DeserializeBasicArrayTest()
        {
            var arr = Json.Deserialize<List<string>>(_basicAStr);
            Assert.IsNotNull(arr);
            Assert.AreEqual(string.Join(",", _basicArray), string.Join(",", arr));
        }

        [Test]
        public void SerializeBasicObjectWithArrayTest()
        {
            var data = Json.Serialize(_basicAObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAObjStr, data);
        }

        [Test]
        public void DeserializeBasicObjectWithArrayTest()
        {
            var data = Json.Deserialize<BasicArrayJson>(_basicAObjStr);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAObj.Id, data.Id);
            Assert.IsNotNull(data.Properties);
            Assert.AreEqual(string.Join(",", _basicAObj.Properties), string.Join(",", data.Properties));
        }

        [Test]
        public void SerializeArrayOfObjectsTest()
        {
            var data = Json.Serialize(_arrayOfObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_arrayOfObjStr, data);
        }

        [Test]
        public void DeserializeArrayOfObjectsTest()
        {
            var data = Json.Deserialize<List<ExtendedPropertyInfo>>(_basicAObjStr);

            Assert.IsNotNull(data);
        }

        [Test]
        public void SerializeAdvObjectTest()
        {
            var data = Json.Serialize(AdvObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(AdvStr, data);
        }

        [Test]
        public void DeserializeAdvObjectTest()
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
        public void SerializeAdvObjectArrayTest()
        {
            var data = Json.Serialize(_advAObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_advAStr, data);
        }

        [Test]
        public void DeserializeAdvObjectArrayTest()
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

        [Test]
        public void SerializeEmptyObjectTest()
        {
            Assert.AreEqual("{ }", Json.Serialize(default(object)));
        }

        [Test]
        public void SerializePrimitiveErrorTest()
        {
            Assert.Throws<ArgumentException>(() => Json.Serialize(1), "Throws exception serializing primitive");
        }

        [Test]
        public void DeserializeEmptyStringErrorTest()
        {
            Assert.IsNull(Json.Deserialize(string.Empty));
            Assert.IsNull(Json.Deserialize<BasicJson>(string.Empty));
        }

        [Test]
        public void DeserializeEmptyPropertyTest()
        {
            Assert.IsNotNull(Json.Deserialize<BasicJson>("{ \"\": \"value\" }"));
        }

        [Test]
        public void CheckJsonFormat()
        {
            Assert.AreEqual(BasicStr, BasicJson.GetDefault().ToJson(false));
            Assert.AreNotEqual(BasicStr, BasicJson.GetDefault().ToJson());

            object nullObj = null;
            Assert.AreEqual(string.Empty, nullObj.ToJson());
        }

        [Test]
        public void DeserializeObjectWithArrayWithDataTest()
        {
            var data = Json.Deserialize<ArrayJsonWithInitialData>("{\"Id\": 2,\"Properties\": [\"THREE\"]}");
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Id);
            Assert.AreEqual(1, data.Properties.Length);
        }

        [Test]
        public void SerializeDateTest()
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
        public void SerializeWithJsonPropertyTest()
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
        public void SerializeWithStructureTest()
        {
            var result = new SampleStruct {Value = 1, Name = "A"};

            var data = Json.Serialize(result);
            Assert.IsNotNull(data);
            Assert.AreEqual("{\"Value\": 1,\"Name\": \"A\"}", data);
        }

        [Test]
        public void SerializeWithStructureArrayTest()
        {
            var result = new[] {new SampleStruct {Value = 1, Name = "A"}, new SampleStruct {Value = 2, Name = "B"}};

            var data = Json.Serialize(result);
            Assert.IsNotNull(data);
            Assert.AreEqual(ArrayStruct, data);
        }

        [Test]
        public void DeserializeWithStructureArrayTest()
        {
            var data = Json.Deserialize<SampleStruct>("{\"Value\": 1,\"Name\": \"A\"}");
            Assert.IsNotNull(data);
            Assert.AreEqual(data.Value, 1);
            Assert.AreEqual(data.Name, "A");
        }

        [Test]
        public void DeserializeWithStructureTest()
        {
            var data = Json.Deserialize<SampleStruct[]>(ArrayStruct);
            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.AreEqual(data.First().Value, 1);
            Assert.AreEqual(data.First().Name, "A");
        }
    }
}