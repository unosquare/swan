#if !UWP
namespace Unosquare.Swan.Networking
{
    using System.Threading;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Generic;
#if !NETSTANDARD1_3
    using System.Net.Mail;
#else
    using Exceptions;
#endif

    /// <summary>
    /// Represents a basic SMTP client that is capable of submitting messages to an SMTP server.
    /// </summary>
    /// <example>
    /// The following code explains how to send a simple e-mail.
    /// <code>
    /// using System.Net.Mail;
    ///  
    /// class Example
    /// {
    ///     static void Main()
    ///     {
    ///         // create a new smtp client using google's smtp server
    ///         var client = new SmtpClient("smtp.gmail.com", 587);
    ///         
    ///         // send an email 
    ///         client.SendMailAsync(
    ///         new MailMessage("sender@test.com", "recipient@test.cm", "Subject", "Body"));
    ///     }
    /// }
    /// </code>
    /// The following code demonstrates how to sent an e-mail using a SmtpSessionState
    /// <code>
    /// using System.Net.Mail;
    ///  
    /// class Example
    /// {
    ///     static void Main()
    ///     {
    ///         // create a new smtp client using google's smtp server
    ///         var client = new SmtpClient("smtp.gmail.com", 587);
    ///         
    ///         // create a new session state with a sender address 
    ///         var session = new SmtpSessionState { SenderAddress = "sender@test.com" };
    ///         
    ///         // add a recipient
    ///         session.Recipients.Add("recipient@test.cm");
    ///         
    ///         // send
    ///         client.SendMailAsync(session);
    ///     }
    /// }
    /// </code>
    /// The following code shows how to send an e-mail with an attachment
    /// <code>
    /// using System.Net.Mail;
    ///  
    /// class Example
    /// {
    ///     static void Main()
    ///     {
    ///         // create a new smtp client using google's smtp server
    ///         var client = new SmtpClient("smtp.gmail.com", 587);
    ///         
    ///         // create a new session state with a sender address 
    ///         var session = new SmtpSessionState { SenderAddress = "sender@test.com" };
    ///         
    ///         // add a recipient
    ///         session.Recipients.Add("recipient@test.cm");
    ///         
    ///         // load a file as an attachment
    ///         var attachment = new MimePart("image", "gif")
    ///         {
    ///             Content = new 
    ///                 MimeContent(File.OpenRead("meme.gif"), ContentEncoding.Default),
    ///             ContentDisposition = 
    ///                 new ContentDisposition(ContentDisposition.Attachment),
    ///             ContentTransferEncoding = ContentEncoding.Base64,
    ///             FileName = Path.GetFileName("meme.gif")
    ///         };
    ///         
    ///         // send
    ///         client.SendMailAsync(session);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class SmtpClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpClient" /> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <exception cref="ArgumentNullException">host</exception>
        public SmtpClient(string host, int port)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
            ClientHostname = Network.HostName;
        }

        /// <summary>
        /// Gets or sets the credentials. No credentials will be used if set to null.
        /// </summary>
        /// <value>
        /// The credentials.
        /// </value>
        public NetworkCredential Credentials { get; set; }

        /// <summary>
        /// Gets or sets the hostname to connect to.
        /// </summary>
        /// <value>
        /// The host.
        /// </value>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port on which the server expects the connection.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SSL is enabled.
        /// If set to false, communication between client and server will not be secured.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable SSL]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets the name of the client that gets announced to the server.
        /// </summary>
        /// <value>
        /// The client hostname.
        /// </value>
        public string ClientHostname { get; set; }
        
#if !NETSTANDARD1_3
        /// <summary>
        /// Sends an email message asynchronously.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A task that represents the asynchronous of send email operation.</returns>
        public Task SendMailAsync(MailMessage message, string sessionId = null)
        {
            var state = new SmtpSessionState
            {
                AuthMode = Credentials == null ? string.Empty : SmtpDefinitions.SmtpAuthMethods.Login,
                ClientHostname = ClientHostname,
                IsChannelSecure = EnableSsl,
                SenderAddress = message.From.Address,
            };

            if (Credentials != null)
            {
                state.Username = Credentials.UserName;
                state.Password = Credentials.Password;
            }

            foreach (var recipient in message.To)
            {
                state.Recipients.Add(recipient.Address);
            }

            state.DataBuffer.AddRange(message.ToMimeMessage().ToArray());

            return SendMailAsync(state, sessionId);
        }
