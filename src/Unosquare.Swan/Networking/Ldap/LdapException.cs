#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Linq;
    
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

        private readonly LdapStatusCode _resultCode;
        private readonly string _matchedDn;
        private readonly Exception _rootException;
        private readonly string _serverMessage;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException" /> class.
        /// Constructs an exception with a detailed message obtained from the
        /// specified <code>MessageOrKey</code> String.
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
            _resultCode = resultCode;
            _rootException = rootException;
            _matchedDn = matchedDN;
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
        public virtual string LdapErrorMessage => _serverMessage != null && _serverMessage.Length == 0 ? null : _serverMessage;

        /// <summary>
        ///     Returns the lower level Exception which caused the failure, if any.
        ///     For example, an IOException with additional information may be returned
        ///     on a CONNECT_ERROR failure.
        /// </summary>
        public virtual Exception Cause => _rootException;

        /// <summary>
        ///     Returns the result code from the exception.
        ///     The codes are defined as <code>public final static int</code> members
        ///     of the Ldap Exception class. If the exception is a
        ///     result of error information returned from a directory operation, the
        ///     code will be one of those defined for the class. Otherwise, a local error
        ///     code is returned.
        /// </summary>
        public virtual LdapStatusCode ResultCode => _resultCode;

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
        public virtual string MatchedDN => _matchedDn;

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message => _resultCode.ToString().Humanize();

        /// <summary>
        /// Returns a string of information about the exception and the
        /// the nested exceptions, if any.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => GetExceptionString(nameof(LdapException));

        /// <summary>
        /// builds a string of information about the exception and the
        /// the nested exceptions, if any.
        /// </summary>
        /// <param name="exception">The name of the exception class</param>
        /// <returns>Strings representing the exception</returns>
        internal string GetExceptionString(string exception)
        {
            // Craft a string from the resouce file
            var msg = $"{exception}: {base.Message} ({_resultCode}) {_resultCode.ToString().Humanize()}";
            
            // Add server message
            if (!string.IsNullOrEmpty(_serverMessage))
            {
                msg += $"\n{exception}: Server Message: {_serverMessage}";
            }

            // Add Matched DN message
            if (_matchedDn != null)
            {
                msg += $"\n{exception}: Matched DN: {_matchedDn}";
            }

            if (_rootException != null)
            {
                msg += "\n" + _rootException;
            }

            return msg;
        }
    }

    /// <summary>
    /// Thrown when a server returns a referral and when a referral has not
    /// been followed.  It contains a list of URL strings corresponding
    /// to the referrals or search continuation references received on an Ldap
    /// operation.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapException" />
    public sealed class LdapReferralException : LdapException
    {
        private string[] _referrals;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException" /> class.
        /// Constructs an exception with a specified error string, result code,
        /// an error message from the server, and an exception that indicates
        /// a failure to follow a referral.
        /// </summary>
        /// <param name="message">The additional error information.</param>
        /// <param name="resultCode">The result code returned.</param>
        /// <param name="serverMessage">Error message specifying additional information
        /// from the server.</param>
        /// <param name="rootException">The root exception.</param>
        public LdapReferralException(
            string message, 
            LdapStatusCode resultCode, 
            string serverMessage,
            Exception rootException = null)
            : base(message, resultCode, serverMessage, rootException: rootException)
        {
        }
        
        /// <summary>
        /// Sets a referral that could not be processed
        /// </summary>
        /// <value>
        /// The failed referral.
        /// </value>
        public string FailedReferral { get; set; }

        /// <summary>
        /// returns a string of information about the exception and the
        /// the nested exceptions, if any.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // Format the basic exception information
            var msg = GetExceptionString(nameof(LdapReferralException));

            // Add failed referral information
            if (FailedReferral != null)
            {
                msg += $"\nServer Message: {FailedReferral}";
            }

            // Add referral information, display all the referrals in the list
            return _referrals != null
                ? _referrals.Aggregate(msg, (current, referral) => current + $"\nServer Message: {referral}")
                : msg;
        }
        
        internal void SetReferrals(string[] urls) => _referrals = urls;
    }
}
#endif