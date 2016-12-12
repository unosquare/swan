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
            var basicObj = new BasicJson { StringData = "string", IntData = 1, BoolData = true };
            var basicStr = "{ \"StringData\" : \"string\", \"IntData\" : 1, \"BoolData\" : true, \"StringNull\" : null}";

            var data = Json.Serialize(basicObj);
            data.Info();

            var obj = Json.Deserialize<BasicJson>(basicStr);
            obj.ToStringInvariant().Info();

            Console.ReadLine();
        }
    }
}