using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ProcessHelperTest
    {
        [Test]
        public async Task GetProcessOutputAsyncTest()
        {
            var data = await ProcessHelper.GetProcessOutputAsync("dotnet", "--help");
            Assert.IsTrue(data.StartsWith(".NET Command Line Tools"));
        }

        [Test]
        public async Task GetValidRunProcessAsyncTest()
        {
            const int OkCode = 0;
            string output = null;

            var result = await ProcessHelper.RunProcessAsync("dotnet", "--help", (data, proc) =>
            {
                if (output == null) output = Encoding.GetEncoding(0).GetString(data);

            }, null, true, default(CancellationToken));

            Assert.IsTrue(result == OkCode);
            Assert.IsNotNull(output);
        }


        [Test]
        public async Task GetInvalidRunProcessAsyncTest()
        {
            const int ErrorCode = 1;
            string output = null;

            var result = await ProcessHelper.RunProcessAsync("dotnet", "lol", null, (data, proc) =>
            {
                if (output == null) output = Encoding.GetEncoding(0).GetString(data);

            }, true, default(CancellationToken));
            
            Assert.IsTrue(result == ErrorCode);
            Assert.IsNotNull(output);
        }
    }
}