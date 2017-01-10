using System;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = Json.Deserialize<ArrayJsonWithInitialData>("{\"Id\": 2,\"Properties\": [\"THREE\"]}");
            data.Stringify().Info();

            Terminal.ReadKey(true, true);
        }
    }
}