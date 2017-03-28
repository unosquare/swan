using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Components;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ComponentsRetryTest
    {
        private static int count = 0;

        [Test]
        public async Task RetryTest()
        {
            var retryCount = 4;
            await Retry.Do(FailFunction, TimeSpan.FromSeconds(1), retryCount);
            Assert.IsTrue(count == retryCount);
        }

        internal async Task FailFunction()
        {
            count++;
            var token = await JsonClient.GetString("http://accesscore.azurewebsites.net/api/token");
        }
    }
}