#endif

        /// <summary>
        /// Sends an email message using a session state object.
        /// Credentials, Enable SSL and Client Hostname are NOT taken from the state object but
        /// rather from the properties of this class.
        /// </summary>
        /// <param name="sessionState">The state.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous of send email operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">sessionState</exception>
        public Task SendMailAsync(
            SmtpSessionState sessionState,
            string sessionId = null,
            CancellationToken ct = default)
        {
            if (sessionState == null)
                throw new ArgumentNullException(nameof(sessionState));

            $"Sending new email from {sessionState.SenderAddress} to {string.Join(";", sessionState.Recipients)}".Info(
                typeof(SmtpClient));
            return SendMailAsync(new[] {sessionState}, sessionId, ct);
        }

        /// <summary>
        /// Sends an array of email messages using a session state object.
        /// Credentials, Enable SSL and Client Hostname are NOT taken from the state object but
        /// rather from the properties of this class.
        /// </summary>
        /// <param name="sessionStates">The session states.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous of send email operation
        /// </returns>
        /// <exception cref="ArgumentNullException">sessionStates</exception>
        /// <exception cref="SecurityException">Could not upgrade the channel to SSL.</exception>
        /// <exception cref="SmtpException">Defines an SMTP Exceptions class</exception>
        public async Task SendMailAsync(
            IEnumerable<SmtpSessionState> sessionStates,
            string sessionId = null,
            CancellationToken ct = default)
        {
            if (sessionStates == null)
                throw new ArgumentNullException(nameof(sessionStates));

            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(Host, Port);
                using (var connection = new Connection(tcpClient, Encoding.UTF8, "\r\n", true, 1000))
                {
                    var sender = new SmtpSender(sessionId);

                    try
                    {
                        // Read the greeting message
                        sender.ReplyText = await connection.ReadLineAsync(ct);

                        // EHLO 1
                        sender.RequestText = $"{SmtpCommandNames.EHLO} {ClientHostname}";

                        await connection.WriteLineAsync(sender.RequestText, ct);
                        do
                        {
                            sender.ReplyText = await connection.ReadLineAsync(ct);
                        } 
                        while (sender.ReplyText.StartsWith("250 ") == false);

                        sender.ValidateReply();

                        // STARTTLS
                        if (EnableSsl)
                        {
                            sender.RequestText = $"{SmtpCommandNames.STARTTLS}";

                            await connection.WriteLineAsync(sender.RequestText, ct);
                            sender.ReplyText = await connection.ReadLineAsync(ct);
                            sender.ValidateReply();

                            if (await connection.UpgradeToSecureAsClientAsync() == false)
                                throw new SecurityException("Could not upgrade the channel to SSL.");
                        }

                        {
                            // EHLO 2
                            sender.RequestText = $"{SmtpCommandNames.EHLO} {ClientHostname}";

                            await connection.WriteLineAsync(sender.RequestText, ct);
                            do
                            {
                                sender.ReplyText = await connection.ReadLineAsync(ct);
                            } 
                            while (sender.ReplyText.StartsWith("250 ") == false);

                            sender.ValidateReply();
                        }

                        // AUTH
                        if (Credentials != null)
                        {
                            var auth = new ConnectionAuth(connection, sender, Credentials);
                            await auth.AuthenticateAsync(ct);
                        }

                        foreach (var sessionState in sessionStates)
                        {
                            {
                                // MAIL FROM
                                sender.RequestText = $"{SmtpCommandNames.MAIL} FROM:<{sessionState.SenderAddress}>";
                                
                                await connection.WriteLineAsync(sender.RequestText, ct);
                                sender.ReplyText = await connection.ReadLineAsync(ct);
                                sender.ValidateReply();
                            }

                            // RCPT TO
                            foreach (var recipient in sessionState.Recipients)
                            {
                                sender.RequestText = $"{SmtpCommandNames.RCPT} TO:<{recipient}>";
                                
                                await connection.WriteLineAsync(sender.RequestText, ct);
                                sender.ReplyText = await connection.ReadLineAsync(ct);
                                sender.ValidateReply();
                            }

                            {
                                // DATA
                                sender.RequestText = $"{SmtpCommandNames.DATA}";
                                
                                await connection.WriteLineAsync(sender.RequestText, ct);
                                sender.ReplyText = await connection.ReadLineAsync(ct);
                                sender.ValidateReply();
                            }

                            {
                                // CONTENT
                                var dataTerminator = sessionState.DataBuffer.Skip(sessionState.DataBuffer.Count - 5)
                                    .ToArray().ToText();

                                sender.RequestText = $"Buffer ({sessionState.DataBuffer.Count} bytes)";
                                
                                await connection.WriteDataAsync(sessionState.DataBuffer.ToArray(), true, ct);
                                if (dataTerminator.EndsWith(SmtpDefinitions.SmtpDataCommandTerminator) == false)
                                    await connection.WriteTextAsync(SmtpDefinitions.SmtpDataCommandTerminator, ct);

                                sender.ReplyText = await connection.ReadLineAsync(ct);
                                sender.ValidateReply();
                            }
                        }

                        {
                            // QUIT
                            sender.RequestText = $"{SmtpCommandNames.QUIT}";
                            
                            await connection.WriteLineAsync(sender.RequestText, ct);
                            sender.ReplyText = await connection.ReadLineAsync(ct);
                            sender.ValidateReply();
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Could not send email. {ex.Message}\r\n    Last Request: {sender.RequestText}\r\n    Last Reply: {sender.ReplyText}";
                        errorMessage.Error(typeof(SmtpClient).FullName, sessionId);

                        throw new SmtpException(errorMessage);
                    }
                }
            }
        }

        private sealed class SmtpSender
        {
            private readonly string _sessionId;
            private string _requestText;

            public SmtpSender(string sessionId)
            {
                _sessionId = sessionId;
            }

            public string RequestText
            {
                get => _requestText;
                set
                {
                    _requestText = value;
                    $"  TX {_requestText}".Debug(typeof(SmtpClient), _sessionId);
                }
            }

            public string ReplyText { get; set; }

            public void ValidateReply()
            {
                if (ReplyText == null)
                    throw new SmtpException("There was no response from the server");

                try
                {
                    var response = SmtpServerReply.Parse(ReplyText);
                    $"  RX {ReplyText} - {response.IsPositive}".Debug(typeof(SmtpClient), _sessionId);

                    if (response.IsPositive) return;

                    var responseContent = string.Empty;
                    if (response.Content.Count > 0)
                        responseContent = string.Join(";", response.Content.ToArray());

                    throw new SmtpException((SmtpStatusCode) response.ReplyCode, responseContent);
                }
                catch
                {
                    throw new SmtpException($"Could not parse server response: {ReplyText}");
                }
            }
        }

        private class ConnectionAuth
        {
            private readonly SmtpSender _sender;
            private readonly Connection _connection;
            private readonly NetworkCredential _credentials;

            public ConnectionAuth(Connection connection, SmtpSender sender, NetworkCredential credentials)
            {
                _connection = connection;
                _sender = sender;
                _credentials = credentials;
            }

            public async Task AuthenticateAsync(CancellationToken ct)
            {
                _sender.RequestText =
                    $"{SmtpCommandNames.AUTH} {SmtpDefinitions.SmtpAuthMethods.Login} {Convert.ToBase64String(Encoding.UTF8.GetBytes(_credentials.UserName))}";

                await _connection.WriteLineAsync(_sender.RequestText, ct);
                _sender.ReplyText = await _connection.ReadLineAsync(ct);
                _sender.ValidateReply();
                _sender.RequestText = Convert.ToBase64String(Encoding.UTF8.GetBytes(_credentials.Password));

                await _connection.WriteLineAsync(_sender.RequestText, ct);
                _sender.ReplyText = await _connection.ReadLineAsync(ct);
                _sender.ValidateReply();
            }
        }
    }
}

#endif