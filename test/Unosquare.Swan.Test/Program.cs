using MimeKit;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var testWork = new ObjectMapperTest();
            testWork.SimpleMapTest();
            Terminal.ReadKey(true, true);
        }
    }
}