using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Swan.Abstractions;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    public class Program
    {
        private static readonly SettingsProvider<AppSettingMock> mock = new SettingsProvider<AppSettingMock>()
        {
            ConfigurationFilePath = Path.GetTempFileName()
        };

        public static void Main(string[] args)
        {
            mock.ConfigurationFilePath.Info();
            mock.Global.ToStringInvariant().Info();

            #region basic obj

            var basicObj = new BasicJson { StringData = "string", IntData = 1, NegativeInt = -1, DecimalData = 10.33M, BoolData = true };
            var basicStr = "{\"StringData\" : \"string\", \"IntData\" : 1, \"NegativeInt\" : -1, \"DecimalData\" : 10.33, \"BoolData\" : true, \"StringNull\" : null}";

            var data = Json.Serialize(basicObj);
            data.Info();

            if (data == basicStr) "Ok serialize".Info(); else "Error serialize".Error();

            var obj = Json.Deserialize<BasicJson>(basicStr);

            if (obj.StringData == basicObj.StringData) "Ok string".Info(); else "Error string".Error();

            if (obj.IntData == basicObj.IntData) "Ok int".Info(); else "Error int".Error();

            if (obj.NegativeInt == basicObj.NegativeInt) "Ok neg int".Info(); else "Error neg int".Error();

            if (obj.BoolData == basicObj.BoolData) "Ok bool".Info(); else "Error bool".Error();

            if (obj.DecimalData == basicObj.DecimalData) "Ok decimal".Info(); else "Error decimal".Error();

            if (obj.StringNull == basicObj.StringNull) "Ok null".Info(); else "Error null".Error();

            #endregion

            #region basic array

            var basicArray = new[] { "One", "Two", "Three" };
            var basicAStr = "[\"One\",\"Two\",\"Three\"]";
            var dataArr = Json.Serialize(basicArray);
            dataArr.Info();

            if (dataArr == basicAStr) "Ok serialize".Info(); else "Error serialize".Error();

            var arr = Json.Deserialize<List<string>>(basicAStr);
            
            if (string.Join(",", basicArray) == string.Join(",", arr)) "Ok array".Info(); else "Error array".Error();

            #endregion

            Console.ReadLine();
        }
    }
}