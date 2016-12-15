using NUnit.Framework;
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
            Assert.IsNotEmpty(data);
            Assert.IsTrue(data.StartsWith(".NET Command Line Tools"));
        }

        [Test]
        public async Task GetValidRunProcessAsyncTest()
        {
            const int okCode = 0;
            string output = null;

            var result = await ProcessHelper.RunProcessAsync("dotnet", "--help", (data, proc) =>
            {
                if (output == null) output = Encoding.GetEncoding(0).GetString(data);

            }, null, true, default(CancellationToken));

            Assert.IsTrue(result == okCode);
            Assert.IsNotNull(output);
        }
        
        [Test]
        public async Task GetInvalidRunProcessAsyncTest()
        {
            const int errorCode = 1;
            string output = null;

            var result = await ProcessHelper.RunProcessAsync("dotnet", "lol", null, (data, proc) =>
            {
                if (output == null) output = Encoding.GetEncoding(0).GetString(data);

            }, true, default(CancellationToken));

            Assert.IsTrue(result == errorCode);
            Assert.IsNotNull(output);
        }

        [Test]
        public void GetCancellationAtRunProcessAsyncTest()
        {
            Assert.Ignore("Pending");
        }
    }
}