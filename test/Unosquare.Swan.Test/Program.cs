using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var basicObj = new BasicJson { StringData = "string", IntData = 1, NegativeInt = -1, DecimalData = 10.33M, BoolData = true };
            var basicStr = "{\"StringData\" : \"string\", \"IntData\" : 1, \"NegativeInt\" : -1, \"DecimalData\" : 10.33, \"BoolData\" : true, \"StringNull\" : null}";

            var data = Json.Serialize(basicObj);
            data.Info();

            if (data == basicStr) "Cool serialize".Info();

            var obj = Json.Deserialize<BasicJson>(basicStr);
            
            if (obj.StringData == basicObj.StringData) "Cool string".Info();

            if (obj.IntData == basicObj.IntData) "Cool int".Info();

            if (obj.NegativeInt == basicObj.NegativeInt) "Cool neg int".Info();

            if (obj.BoolData == basicObj.BoolData) "Cool bool".Info();

            if (obj.DecimalData == basicObj.DecimalData) "Cool decimal".Info();

            if (obj.StringNull == basicObj.StringNull) "Cool null".Info();

            Console.ReadLine();
        }
    }
}