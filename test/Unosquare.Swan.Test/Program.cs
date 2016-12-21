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
            CurrentApp.Container.AutoRegister();
            CurrentApp.Container.CanResolve<ICar>().ToStringInvariant().Info();
            var car = CurrentApp.Container.Resolve<ICar>();

            Terminal.ReadKey(true);
        }
    }
}