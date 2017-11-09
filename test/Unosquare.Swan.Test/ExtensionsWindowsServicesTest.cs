#if NET452
namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;
    using Unosquare.Swan.Test.Mocks;

    [TestFixture]
    public class ExtensionsWindowsServicesTest
    {
        [Test]
        public void RunInConsoleModeTest()
        {
            Assert.Ignore("Rewrite test");

            var service = new WinServiceMock();
            
            Task.Factory.StartNew(service.RunInConsoleMode);
            Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            service.Stop();
            Assert.GreaterOrEqual(service.Counter, 3);
        }
    }
}
#endif