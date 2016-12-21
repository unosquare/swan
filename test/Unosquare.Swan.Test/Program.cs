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
            var test = new SettingsProviderTest();
            test.Setup();
            test.RefreshFromListTest();
            Terminal.ReadKey(true);
        }
    }
}