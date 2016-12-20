using NUnit.Framework;
using System.IO;
using System.Linq;
using Unosquare.Swan.Abstractions;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SettingsProviderTest
    {
        private readonly SettingsProvider<AppSettingMock> _mock = new SettingsProvider<AppSettingMock>()
        {
            ConfigurationFilePath = Path.GetTempFileName()
        };

        [Test]
        public void TryGlobalTest()
        {
            Assert.IsNotNull(_mock.Global);
            Assert.IsNotNull(_mock.Global.WebServerHostname);
            Assert.IsNotNull(_mock.Global.WebServerPort);

            var appSettings = new AppSettingMock();

            Assert.AreEqual(appSettings.WebServerHostname, _mock.Global.WebServerHostname);
            Assert.AreEqual(appSettings.WebServerPort, _mock.Global.WebServerPort);
        }

        [Test]
        public void GetListTest()
        {
            Assert.IsNotNull(_mock.GetList());

            Assert.AreEqual(3, _mock.GetList().Count);
            Assert.AreEqual(typeof(int).Name, _mock.GetList().First().DataType);
            Assert.AreEqual("WebServerPort", _mock.GetList().First().Property);
            Assert.AreEqual(9898, _mock.GetList().First().DefaultValue);
        }
    }
}
