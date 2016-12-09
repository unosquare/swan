using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Abstractions;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SettingsProviderTest
    {
        private SettingsProvider<AppSettingMock> mock = new SettingsProvider<AppSettingMock>();

        [Test]
        public void GlobalTest()
        {
            Assert.IsNotNull(mock.Global);
        }
    }
}
