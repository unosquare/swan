using System;
using System.Collections.Generic;
using Unosquare.Swan.Runtime;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var options = new OptionMock();

            //var dumpArgs = new[] { "--color", System.ConsoleColor.White.ToString().ToLowerInvariant() };


            //if (ArgumentParser.Default.ParseArguments(dumpArgs, options))
            //    "OK".Info();

            dynamic assemblies = Swan.Runtime.AppDomain.CurrentDomain.GetDependencyContext();
            var data = assemblies as IDictionary<String, object>;

            Terminal.ReadKey(true);
        }
    }
}