using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unosquare.Swan.Abstractions;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Runtime;
using Unosquare.Swan.Test.Mocks;
using Unosquare.Swan.Utilities;

namespace Unosquare.Swan.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SettingsProvider<AppSettingMock>.Instance.ConfigurationFilePath = Path.GetTempFileName();
            SettingsProvider<AppSettingMock>.Instance.ResetGlobalSettings();
            var data = SettingsProvider<AppSettingMock>.Instance.Global.BackgroundImage;

            Terminal.ReadKey(true);
        }
    }
}