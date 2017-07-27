using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class SmtpClientntTest
    {
        private const string SenderEmail = "test@test.com";
        private const string RecipientEmail = "me@test.com";
        private const string EmailFile = "tempFile.msg";

        [Test]
        public void TestConnectGmailSmtpException()
        {
#if NET452
            Assert.ThrowsAsync<System.Net.Mail.SmtpException>
#else
            Assert.ThrowsAsync<Unosquare.Swan.Exceptions.SmtpException>
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
            if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
                Assert.Inconclusive("Can not test in AppVeyor");

            var filename = Path.Combine(Path.GetTempPath(), EmailFile);

            if (File.Exists(filename))
                File.Delete(filename);

            Assert.IsFalse(File.Exists(filename));
            var email = new SmtpClient("localhost", 1030);
            var session = new SmtpSessionState {SenderAddress = SenderEmail};

            session.Recipients.Add(RecipientEmail);
            session.DataBuffer.AddRange(new byte[] {0x48, 0x48, 0x0A, 0x0C});

            await email.SendMailAsync(session);
            await Task.Delay(100);
            Assert.IsTrue(File.Exists(filename));

            var smtpMock = Json.Deserialize<SmtpMock>(File.ReadAllText(filename));
            Assert.IsNotNull(smtpMock);

            Assert.AreEqual(SenderEmail, smtpMock.envelope.from.address);
            Assert.AreEqual(RecipientEmail, smtpMock.envelope.to.First().address);

            Assert.AreEqual("hh", smtpMock.headers.First().Key);
        }

#if NET452
        [Test]
        public async Task SendLocalEmailWithMailMessage()
        {
            if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
                Assert.Inconclusive("Can not test in AppVeyor");

            var filename = Path.Combine(Path.GetTempPath(), EmailFile);

            if (File.Exists(filename))
                File.Delete(filename);

            Assert.IsFalse(File.Exists(filename));
            var email = new SmtpClient("localhost", 1030);
            var emailMessage = new System.Net.Mail.MailMessage(SenderEmail, RecipientEmail, "Test", "Sure");

            await email.SendMailAsync(emailMessage);
            await Task.Delay(100);
            Assert.IsTrue(File.Exists(filename));

            var smtpMock = Json.Deserialize<SmtpMock>(File.ReadAllText(filename));
            Assert.IsNotNull(smtpMock);

            Assert.AreEqual(SenderEmail, smtpMock.envelope.from.address);
            Assert.AreEqual(RecipientEmail, smtpMock.envelope.to.First().address);

            Assert.AreEqual("x-sender", smtpMock.headers.First().Key);
            Assert.AreEqual(SenderEmail, smtpMock.headers.First().Value);
        }
#endif

        [Test]
        public async Task CancelSendEmail()
        {
            if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
                Assert.Inconclusive("Can not test in AppVeyor");

            var filename = Path.Combine(Path.GetTempPath(), EmailFile);

            if (File.Exists(filename))
                File.Delete(filename);

            Assert.IsFalse(File.Exists(filename));
            var cts = new CancellationTokenSource();
            var email = new SmtpClient("localhost", 1030);
            var session = new SmtpSessionState {SenderAddress = SenderEmail};

            session.Recipients.Add(RecipientEmail);
            session.DataBuffer.AddRange(new byte[] {0x48, 0x48, 0x0A, 0x0C});

            email.SendMailAsync(session, ct: cts.Token);
            cts.Cancel();
            await Task.Delay(100);
            Assert.IsFalse(File.Exists(filename));
        }
    }
}