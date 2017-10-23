using NUnit.Framework;
using System;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Swan.Components;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ProcessHelperTest
    {
        [Test]
        public async Task GetProcessOutputAsyncTest()
        {
            var data = await ProcessRunner.GetProcessOutputAsync("dotnet", "--help");
            Assert.IsNotEmpty(data);
            Assert.IsTrue(data.StartsWith(".NET Command Line Tools"));
        }

        [Test]
        public async Task GetValidRunProcessAsyncTest()
        {
            const int okCode = 0;
            string output = null;

            var result = await ProcessRunner.RunProcessAsync("dotnet", "--help", (data, proc) =>
            {
                if (output == null) output = Encoding.GetEncoding(0).GetString(data);

            }, null);

            Assert.IsTrue(result == okCode);
            Assert.IsNotNull(output);
        }

        [Test]
        public async Task GetInvalidRunProcessAsyncTest()
        {
            if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
            {
                Assert.Ignore("Pending");
            }
            else
            {
                const int errorCode = 1;
                string output = null;

                var result = await ProcessRunner.RunProcessAsync("dotnet", "lol", null, (data, proc) =>
                {
                    if (output == null) output = Encoding.GetEncoding(0).GetString(data);
                });

                Assert.IsTrue(result == errorCode);
                Assert.IsNotNull(output);
            }
        }

        [Test]
        public void GetCancellationAtRunProcessAsyncTest()
        {
            // I need a binary multiOS to run at loop
            Assert.Ignore("Pending");
        }
    }
}