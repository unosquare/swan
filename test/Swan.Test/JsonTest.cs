namespace Swan.Test.JsonTests
{
    using Formatters;
    using Mocks;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class JsonTest : TestFixtureBase
    {
        protected const string ArrayStruct = "[{\"Value\": 1,\"Name\": \"A\"},{\"Value\": 2,\"Name\": \"B\"}]";

        protected static readonly AdvJson AdvObj = new()
        {
            StringData = "string,\r\ndata\\",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true,
            InnerChild = BasicJson.GetDefault(),
        };

        protected static string BasicStr => "{" + BasicJson.GetControlValue() + "}";

        protected string AdvStr =>
            "{\"InnerChild\": " + BasicStr + "," + BasicJson.GetControlValue() + "}";

        protected string BasicAStr => "[\"A\",\"B\",\"C\"]";

        protected int[] NumericArray => new[] {1, 2, 3};

        protected string NumericAStr => "[1,2,3]";

        protected BasicArrayJson BasicAObj => new()
        {
            Id = 1,
            Properties = new[] {"One", "Two", "Babu"},
        };

        protected AdvArrayJson AdvAObj => new()
        {
            Id = 1,
            Properties = new[] {BasicJson.GetDefault(), BasicJson.GetDefault()},
        };

        protected string BasicAObjStr => "{\"Id\": 1,\"Properties\": [\"One\",\"Two\",\"Babu\"]}";

        protected string AdvAStr => "{\"Id\": 1,\"Properties\": [" + BasicStr + "," + BasicStr + "]}";
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
        public void WithBasicObject_ReturnsObjectDeserialized()
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
        public void WithBasicArray_ReturnsArrayDeserialized()
        {
            var arr = Json.Deserialize<List<string>>(BasicAStr);

            Assert.IsNotNull(arr);
            Assert.AreEqual(string.Join(",", DefaultStringList), string.Join(",", arr));
        }

        [Test]
        public void WithBasicObjectWithArray_ReturnsBasicObjectWithArrayDeserialized()
        {
            var data = Json.Deserialize<BasicArrayJson>(BasicAObjStr);

            Assert.IsNotNull(data);
            Assert.AreEqual(BasicAObj.Id, data.Id);
            Assert.IsNotNull(data.Properties);
            Assert.AreEqual(string.Join(",", BasicAObj.Properties), string.Join(",", data.Properties));
        }

        [Test]
        public void WithAdvObject_ReturnsAdvObjectDeserialized()
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
        public void WithAdvObjectArray_ReturnsAdvObjectArray()
        {
            var data = Json.Deserialize<AdvArrayJson>(AdvAStr);

            Assert.IsNotNull(data);
            Assert.AreEqual(BasicAObj.Id, data.Id);
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
        public void WithEmptyString_ReturnsNull()
        {
            Assert.IsNull(Json.Deserialize(string.Empty));
        }

        [Test]
        public void WithEmptyStringWithTypeParam_ReturnsNull()
        {
            Assert.IsNull(Json.Deserialize<BasicJson>(string.Empty));
        }

        [Test]
        public void WithEmptyPropertyTest_ReturnsNotNullPropertyDeserialized()
        {
            Assert.IsNotNull(Json.Deserialize<BasicJson>("{ \"\": \"value\" }"));
        }

        [Test]
        public void ObjectWithArrayWithData_ReturnsObjectWithArrayWithDataDeserialized()
        {
            var data = Json.Deserialize<ArrayJsonWithInitialData>("{\"Id\": 2,\"Properties\": [\"THREE\"]}");

            Assert.AreEqual(2, data.Id);
            Assert.AreEqual(1, data.Properties.Length);
        }

        [Test]
        public void WithJsonProperty_ReturnsPropertiesDeserialized()
        {
            var obj = new JsonPropertySample {Data = "OK", IgnoredData = "OK"};
            var data = Json.Serialize(obj);

            Assert.AreEqual("{\"data\": \"OK\"}", data);

            var objDeserialized = Json.Deserialize<JsonPropertySample>(data);

            Assert.AreEqual(obj.Data, objDeserialized.Data);
        }

        [Test]
        public void WithStructureArray_ReturnsStructureArrayDeserialized()
        {
            var data = Json.Deserialize<SampleStruct>("{\"Value\": 1,\"Name\": \"A\"}");

            Assert.AreEqual(data.Value, 1);
            Assert.AreEqual(data.Name, "A");
        }

        [Test]
        public void WithStructure_ReturnsStructureDeserialized()
        {
            var data = Json.Deserialize<SampleStruct[]>(ArrayStruct);

            Assert.IsTrue(data.Any());
            Assert.AreEqual(data.First().Value, 1);
            Assert.AreEqual(data.First().Name, "A");
        }

        [Test]
        public void WithEmptyClass_ReturnsEmptyClassDeserialized()
        {
            Assert.IsNotNull(Json.Deserialize<EmptyJson>("{ }"));
        }

        [Test]
        public void WithEmptyType_ResolveType()
        {
            Assert.IsNotNull(Json.Deserialize(BasicStr, null));
        }

        [Test]
        public void WithClassWithoutPublicCtor_ReturnDefault()
        {
            // Default value is null
            Assert.IsNull(Json.Deserialize<BasicJsonWithoutCtor>(BasicStr));
        }

        [Test]
        public void WithInvalidProperty_ReturnDefaultValueProperty()
        {
            var obj = Json.Deserialize<BasicJson>(BasicStr.Replace("\"NegativeInt\": -1", "\"NegativeInt\": \"OK\""));

            Assert.IsNotNull(obj);
            Assert.AreEqual(default(int), obj.NegativeInt);
        }

        [Test]
        public void WithEnumStringProperty_ReturnValidObject()
        {
            var obj = Json.Deserialize<ObjectEnum>("{ \"Id\": 1, \"MyEnum\": \"Three\" }");

            Assert.IsNotNull(obj);
            Assert.AreEqual(MyEnum.Three, obj.MyEnum);
        }

        [Test]
        public void WithEnumIntProperty_ReturnValidObject()
        {
            var obj = Json.Deserialize<ObjectEnum>("{ \"Id\": 1, \"MyEnum\": 3 }");

            Assert.IsNotNull(obj);
            Assert.AreEqual(MyEnum.Three, obj.MyEnum);
        }

        [Test]
        public void WithByteArrayProperty_ReturnValidObject()
        {
            var obj = Json.Deserialize<JsonFile>("{ \"Data\": \"DATA1\", \"Filename\": \"Three\" }");

            Assert.IsNotNull(obj);
            Assert.AreEqual("DATA1", obj.Data.ToText());
        }

        [Test]
        public void WithClassNoEmptyConstructor_ReturnClassDeserialized()
        {
            var obj = Json.Deserialize<ObjectNoEmptyCtor>("{ \"Id\": 1 }");

            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.Id);
        }
    }
}
