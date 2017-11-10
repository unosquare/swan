namespace Unosquare.Swan.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents an SMTP server response object
    /// </summary>
    public class SmtpServerReply
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpServerReply"/> class.
        /// </summary>
        /// <param name="responseCode">The response code.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The content.</param>
        public SmtpServerReply(int responseCode, string statusCode, params string[] content)
        {
            Content = new List<string>();
            ReplyCode = responseCode;
            EnhancedStatusCode = statusCode;
            Content.AddRange(content);
            IsValid = responseCode >= 200 && responseCode < 600;
            ReplyCodeSeverity = SmtpReplyCodeSeverities.Unknown;
            ReplyCodeCategory = SmtpReplyCodeCategories.Unknown;

            if (!IsValid) return;
            if (responseCode >= 200) ReplyCodeSeverity = SmtpReplyCodeSeverities.PositiveCompletion;
            if (responseCode >= 300) ReplyCodeSeverity = SmtpReplyCodeSeverities.PositiveIntermediate;
            if (responseCode >= 400) ReplyCodeSeverity = SmtpReplyCodeSeverities.TransientNegative;
            if (responseCode >= 500) ReplyCodeSeverity = SmtpReplyCodeSeverities.PermanentNegative;
            if (responseCode >= 600) ReplyCodeSeverity = SmtpReplyCodeSeverities.Unknown;

            if (int.TryParse(responseCode.ToString(CultureInfo.InvariantCulture).Substring(1, 1), out var middleDigit))
            {
                if (middleDigit >= 0 && middleDigit <= 5)
                    ReplyCodeCategory = (SmtpReplyCodeCategories) middleDigit;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpServerReply"/> class.
        /// </summary>
        public SmtpServerReply()
            : this(0, string.Empty, string.Empty)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpServerReply"/> class.
        /// </summary>
        /// <param name="responseCode">The response code.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The content.</param>
        public SmtpServerReply(int responseCode, string statusCode, string content)
            : this(responseCode, statusCode, new[] {content})
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpServerReply"/> class.
        /// </summary>
        /// <param name="responseCode">The response code.</param>
        /// <param name="content">The content.</param>
        public SmtpServerReply(int responseCode, string content)
            : this(responseCode, string.Empty, content)
        {
        }

        #endregion

        #region Pre-built responses (https://tools.ietf.org/html/rfc5321#section-4.2.2)

        /// <summary>
        /// Gets the command unrecognized reply.
        /// </summary>
        public static SmtpServerReply CommandUnrecognized =>
            new SmtpServerReply(500, "Syntax error, command unrecognized");

        /// <summary>
        /// Gets the syntax error arguments reply.
        /// </summary>
        public static SmtpServerReply SyntaxErrorArguments =>
            new SmtpServerReply(501, "Syntax error in parameters or arguments");

        /// <summary>
        /// Gets the command not implemented reply.
        /// </summary>
        public static SmtpServerReply CommandNotImplemented => new SmtpServerReply(502, "Command not implemented");

        /// <summary>
        /// Gets the bad sequence of commands reply.
        /// </summary>
        public static SmtpServerReply BadSequenceOfCommands => new SmtpServerReply(503, "Bad sequence of commands");

        /// <summary>
        /// Gets the protocol violation reply.
        /// </summary>=
        public static SmtpServerReply ProtocolViolation =>
            new SmtpServerReply(451, "Requested action aborted: error in processing");

        /// <summary>
        /// Gets the system status bye reply.
        /// </summary>
        public static SmtpServerReply SystemStatusBye =>
            new SmtpServerReply(221, "Service closing transmission channel");

        /// <summary>
        /// Gets the system status help reply.
        /// </summary>=
        public static SmtpServerReply SystemStatusHelp => new SmtpServerReply(221, "Refer to RFC 5321");

        /// <summary>
        /// Gets the bad syntax command empty reply.
        /// </summary>
        public static SmtpServerReply BadSyntaxCommandEmpty => new SmtpServerReply(400, "Error: bad syntax");

        /// <summary>
        /// Gets the OK reply.
        /// </summary>
        public static SmtpServerReply Ok => new SmtpServerReply(250, "OK");

        /// <summary>
        /// Gets the authorization required reply.
        /// </summary>
        public static SmtpServerReply AuthorizationRequired => new SmtpServerReply(530, "Authorization Required");

        #endregion

        #region Properties

        /// <summary>
        /// Gets the response severity.
        /// </summary>
        public SmtpReplyCodeSeverities ReplyCodeSeverity { get; }

        /// <summary>
        /// Gets the response category.
        /// </summary>
        public SmtpReplyCodeCategories ReplyCodeCategory { get; }

        /// <summary>
        /// Gets the numeric response code.
        /// </summary>
        public int ReplyCode { get; }

        /// <summary>
        /// Gets the enhanced status code.
        /// </summary>
        public string EnhancedStatusCode { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public List<string> Content { get; }

        /// <summary>
        /// Returns true if the response code is between 200 and 599
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is positive.
        /// </summary>
        public bool IsPositive => ReplyCode >= 200 && ReplyCode <= 399;

        #endregion

        #region Methods

        /// <summary>
        /// Parses the specified text into a Server Reply for thorough analysis.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>A new instance of SMTP server response object</returns>
        public static SmtpServerReply Parse(string text)
        {
            var lines = text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return new SmtpServerReply();

            var lastLineParts = lines.Last().Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            var enhancedStatusCode = string.Empty;
            int.TryParse(lastLineParts[0], out var responseCode);
            if (lastLineParts.Length > 1)
            {
                if (lastLineParts[1].Split('.').Length == 3)
                    enhancedStatusCode = lastLineParts[1];
            }

            var content = new List<string>();

            for (var i = 0; i < lines.Length; i++)
            {
                var splitChar = i == lines.Length - 1 ? " " : "-";

                var lineParts = lines[i].Split(new[] {splitChar}, 2, StringSplitOptions.None);
                var lineContent = lineParts.Last();
                if (string.IsNullOrWhiteSpace(enhancedStatusCode) == false)
                    lineContent = lineContent.Replace(enhancedStatusCode, string.Empty).Trim();

                content.Add(lineContent);
            }

            return new SmtpServerReply(responseCode, enhancedStatusCode, content.ToArray());
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var responseCodeText = ReplyCode.ToString(CultureInfo.InvariantCulture);
            var statusCodeText = string.IsNullOrWhiteSpace(EnhancedStatusCode)
                ? string.Empty
                : $" {EnhancedStatusCode.Trim()}";
            if (Content.Count == 0) return $"{responseCodeText}{statusCodeText}";

            var builder = new StringBuilder();

            for (var i = 0; i < Content.Count; i++)
            {
                var isLastLine = i == Content.Count - 1;

                builder.Append(isLastLine
                    ? $"{responseCodeText}{statusCodeText} {Content[i]}"
                    : $"{responseCodeText}-{Content[i]}\r\n");
            }

            return builder.ToString();
        }

        #endregion
    }
}