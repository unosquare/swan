using Swan.Logging;
using System;
using System.Linq;
using System.Net.Mail;

namespace Swan.Net.Smtp
{
    /// <summary>
    /// Use this class to store the sender session data.
    /// </summary>
    internal class SmtpSender
    {
        private readonly string? _sessionId;
        private string? _requestText;

        public SmtpSender(string? sessionId)
        {
            _sessionId = sessionId;
        }

        public string? RequestText
        {
            get => _requestText;
            set
            {
                _requestText = value;
                $"  TX {_requestText}".Trace(typeof(SmtpClient), _sessionId);
            }
        }

        public string? ReplyText { get; set; }

        public bool IsReplyOk => ReplyText?.StartsWith("250 ", StringComparison.OrdinalIgnoreCase) == true;

        public void ValidateReply()
        {
            if (ReplyText == null)
                throw new SmtpException("There was no response from the server");

            try
            {
                var response = SmtpServerReply.Parse(ReplyText);
                $"  RX {ReplyText} - {response.IsPositive}".Trace(typeof(SmtpClient), _sessionId);

                if (response.IsPositive) return;

                var responseContent = response.Content.Any()
                    ? string.Join(";", response.Content.ToArray())
                    : string.Empty;

                throw new SmtpException((SmtpStatusCode)response.ReplyCode, responseContent);
            }
            catch (Exception ex)
            {
                if (!(ex is SmtpException))
                    throw new SmtpException($"Could not parse server response: {ReplyText}");
            }
        }
    }
}