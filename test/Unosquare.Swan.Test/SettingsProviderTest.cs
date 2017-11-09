namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System.IO;
    using System.Linq;
    using Abstractions;
    using Mocks;

    [TestFixture]
    public class SettingsProviderTest
    {
        [SetUp]
        public void Setup()
        {
            SettingsProvider<AppSettingMock>.Instance.ConfigurationFilePath = Path.GetTempFileName();
            SettingsProvider<AppSettingMock>.Instance.ResetGlobalSettings();
        }
        
        [Test]
        public void TryGlobalTest()
        {
            Assert.IsNotNull(SettingsProvider<AppSettingMock>.Instance.Global);
            Assert.IsNotNull(SettingsProvider<AppSettingMock>.Instance.Global.WebServerHostname);
            Assert.IsNotNull(SettingsProvider<AppSettingMock>.Instance.Global.WebServerPort);

            var appSettings = new AppSettingMock();

            Assert.AreEqual(appSettings.WebServerHostname, SettingsProvider<AppSettingMock>.Instance.Global.WebServerHostname);
        }

        [Test]
        public void GetListTest()
        {
            Assert.IsNotNull(SettingsProvider<AppSettingMock>.Instance.GetList());

            Assert.AreEqual(3, SettingsProvider<AppSettingMock>.Instance.GetList().Count);
            Assert.AreEqual(typeof(int).Name, SettingsProvider<AppSettingMock>.Instance.GetList().First().DataType);
            Assert.AreEqual("WebServerPort", SettingsProvider<AppSettingMock>.Instance.GetList().First().Property);
            Assert.AreEqual(9898, SettingsProvider<AppSettingMock>.Instance.GetList().First().DefaultValue);
        }

        [Test]
        public void RefreshFromListTest()
        {
            var arraySample = new[] {"One", "Two"};

            var list = SettingsProvider<AppSettingMock>.Instance.GetList();
            list[0].Value = 100;
            list[2].Value = arraySample;

            var updateList = SettingsProvider<AppSettingMock>.Instance.RefreshFromList(list);
            Assert.IsNotNull(updateList);
            Assert.AreEqual(2, updateList.Count);
            Assert.AreEqual(list[0].Value, SettingsProvider<AppSettingMock>.Instance.Global.WebServerPort);
            Assert.AreEqual(arraySample, SettingsProvider<AppSettingMock>.Instance.Global.BackgroundImage);
        }
    }
}
