using System;
using System.Collections.Generic;
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
        public static void Main(string[] args)
        {
            Task.Factory.StartNew(async () =>
            {
                var test = new JsonClientTest();
                await test.PostWithAuthenticationTest();
            }).Unwrap().Wait();

            Terminal.ReadKey(true);
        }
    }
}