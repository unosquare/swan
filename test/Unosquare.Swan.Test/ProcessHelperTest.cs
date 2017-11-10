namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Unosquare.Swan.Components;

    [TestFixture]
    public class GetProcessOutputAsync
    {
        [Test]
        public async Task WithValidParams_ReturnsProcessOutput()
        {
            var data = await ProcessRunner.GetProcessOutputAsync("dotnet", "--help");
            Assert.IsNotEmpty(data);
            Assert.IsTrue(data.StartsWith(".NET Command Line Tools"));
        }
    }

    [TestFixture]
    public class RunProcessAsync
    {
        [Test]
        public async Task WithNullonErrorData_ValidRunProcess()
        {
            const int okCode = 0;
            string output = null;

            var result = await ProcessRunner.RunProcessAsync("dotnet", "--help", (data, proc) =>
            {
                if (output == null)
                    output = Encoding.GetEncoding(0).GetString(data);
            }, null);

            Assert.IsTrue(result == okCode);
            Assert.IsNotNull(output);
        }

        [Test]
        public async Task WithNullOnOutputData_InvalidRunProcess()
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
                    if (output == null)
                        output = Encoding.GetEncoding(0).GetString(data);
                });

                Assert.IsTrue(result == errorCode);
                Assert.IsNotNull(output);
            }
        }
    }
}