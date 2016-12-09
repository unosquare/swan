using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Unosquare.Swan.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Factory.StartNew(async () =>
            {
                var result = await ProcessHelper.RunProcessAsync("dotnet", $"--help", (data, proc) =>
                {
                    $"Code {proc.ExitCode}".Info();
                }, null, true, default(CancellationToken));

                $"Result {result}".Info();
            });

            "Waiting".Info();
            Console.ReadLine();
        }
    }
}
