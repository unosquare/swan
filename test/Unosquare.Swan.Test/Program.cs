using System;
using System.Collections.Generic;
using Unosquare.Swan.Abstractions;
using Unosquare.Swan.Formatters;
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

            //          var test = @"{
            //""runtimeTarget"": {
            //              ""name"": "".NETCoreApp,Version=v1.1/win8-x64"",
            //  ""signature"": ""e7a9f33347f5f1089fe569c200a1c8eb4adc96a1""
            //}";

            //          var data = JsonFormatter.Deserialize(test);

            var data = SettingsProvider<AppSettingMock>.Instance.GetList();
            
            Terminal.ReadKey(true);
        }
    }
}