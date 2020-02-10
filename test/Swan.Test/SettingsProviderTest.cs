namespace Swan.Test
{
    using Configuration;
    using Mocks;
    using NUnit.Framework;
    using System.IO;

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
    }
}
