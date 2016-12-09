using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan;

namespace Unosquare.Swan.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Dictionary<ConsoleKey, string> SampleOptions = new Dictionary<ConsoleKey, string>
            {
                { ConsoleKey.A, "Sample A" },
                { ConsoleKey.B, "Sample B" }
            };

            Terminal.PrintCurrentCodePage();
            $"This is some error".Error();
            $"This is some info".Info();
            $"This is some warning".Warn();
            $"This is some tracing info".Trace();
            $"This is for debugging stuff".Debug();

            var input = "Please provide an option".ReadPrompt(SampleOptions, "Exit this program");

            

            byte output = 255;
            var success = Constants.BasicTypesInfo[typeof(byte)].TryParse("4", out output);
            $"Success: {success}, Output: {output}".Info();

            "Enter any key to exit".ReadKey();

            
        }
    }
}
