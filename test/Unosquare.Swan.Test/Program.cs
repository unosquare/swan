using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Factory.StartNew(async () =>
            {
                var data = await ProcessHelper.GetProcessOutputAsync("dotnet", "--help");
                data.Info();
            });

            "Waiting".Info();
            Console.ReadLine();
        }
    }
}
