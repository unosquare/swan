#if !NETSTANDARD1_3
namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;
    using Mocks;

    [TestFixture]
    public class ExtensionsWindowsServicesTest
    {
        [Test]
        public void RunInConsoleModeTest()
        {
            Assert.Ignore("Rewrite test, RunInConsoleMode will not wait for start");

            var service = new WinServiceMock();
            
            Task.Factory.StartNew(service.RunInConsoleMode);
            Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            service.Stop();
            Assert.GreaterOrEqual(service.Counter, 3);
        }
    }
}
#endif