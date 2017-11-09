namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

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
            catch (Exception ex)
            {
                var msg = ex.ExceptionMessage();
                Assert.IsNotNull(msg);
                Assert.AreEqual(msg, ex.Message);
            }
        }

        [Test]
        public void InnerExceptionTest()
        {
            string[] splits = {"\r\n"};
            var exceptions = new List<Exception>
            {
                new TimeoutException("It timed out", new ArgumentException("ID missing")),
                new NotImplementedException("Somethings not implemented", new ArgumentNullException())
            };

            var ex = new AggregateException(exceptions);

            var msg = ex.ExceptionMessage();
            Assert.IsNotNull(msg);

            var lines = msg.Split(splits, StringSplitOptions.None);
            Assert.AreEqual(lines.Length - 1, ex.InnerExceptions.Count);
        }
    }
}