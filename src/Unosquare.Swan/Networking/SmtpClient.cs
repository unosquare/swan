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

        /// <summary>
        /// Parses and verifies the server reply. If the reply is not positive or cannot be parsed,
        /// it will throw an SmtpException
        /// </summary>
        /// <param name="replyText">The reply text.</param>
        /// <param name="sessionId">The session id.</param>
        /// <exception cref="SmtpException">Defines an SMTP Exceptions class</exception>
        private static void ValidateReply(string replyText, string sessionId)
        {
            if (replyText == null)
                throw new SmtpException("There was no response from the server");

            try
            {
                var response = SmtpServerReply.Parse(replyText);
                $"  RX {replyText} - {response.IsPositive}".Debug(typeof(SmtpClient), sessionId);

                if (response.IsPositive) return;

                var responseContent = string.Empty;
                if (response.Content.Count > 0)
                    responseContent = string.Join(";", response.Content.ToArray());

                throw new SmtpException((SmtpStatusCode) response.ReplyCode, responseContent);
            }
            catch
            {
                throw new SmtpException($"Could not parse server response: {replyText}");
            }
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Sends an email message asynchronously.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>A task that represents the asynchronous of send email operation</returns>
        public Task SendMailAsync(MailMessage message, string sessionId = null)
        {
            var state = new SmtpSessionState
            {
                AuthMode = Credentials == null ? string.Empty : SmtpDefinitions.SmtpAuthMethods.Login,
                ClientHostname = ClientHostname,
                IsChannelSecure = EnableSsl,
                SenderAddress = message.From.Address
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
        /// A task that represents the asynchronous of send email operation
        /// </returns>
        /// <exception cref="ArgumentNullException">sessionState</exception>
        public Task SendMailAsync(
            SmtpSessionState sessionState,
            string sessionId = null,
            CancellationToken ct = default(CancellationToken))
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
            CancellationToken ct = default(CancellationToken))
        {
            if (sessionStates == null)
                throw new ArgumentNullException(nameof(sessionStates));

            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(Host, Port);
                using (var connection = new Connection(tcpClient, Encoding.UTF8, "\r\n", true, 1000))
                {
                    string requestText = null;
                    string replyText = null;

                    try
                    {
                        // Read the greeting message
                        replyText = await connection.ReadLineAsync(ct);

                        // EHLO 1
                        requestText = $"{SmtpCommandNames.EHLO} {ClientHostname}";
                        $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                        await connection.WriteLineAsync(requestText, ct);
                        do
                        {
                            replyText = await connection.ReadLineAsync(ct);
                        } while (replyText.StartsWith("250 ") == false);

                        ValidateReply(replyText, sessionId);

                        // STARTTLS
                        if (EnableSsl)
                        {
                            requestText = $"{SmtpCommandNames.STARTTLS}";
                            $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                            await connection.WriteLineAsync(requestText, ct);
                            replyText = await connection.ReadLineAsync(ct);
                            ValidateReply(replyText, sessionId);

                            if (await connection.UpgradeToSecureAsClientAsync() == false)
                                throw new SecurityException("Could not upgrade the channel to SSL.");
                        }

                        {
                            // EHLO 2
                            requestText = $"{SmtpCommandNames.EHLO} {ClientHostname}";
                            $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                            await connection.WriteLineAsync(requestText, ct);
                            do
                            {
                                replyText = await connection.ReadLineAsync(ct);
                            } while (replyText.StartsWith("250 ") == false);

                            ValidateReply(replyText, sessionId);
                        }

                        // AUTH
                        if (Credentials != null)
                        {
                            requestText =
                                $"{SmtpCommandNames.AUTH} {SmtpDefinitions.SmtpAuthMethods.Login} {Convert.ToBase64String(Encoding.UTF8.GetBytes(Credentials.UserName))}";
                            $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                            await connection.WriteLineAsync(requestText, ct);
                            replyText = await connection.ReadLineAsync(ct);
                            ValidateReply(replyText, sessionId);
                            requestText = Convert.ToBase64String(Encoding.UTF8.GetBytes(Credentials.Password));
                            $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                            await connection.WriteLineAsync(requestText, ct);
                            replyText = await connection.ReadLineAsync(ct);
                            ValidateReply(replyText, sessionId);
                        }

                        foreach (var sessionState in sessionStates)
                        {
                            {
                                // MAIL FROM
                                requestText = $"{SmtpCommandNames.MAIL} FROM:<{sessionState.SenderAddress}>";
                                $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                                await connection.WriteLineAsync(requestText, ct);
                                replyText = await connection.ReadLineAsync(ct);
                                ValidateReply(replyText, sessionId);
                            }

                            // RCPT TO
                            foreach (var recipient in sessionState.Recipients)
                            {
                                requestText = $"{SmtpCommandNames.RCPT} TO:<{recipient}>";
                                $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                                await connection.WriteLineAsync(requestText, ct);
                                replyText = await connection.ReadLineAsync(ct);
                                ValidateReply(replyText, sessionId);
                            }

                            {
                                // DATA
                                requestText = $"{SmtpCommandNames.DATA}";
                                $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                                await connection.WriteLineAsync(requestText, ct);
                                replyText = await connection.ReadLineAsync(ct);
                                ValidateReply(replyText, sessionId);
                            }

                            {
                                // CONTENT
                                var dataTerminator = sessionState.DataBuffer.Skip(sessionState.DataBuffer.Count - 5)
                                    .ToArray().ToText();

                                requestText = $"Buffer ({sessionState.DataBuffer.Count} bytes)";
                                $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);

                                await connection.WriteDataAsync(sessionState.DataBuffer.ToArray(), true, ct);
                                if (dataTerminator.EndsWith(SmtpDefinitions.SmtpDataCommandTerminator) == false)
                                    await connection.WriteTextAsync(SmtpDefinitions.SmtpDataCommandTerminator, ct);

                                replyText = await connection.ReadLineAsync(ct);
                                ValidateReply(replyText, sessionId);
                            }
                        }

                        {
                            // QUIT
                            requestText = $"{SmtpCommandNames.QUIT}";
                            $"  TX {requestText}".Debug(typeof(SmtpClient), sessionId);
                            await connection.WriteLineAsync(requestText, ct);
                            replyText = await connection.ReadLineAsync(ct);
                            ValidateReply(replyText, sessionId);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Could not send email. {ex.Message}\r\n    Last Request: {requestText}\r\n    Last Reply: {replyText}";
                        errorMessage.Error(typeof(SmtpClient).FullName, sessionId);

                        throw new SmtpException(errorMessage);
                    }
                }
            }
        }
    }
}

#endif