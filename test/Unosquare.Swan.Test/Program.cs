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
using Unosquare.Swan.Components;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var first = new[] { 1, 2, 3 };
            var second = new[] { 1, 2, 4 };

            Assert.IsFalse(ObjectComparer.AreEnumsEqual(first, second));

            Console.ReadLine();
        }
    }
}