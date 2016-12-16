using System;
using System.Collections.Generic;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Reflection;
using Unosquare.Swan.Runtime;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tst = new JsonTest();
            tst.DeserializeAdvObjectArrayTest();
            
            //var dumpArgs = new[] { "--ño", "-n", "babu", "--verbose", "--color", "white" };
            //var result = CmdArgsParser.Default.ParseArguments<OptionMock>(dumpArgs);

            Terminal.ReadKey(true);
        }
    }
}