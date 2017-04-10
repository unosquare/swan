using MimeKit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var instance = MockProvider.Instance)
            {
                instance.GetName().Info();
            }

            Console.ReadLine();
        }
    }
}