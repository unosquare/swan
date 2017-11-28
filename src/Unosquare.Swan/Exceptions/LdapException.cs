#if !UWP
namespace Unosquare.Swan.Exceptions
{
    using System;
    using System.Linq;
    using Networking.Ldap;

    /// <summary>
    /// Thrown to indicate that an Ldap exception has occurred. This is a general
    /// exception which includes a message and an Ldap result code.
    /// An LdapException can result from physical problems (such as
    /// network errors) as well as problems with Ldap operations detected
    /// by the server. For example, if an Ldap add operation fails because of a
    /// duplicate entry, the server returns a result code.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class LdapException
        : Exception
    {
        internal const string UnexpectedEnd = "Unexpected end of filter";
        internal const string MissingLeftParen = "Unmatched parentheses, left parenthesis missing";
        internal const string MissingRightParen = "Unmatched parentheses, right parenthesis missing";
        internal const string ExpectingRightParen = "Expecting right parenthesis, found \"{0}\"";
        internal const string ExpectingLeftParen = "Expecting left parenthesis, found \"{0}\"";

        private readonly string _serverMessage;
        private string[] _referrals;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException" /> class.
        /// Constructs an exception with a detailed message obtained from the
        /// specified <c>MessageOrKey</c> String.
        /// Additional parameters specify the result code, the message returned
        /// from the server, and a matchedDN returned from the server.
        /// The String is used either as a message key to obtain a localized
        /// messsage from ExceptionMessages, or if there is no key in the
        /// resource matching the text, it is used as the detailed message itself.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="resultCode">The result code returned.</param>
        /// <param name="serverMsg">Error message specifying additional information
        /// from the server</param>
        /// <param name="matchedDN">The maximal subset of a specified DN which could
        /// be matched by the server on a search operation.</param>
        /// <param name="rootException">The root exception.</param>
        public LdapException(
            string message,
            LdapStatusCode resultCode,
            string serverMsg = null,
            string matchedDN = null,
            Exception rootException = null)
            : base(message)
        {
            ResultCode = resultCode;
            Cause = rootException;
            MatchedDN = matchedDN;
            _serverMessage = serverMsg;
        }

        /// <summary>
        ///     Returns the error message from the Ldap server, if this message is
        ///     available (that is, if this message was set). If the message was not set,
        ///     this method returns null.
        /// </summary>
        /// <returns>
        ///     The error message or null if the message was not set.
        /// </returns>
        public string LdapErrorMessage =>
            _serverMessage != null && _serverMessage.Length == 0 ? null : _serverMessage;

        /// <summary>
        /// Returns the lower level Exception which caused the failure, if any.
        /// For example, an IOException with additional information may be returned
        /// on a CONNECT_ERROR failure.
        /// </summary>
        /// <value>
        /// The cause.
        /// </value>
        public Exception Cause { get; }

        /// <summary>
        /// Returns the result code from the exception.
        /// The codes are defined as <c>public final static int</c> members
        /// of the Ldap Exception class. If the exception is a
        /// result of error information returned from a directory operation, the
        /// code will be one of those defined for the class. Otherwise, a local error
        /// code is returned.
        /// </summary>
        /// <value>
        /// The result code.
        /// </value>
        public LdapStatusCode ResultCode { get; }

        /// <summary>
        /// Returns the part of a submitted distinguished name which could be
        /// matched by the server.
        /// If the exception was caused by a local error, such as no server
        /// available, the return value is null. If the exception resulted from
        /// an operation being executed on a server, the value is an empty string
        /// except when the result of the operation was one of the following:
        /// <ul><li>NO_SUCH_OBJECT</li><li>ALIAS_PROBLEM</li><li>INVALID_DN_SYNTAX</li><li>ALIAS_DEREFERENCING_PROBLEM</li></ul>
        /// </summary>
        /// <value>
        /// The matched dn.
        /// </value>
        public string MatchedDN { get; }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message => ResultCode.ToString().Humanize();

        /// <summary>
        /// Returns a string of information about the exception and the
        /// the nested exceptions, if any.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // Craft a string from the resouce file
            var msg = $"{nameof(LdapException)}: {base.Message} ({ResultCode}) {ResultCode.ToString().Humanize()}";

            // Add server message
            if (!string.IsNullOrEmpty(_serverMessage))
            {
                msg += $"\r\nServer Message: {_serverMessage}";
            }

            // Add Matched DN message
            if (MatchedDN != null)
            {
                msg += $"\r\nMatched DN: {MatchedDN}";
            }

            if (Cause != null)
            {
                msg += $"\r\n{Cause}";
            }

            // Add referral information, display all the referrals in the list
            return _referrals != null
                ? _referrals.Aggregate(msg, (current, referral) => current + $"\r\nServer Message: {referral}")
                : msg;
        }

        internal void SetReferrals(string[] urls) => _referrals = urls;
    }
}
#endif