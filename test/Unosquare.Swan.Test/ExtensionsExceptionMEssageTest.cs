using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsExceptionMessageTest
    {
        [Test]
        public async Task ExceptionMessageTest()
        {
            try
            {
                var token = await JsonClient.GetString("https://accesscore.azurewebsites.net/api/token");
            }
            catch(Exception ex)
            {
                var exMsg = ex.ExceptionMessage();
                Assert.IsNotNull(exMsg);
                Assert.AreEqual(exMsg, ex.Message);
            }
        }

        [Test]
        public void InnerExceptionTest()
        {
            string[] splits = { "\r\n" };
            List<Exception> exceptions = new List<Exception>();
            exceptions.Add(new TimeoutException("It timed out", new ArgumentException("ID missing")));
            exceptions.Add(new NotImplementedException("Somethings not implemented", new ArgumentNullException()));
            AggregateException ex = new AggregateException(exceptions);

            var exMsg = ex.ExceptionMessage();

            string[] lines = exMsg.Split(splits, StringSplitOptions.None);

            Assert.IsNotNull(exMsg);
            Assert.AreEqual(lines.Count() - 1, ex.InnerExceptions.Count());
        }
    }
}
