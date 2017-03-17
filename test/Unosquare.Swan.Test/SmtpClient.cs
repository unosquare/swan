#if !NET452
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SnmpClieSmtpClientntTest
    {
        [Test]
        public void TestConnectGmailSmtpException()
        {
            // TODO: Make compatible with NET452
            Assert.ThrowsAsync<SmtpException>(async () =>
            {
                var client = new SmtpClient("smtp.gmail.com", 587);

                await client.SendMailAsync(new SmtpSessionState());
            });
        }
    }
}
#endif