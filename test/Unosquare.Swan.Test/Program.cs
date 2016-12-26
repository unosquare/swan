using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        private static string _basicAObjStr = "{\"Id\": 1,\"Properties\": [\"One\",\"Two\",\"Babu\"]}";

        public static void Main(string[] args)
        {
            var data = Json.Deserialize<BasicArrayJson>(_basicAObjStr);
            Terminal.ReadKey(true);
        }
    }
}