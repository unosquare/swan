namespace Unosquare.Swan.Test.SmtpTests
{
    using Formatters;
    using Mocks;
    using Networking;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SmtpClientTest
    {
        public const string SenderEmail = "test@test.com";
        public const string RecipientEmail = "me@test.com";
        public const string EmailFile = "tempFile.msg";
        public const string Host = "smtp.gmail.com";
        public const string LocalHost = "localhost";
    }

    [TestFixture]
    public class SendMailAsync : SmtpClientTest
    {
        [Test]
        public void NullState_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var client = new SmtpClient(Host, 587);

                await client.SendMailAsync((SmtpSessionState)null);
            });
        }

        [Test]
        public void InvalidServer_ThrowsSocketException()
        {
            Assert.CatchAsync<System.Net.Sockets.SocketException>(async () =>
            {
                var client = new SmtpClient("invalid.local", 587);

                await client.SendMailAsync(new SmtpSessionState());
            });
        }

        [Test]
        public void NullStateEnumeration_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var client = new SmtpClient(Host, 587);
                IEnumerable<SmtpSessionState> sessions = null;

                await client.SendMailAsync(sessions);
            });
        }

        [Test]
        public async Task SendLocalEmail()
        {
            var filename = Path.Combine(Path.GetTempPath(), EmailFile);

            if (File.Exists(filename))
                File.Delete(filename);

            Assert.IsFalse(File.Exists(filename));
            var client = new SmtpClient(LocalHost, 1030) { Credentials = new System.Net.NetworkCredential("mail", "pass") };
            var session = new SmtpSessionState { SenderAddress = SenderEmail };

            session.Recipients.Add(RecipientEmail);
            session.DataBuffer.AddRange(System.Text.Encoding.ASCII.GetBytes("HH\r\n"));

            await client.SendMailAsync(session);
            await Task.Delay(100);
            Assert.IsTrue(File.Exists(filename));

            var smtpMock = Json.Deserialize<SmtpMock>(File.ReadAllText(filename));
            Assert.IsNotNull(smtpMock);

            Assert.AreEqual(SenderEmail, smtpMock.Envelope.MailFrom.Address);
            Assert.AreEqual(RecipientEmail, smtpMock.Envelope.RcptTo.First().Address);
        }

        [Test]
        public async Task SendLocalEmailWithMailMessage()
        {
            var filename = Path.Combine(Path.GetTempPath(), EmailFile);

            if (File.Exists(filename))
                File.Delete(filename);

            Assert.IsFalse(File.Exists(filename));
            var client = new SmtpClient(LocalHost, 1030) { Credentials = new System.Net.NetworkCredential("mail", "pass") };
            var emailMessage = new System.Net.Mail.MailMessage(SenderEmail, RecipientEmail, "Test", "Sure");

            await client.SendMailAsync(emailMessage);
            await Task.Delay(100);
            Assert.IsTrue(File.Exists(filename));

            var smtpMock = Json.Deserialize<SmtpMock>(File.ReadAllText(filename));
            Assert.IsNotNull(smtpMock);
            
            Assert.AreEqual(SenderEmail, smtpMock.Envelope.MailFrom.Address);
            Assert.AreEqual(RecipientEmail, smtpMock.Envelope.RcptTo.First().Address);
        }

        [Test]
        public async Task CancelSendEmail()
        {
            var filename = Path.Combine(Path.GetTempPath(), EmailFile);

            if (File.Exists(filename))
                File.Delete(filename);

            Assert.IsFalse(File.Exists(filename));
            var cts = new CancellationTokenSource();
            var email = new SmtpClient(LocalHost, 1030);
            var session = new SmtpSessionState { SenderAddress = SenderEmail };

            session.Recipients.Add(RecipientEmail);
            session.DataBuffer.AddRange(new byte[] { 0x48, 0x48, 0x0A, 0x0C });

#pragma warning disable 4014
            email.SendMailAsync(session, ct: cts.Token);
#pragma warning restore 4014
            cts.Cancel();
            await Task.Delay(100);
            Assert.IsFalse(File.Exists(filename));
        }
    }
}