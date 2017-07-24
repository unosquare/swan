using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

        [Test]
        public async Task SendLocalEmail()
        {
            var filename = Path.Combine(Path.GetTempPath(), "tempFile.msg");
            if (File.Exists(filename))
                File.Delete(filename);

            Assert.IsFalse(File.Exists(filename));
            var email = new SmtpClient("localhost", 1030);
            var session = new SmtpSessionState() {
                SenderAddress = "test@test.com",
                
            };
            session.Recipients.Add("test@test.com");
            session.DataBuffer.AddRange(new byte[] { 0x48, 0x48, 0x0A, 0x0C });

            await email.SendMailAsync(session);
            await Task.Delay(100);
            Assert.IsTrue(File.Exists(filename));
        }
    }
}