using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsExceptionMessageTest
    {
        [Test]
        public void ExceptionMessageTest()
        {
            try
            {
                throw new Exception("Random message");
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
            var exceptions = new List<Exception>
            {
                new TimeoutException("It timed out", new ArgumentException("ID missing")),
                new NotImplementedException("Somethings not implemented", new ArgumentNullException())
            };

            var ex = new AggregateException(exceptions);

            var exMsg = ex.ExceptionMessage();

            var lines = exMsg.Split(splits, StringSplitOptions.None);

            Assert.IsNotNull(exMsg);
            Assert.AreEqual(lines.Length - 1, ex.InnerExceptions.Count);
        }
    }
}
