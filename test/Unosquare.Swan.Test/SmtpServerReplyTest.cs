namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using Networking;

    [TestFixture]
    public class Constructor
    {
        [Test]
        public void StatusCode_ReturnZeroReplyCode()
        {
            var serverReply = new SmtpServerReply();

            Assert.IsNotNull(serverReply);
            Assert.AreEqual(0, serverReply.ReplyCode);
        }

        [Test]
        public void StatusCode_Return()
        {
            const string content = "Hello World!";
            const int status = 600; 

            var serverReply = new SmtpServerReply(status, content);

            Assert.IsNotNull(serverReply);
            Assert.AreEqual(status, serverReply.ReplyCode);
            Assert.AreEqual(content, serverReply.Content[0]);
        }
    }

    [TestFixture]
    public class PreBuildsResponses
    {
        private static readonly object[] ServerReplyCases =
        {
            new object[] { SmtpServerReply.CommandUnrecognized, 500, "Syntax error, command unrecognized" },
            new object[] { SmtpServerReply.SyntaxErrorArguments, 501, "Syntax error in parameters or arguments" },
            new object[] { SmtpServerReply.CommandNotImplemented, 502, "Command not implemented" },
            new object[] { SmtpServerReply.BadSequenceOfCommands, 503, "Bad sequence of commands" },
            new object[] { SmtpServerReply.ProtocolViolation, 451, "Requested action aborted: error in processing" },
            new object[] { SmtpServerReply.SystemStatusBye, 221, "Service closing transmission channel" },
            new object[] { SmtpServerReply.SystemStatusHelp, 221, "Refer to RFC 5321" },
            new object[] { SmtpServerReply.BadSyntaxCommandEmpty, 400, "Error: bad syntax" },
            new object[] { SmtpServerReply.Ok, 250, "OK" },
            new object[] { SmtpServerReply.AuthorizationRequired, 530, "Authorization Required" },
        };

        [Test]
        [TestCaseSource(nameof(ServerReplyCases))]
        public void CommandUnrecognized_SmtpServerReply(SmtpServerReply serverReply, int responseCode, string message)
        {
            Assert.IsNotNull(serverReply);
            Assert.AreEqual(responseCode, serverReply.ReplyCode);
            Assert.AreEqual(message, serverReply.Content[0]);
        }
    }

    [TestFixture]
    public class ToString
    {
        [Test]
        public void ReplyCode_ToString()
        {
            const string content = "Hello World!";
            const int status = 500;

            var serverReply = new SmtpServerReply(status, content);

            Assert.IsNotNull(serverReply);
            Assert.AreEqual($"{status} {content}", serverReply.ToString());
            Assert.AreEqual(content, serverReply.Content[0]);
        }
    }
}
