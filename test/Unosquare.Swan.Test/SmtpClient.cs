using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SmtpClientntTest
    {
        [Test]
        public void TestConnectGmailSmtpException()
        {
#if NET452
            Assert.ThrowsAsync<System.Net.Mail.SmtpException>
#else
            Assert.ThrowsAsync<SmtpException>
#endif
            (async () =>
            {
                var client = new SmtpClient("smtp.gmail.com", 587);

                await client.SendMailAsync(new SmtpSessionState());
            });
        }
    }
}