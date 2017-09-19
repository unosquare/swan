#if NET452
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsWindowsServicesTest
    {
        [Test]
        public void RunInConsoleModeTest()
        {
            var service = new WinServiceMock();
            
            Task.Factory.StartNew(service.RunInConsoleMode);
            Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            service.Stop();
            Assert.GreaterOrEqual(service.Counter, 3);
        }
    }
}
#endif