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
        private readonly BasicJson _basicObj = new BasicJson
        {
            StringData = "string",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true
        };

        private string _basicStr =
            "{\"StringData\" : \"string\", \"IntData\" : 1, \"NegativeInt\" : -1, \"DecimalData\" : 10.33, \"BoolData\" : true, \"StringNull\" : null}";

        private readonly string[] _basicArray = {"One", "Two", "Three"};
        private string _basicAStr = "[\"One\",\"Two\",\"Three\"]";

        private readonly BasicArrayJson _basicAObj = new BasicArrayJson
        {
            Id = 1,
            Properties = new[] {"One", "Two", "Babu"}
        };

        private string _basicAObjStr = "{\"Id\" : 1, \"Properties\" : [\"One\",\"Two\",\"Babu\"]}";

        private readonly List<ExtendedPropertyInfo> _arrayOfObj = new List<ExtendedPropertyInfo>
        {
            new ExtendedPropertyInfo<AppSettingMock>(nameof(AppSettingMock.WebServerPort)),
            new ExtendedPropertyInfo<AppSettingMock>(nameof(AppSettingMock.WebServerHostname))
        };

        private string _arrayOfObjStr =
            "[{\"Property\":\"WebServerPort\",\"DataType\":\"Int32\",\"Value\":9898,\"DefaultValue\":9898,\"Name\":\"Web Server Port\",\"Description\":\"The port on which the web server listens for requests\",\"GroupName\":\"Administration\"},{\"Property\":\"WebServerHostname\",\"DataType\":\"String\",\"Value\":\"localhost\",\"DefaultValue\":\"localhost\",\"Name\":\"Web Server Host Name\",\"Description\":\"The hostname to which the web server binds, it can be localhost, a specific IP address or a '+' sign to bind to all IP addresses\",\"GroupName\":\"Administration\"}]";

        [Test]
        public void SerializeBasicObjectTest()
        {
            var data = JsonFormatter.Serialize(_basicObj);

            Assert.IsNotNull(data);
            Assert.AreEqual(_basicStr, data);
        }

        [Test]
        public void DeserializeBasicObjectTest()
        {
            var obj = JsonFormatter.Deserialize<BasicJson>(_basicStr);

            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.StringData, _basicObj.StringData);
            Assert.AreEqual(obj.IntData, _basicObj.IntData);
            Assert.AreEqual(obj.NegativeInt, _basicObj.NegativeInt);
            Assert.AreEqual(obj.BoolData, _basicObj.BoolData);
            Assert.AreEqual(obj.DecimalData, _basicObj.DecimalData);
            Assert.AreEqual(obj.StringNull, _basicObj.StringNull);
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
    }
}