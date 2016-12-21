using NUnit.Framework;
using System.Collections.Generic;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Reflection;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class JsonTest
    {
        private static readonly AdvJson _advObj = new AdvJson
        {
            StringData = "string",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true,
            InnerChild = BasicJson.GetDefault()
        };

        const string _basicStr =
            "{\"StringData\" : \"string\", \"IntData\" : 1, \"NegativeInt\" : -1, \"DecimalData\" : 10.33, \"BoolData\" : true, \"StringNull\" : null}";

        const string _advStr =
            "{\"InnerChild\" : " + _basicStr + ", \"StringData\" : \"string\", \"IntData\" : 1, \"NegativeInt\" : -1, \"DecimalData\" : 10.33, \"BoolData\" : true, \"StringNull\" : null}";

        private readonly string[] _basicArray = { "One", "Two", "Three" };
        private string _basicAStr = "[ \"One\",\"Two\",\"Three\" ]";

        private readonly BasicArrayJson _basicAObj = new BasicArrayJson
        {
            Id = 1,
            Properties = new[] { "One", "Two", "Babu" }
        };

        private readonly AdvArrayJson _advAObj = new AdvArrayJson
        {
            Id = 1,
            Properties = new[] { BasicJson.GetDefault(), BasicJson.GetDefault() }
        };

        private string _basicAObjStr = "{\"Id\" : 1, \"Properties\" : [ \"One\",\"Two\",\"Babu\" ]}";

        private string _advAStr = "{\"Id\" : 1, \"Properties\" : [ " + _basicStr + "," + _basicStr + " ]}";

        private readonly List<ExtendedPropertyInfo> _arrayOfObj = new List<ExtendedPropertyInfo>
        {
            new ExtendedPropertyInfo<AppSettingMock>(nameof(AppSettingMock.WebServerPort)),
            new ExtendedPropertyInfo<AppSettingMock>(nameof(AppSettingMock.WebServerHostname))
        };

        private string _arrayOfObjStr =
            "[ {\"Property\" : \"WebServerPort\", \"DataType\" : \"Int32\", \"Value\" : null, \"DefaultValue\" : 9898, \"Name\" : \"Web Server Port\", \"Description\" : \"The port on which the web server listens for requests\", \"GroupName\" : \"Administration\"},{\"Property\" : \"WebServerHostname\", \"DataType\" : \"String\", \"Value\" : null, \"DefaultValue\" : \"localhost\", \"Name\" : \"Web Server Host Name\", \"Description\" : \"The hostname to which the web server binds, it can be localhost, a specific IP address or a '+' sign to bind to all IP addresses\", \"GroupName\" : \"Administration\"} ]";

        [Test]
        public void SerializeBasicObjectTest()
        {
            var data = JsonFormatter.Serialize(BasicJson.GetDefault());

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicStr, data);
        }

        [Test]
        public void DeserializeBasicObjectTest()
        {
            var obj = JsonFormatter.Deserialize<BasicJson>(_basicStr);

            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.StringData, BasicJson.GetDefault().StringData);
            Assert.AreEqual(obj.IntData, BasicJson.GetDefault().IntData);
            Assert.AreEqual(obj.NegativeInt, BasicJson.GetDefault().NegativeInt);
            Assert.AreEqual(obj.BoolData, BasicJson.GetDefault().BoolData);
            Assert.AreEqual(obj.DecimalData, BasicJson.GetDefault().DecimalData);
            Assert.AreEqual(obj.StringNull, BasicJson.GetDefault().StringNull);
        }

        [Test]
        public void SerializeBasicArrayTest()
        {
            var data = JsonFormatter.Serialize(_basicArray);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAStr, data);
        }

        [Test]
        public void DeserializeBasicArrayTest()
        {
            var arr = JsonFormatter.Deserialize<List<string>>(_basicAStr);
            Assert.IsNotNull(arr);
            Assert.AreEqual(string.Join(",", _basicArray), string.Join(",", arr));
        }

        [Test]
        public void SerializeBasicObjectWithArrayTest()
        {
            var data = JsonFormatter.Serialize(_basicAObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAObjStr, data);
        }

        [Test]
        public void DeserializeBasicObjectWithArrayTest()
        {
            var data = JsonFormatter.Deserialize<BasicArrayJson>(_basicAObjStr);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAObj.Id, data.Id);
            Assert.IsNotNull(_basicAObj.Properties);
            Assert.AreEqual(string.Join(",", _basicAObj.Properties), string.Join(",", data.Properties));
        }

        [Test]
        public void SerializeArrayOfObjectsTest()
        {
            var data = JsonFormatter.Serialize(_arrayOfObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_arrayOfObjStr, data);
        }

        [Test]
        public void DeserializeArrayOfObjectsTest()
        {
            var data = JsonFormatter.Deserialize<List<ExtendedPropertyInfo>>(_basicAObjStr);

            Assert.IsNotNull(data);
        }

        [Test]
        public void SerializeAdvObjectTest()
        {
            var data = JsonFormatter.Serialize(_advObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_advStr, data);
        }

        [Test]
        public void DeserializeAdvObjectTest()
        {
            var data = JsonFormatter.Deserialize<AdvJson>(_advStr);

            Assert.IsNotNull(data);
            Assert.IsNotNull(data.InnerChild);

            foreach (var obj in new[] { data, data.InnerChild })
            {
                Assert.AreEqual(obj.StringData, _advObj.StringData);
                Assert.AreEqual(obj.IntData, _advObj.IntData);
                Assert.AreEqual(obj.NegativeInt, _advObj.NegativeInt);
                Assert.AreEqual(obj.BoolData, _advObj.BoolData);
                Assert.AreEqual(obj.DecimalData, _advObj.DecimalData);
                Assert.AreEqual(obj.StringNull, _advObj.StringNull);
            }
        }

        [Test]
        public void SerializeAdvObjectArrayTest()
        {
            var data = JsonFormatter.Serialize(_advAObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_advAStr, data);
        }

        [Test]
        public void DeserializeAdvObjectArrayTest()
        {
            var data = JsonFormatter.Deserialize<AdvArrayJson>(_advAStr);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicAObj.Id, data.Id);
            Assert.IsNotNull(_basicAObj.Properties);

            foreach (var obj in data.Properties)
            {
                Assert.AreEqual(obj.StringData, _advObj.StringData);
                Assert.AreEqual(obj.IntData, _advObj.IntData);
                Assert.AreEqual(obj.NegativeInt, _advObj.NegativeInt);
                Assert.AreEqual(obj.BoolData, _advObj.BoolData);
                Assert.AreEqual(obj.DecimalData, _advObj.DecimalData);
                Assert.AreEqual(obj.StringNull, _advObj.StringNull);
            }
        }
    }
}