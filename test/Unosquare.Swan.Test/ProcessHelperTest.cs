using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ProcessHelperTest
    {
        [Test]
        public async Task GetProcessOutputAsyncTest()
        {
            var data = await ProcessHelper.GetProcessOutputAsync("dotnet", $"--help");
            Assert.IsTrue(data.StartsWith(".NET Command Line Tools"));
        }
    }
}
