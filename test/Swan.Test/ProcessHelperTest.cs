namespace Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    [TestFixture]
    public class GetProcessOutputAsync
    {
        [Test]
        public void WithInValidParams_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await ProcessRunner.GetProcessOutputAsync(null));
        }

        [Test]
        public async Task WithValidParams_ReturnsProcessOutput()
        {
            var data = await ProcessRunner.GetProcessOutputAsync("dotnet", "--help");
            Assert.IsNotEmpty(data);
            Console.Write(data);
            Assert.IsTrue(data.StartsWith(".NET Command Line Tools"));
        }

        [Test]
        public async Task WithValidParamsAndTempDirectory_ReturnsProcessOutput()
        {
            var data = await ProcessRunner.GetProcessOutputAsync("dotnet", "--help", System.IO.Path.GetTempPath());
            Assert.IsNotEmpty(data);
            Assert.IsTrue(data.StartsWith(".NET Command Line Tools"));
        }
        
        [Test]
        public async Task WithInvalidParams_ReturnsProcessError()
        {
            var data = await ProcessRunner.GetProcessOutputAsync("dotnet", "lol");
            Assert.IsNotEmpty(data);
            Assert.IsTrue(data.StartsWith("No executable found"));
        }
    }

    [TestFixture]
    public class GetProcessEncodedOutputAsync
    {
        [Test]
        public void WithInValidParams_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await ProcessRunner.GetProcessEncodedOutputAsync(null));
        }

        [Test]
        public async Task WithValidParams_ReturnsProcessOutput()
        {
            var data = await ProcessRunner.GetProcessEncodedOutputAsync("dotnet", "--help", Encoding.UTF8);
            Assert.IsNotEmpty(data);
            Assert.IsTrue(data.StartsWith(".NET Command Line Tools"));
        }
    }

    [TestFixture]
    public class RunProcessAsync
    {
        [Test]
        public void WithInValidParams_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await ProcessRunner.RunProcessAsync(null, null, null, null));
        }

        [Test]
        public async Task WithNullOnErrorData_ValidRunProcess()
        {
            const int resultCode = 0;
            string output = null;

            var result = await ProcessRunner.RunProcessAsync(
                "dotnet",
                "--help",
                (data, proc) =>
                {
                    if (output == null)
                        output = Encoding.GetEncoding(0).GetString(data);
                },
                null);

            Assert.IsTrue(result == resultCode);
            Assert.IsNotNull(output);
        }

        [Test]
        public async Task WithNullOnOutputData_InvalidRunProcess()
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