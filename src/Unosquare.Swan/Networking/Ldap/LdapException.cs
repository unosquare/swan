#if !UWP

namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an Ldap exception that is not a result of a server response.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapException" />
    public sealed class LdapLocalException : LdapException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapLocalException"/> class.
        /// </summary>
        public LdapLocalException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapLocalException"/> class.
        /// Constructs a local exception with a detailed message obtained from the
        /// specified <code>MessageOrKey</code> String.
        /// Additional parameters specify the result code and a rootException which
        /// is the underlying cause of an error on the client.
        /// The String is used either as a message key to obtain a localized
        /// message from ExceptionMessages, or if there is no key in the
        /// resource matching the text, it is used as the detailed message itself.
        /// </summary>
        /// <param name="messageOrKey">Key to addition result information, a key into
        /// ExceptionMessages, or the information
        /// itself if the key doesn't exist.</param>
        /// <param name="resultCode">The result code returned.</param>
        /// <param name="rootException">A throwable which is the underlying cause
        /// of the LdapException.</param>
        public LdapLocalException(string messageOrKey, LdapStatusCode resultCode, Exception rootException = null)
            : base(messageOrKey, resultCode, rootException: rootException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapLocalException"/> class.
        /// Constructs a local exception with a detailed message obtained from the
        /// specified <code>MessageOrKey</code> String and modifying arguments.
        /// Additional parameters specify the result code
        /// and a rootException which is the underlying cause of an error
        /// on the client.
        /// The String is used either as a message key to obtain a localized
        /// messsage from ExceptionMessages, or if there is no key in the
        /// resource matching the text, it is used as the detailed message itself.
        /// The message in the default locale is built with the supplied arguments,
        /// which are saved to be used for building messages for other locales.
        /// </summary>
        /// <param name="messageOrKey">Key to addition result information, a key into
        /// ExceptionMessages, or the information
        /// itself if the key doesn't exist.</param>
        /// <param name="arguments">The modifying arguments to be included in the
        /// message string.</param>
        /// <param name="resultCode">The result code returned.</param>
        /// <param name="rootException">A throwable which is the underlying cause
        /// of the LdapException.</param>
        public LdapLocalException(string messageOrKey, object[] arguments, LdapStatusCode resultCode,
            Exception rootException = null)
            : base(messageOrKey, arguments, resultCode, rootException: rootException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapLocalException"/> class.
        /// </summary>
        /// <param name="messageOrKey">The message or key.</param>
        /// <param name="argument">The argument.</param>
        /// <param name="resultCode">The result code.</param>
        /// <param name="rootException">The root exception.</param>
        public LdapLocalException(string messageOrKey, object argument, LdapStatusCode resultCode,
            Exception rootException = null)
            : base(messageOrKey, new[] { argument}, resultCode, rootException: rootException)
        {
        }

        /// <summary>
        /// Returns a string of information about the exception and the
        /// the nested exceptions, if any.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => GetExceptionString("LdapLocalException");
    }

    /// <summary>
    /// This class contains strings that may be associated with Exceptions generated
    /// by the Ldap API libraries.
    /// Two entries are made for each message, a String identifier, and the
    /// actual error string.  Parameters are identified as {0}, {1}, etc.
    /// </summary>
    public static class ExceptionMessages
    {
        // static strings to aide lookup and guarantee accuracy:
        // DO NOT include these strings in other Locales
        public const string TOSTRING = "TOSTRING";

        public const string SERVER_MSG = "SERVER_MSG";
        public const string MATCHED_DN = "MATCHED_DN";
        public const string FAILED_REFERRAL = "FAILED_REFERRAL";
        public const string REFERRAL_ITEM = "REFERRAL_ITEM";
        public const string CONNECTION_ERROR = "CONNECTION_ERROR";
        public const string CONNECTION_IMPOSSIBLE = "CONNECTION_IMPOSSIBLE";
        public const string CONNECTION_WAIT = "CONNECTION_WAIT";
        public const string CONNECTION_FINALIZED = "CONNECTION_FINALIZED";
        public const string CONNECTION_CLOSED = "CONNECTION_CLOSED";
        public const string CONNECTION_READER = "CONNECTION_READER";
        public const string DUP_ERROR = "DUP_ERROR";
        public const string REFERRAL_ERROR = "REFERRAL_ERROR";
        public const string REFERRAL_LOCAL = "REFERRAL_LOCAL";
        public const string REFERENCE_ERROR = "REFERENCE_ERROR";
        public const string REFERRAL_SEND = "REFERRAL_SEND";
        public const string REFERENCE_NOFOLLOW = "REFERENCE_NOFOLLOW";
        public const string REFERRAL_BIND = "REFERRAL_BIND";
        public const string REFERRAL_BIND_MATCH = "REFERRAL_BIND_MATCH";
        public const string NO_DUP_REQUEST = "NO_DUP_REQUEST";
        public const string SERVER_CONNECT_ERROR = "SERVER_CONNECT_ERROR";
        public const string NO_SUP_PROPERTY = "NO_SUP_PROPERTY";
        public const string ENTRY_PARAM_ERROR = "ENTRY_PARAM_ERROR";
        public const string DN_PARAM_ERROR = "DN_PARAM_ERROR";
        public const string RDN_PARAM_ERROR = "RDN_PARAM_ERROR";
        public const string OP_PARAM_ERROR = "OP_PARAM_ERROR";
        public const string PARAM_ERROR = "PARAM_ERROR";
        public const string DECODING_ERROR = "DECODING_ERROR";
        public const string ENCODING_ERROR = "ENCODING_ERROR";
        public const string IO_EXCEPTION = "IO_EXCEPTION";
        public const string INVALID_ESCAPE = "INVALID_ESCAPE";
        public const string SHORT_ESCAPE = "SHORT_ESCAPE";
        public const string INVALID_CHAR_IN_FILTER = "INVALID_CHAR_IN_FILTER";
        public const string INVALID_CHAR_IN_DESCR = "INVALID_CHAR_IN_DESCR";
        public const string INVALID_ESC_IN_DESCR = "INVALID_ESC_IN_DESCR";
        public const string UNEXPECTED_END = "UNEXPECTED_END";
        public const string MISSING_LEFT_PAREN = "MISSING_LEFT_PAREN";
        public const string MISSING_RIGHT_PAREN = "MISSING_RIGHT_PAREN";
        public const string EXPECTING_RIGHT_PAREN = "EXPECTING_RIGHT_PAREN";
        public const string EXPECTING_LEFT_PAREN = "EXPECTING_LEFT_PAREN";
        public const string NO_OPTION = "NO_OPTION";
        public const string INVALID_FILTER_COMPARISON = "INVALID_FILTER_COMPARISON";
        public const string NO_MATCHING_RULE = "NO_MATCHING_RULE";
        public const string NO_ATTRIBUTE_NAME = "NO_ATTRIBUTE_NAME";
        public const string NO_DN_NOR_MATCHING_RULE = "NO_DN_NOR_MATCHING_RULE";
        public const string NOT_AN_ATTRIBUTE = "NOT_AN_ATTRIBUTE";
        public const string UNEQUAL_LENGTHS = "UNEQUAL_LENGTHS";
        public const string IMPROPER_REFERRAL = "IMPROPER_REFERRAL";
        public const string NOT_IMPLEMENTED = "NOT_IMPLEMENTED";
        public const string NO_MEMORY = "NO_MEMORY";
        public const string SERVER_SHUTDOWN_REQ = "SERVER_SHUTDOWN_REQ";
        public const string INVALID_ADDRESS = "INVALID_ADDRESS";
        public const string UNKNOWN_RESULT = "UNKNOWN_RESULT";
        public const string OUTSTANDING_OPERATIONS = "OUTSTANDING_OPERATIONS";
        public const string WRONG_FACTORY = "WRONG_FACTORY";
        public const string NO_TLS_FACTORY = "NO_TLS_FACTORY";
        public const string NO_STARTTLS = "NO_STARTTLS";
        public const string STOPTLS_ERROR = "STOPTLS_ERROR";
        public const string MULTIPLE_SCHEMA = "MULTIPLE_SCHEMA";
        public const string NO_SCHEMA = "NO_SCHEMA";
        public const string READ_MULTIPLE = "READ_MULTIPLE";
        public const string CANNOT_BIND = "CANNOT_BIND";
        public const string SSL_PROVIDER_MISSING = "SSL_PROVIDER_MISSING";

        internal static readonly Dictionary<string, string> MessageMap = new Dictionary<string, string>
        {
            {TOSTRING, "{0}: {1} ({2}) {3}"},
            {SERVER_MSG, "{0}: Server Message: {1}"},
            {MATCHED_DN, "{0}: Matched DN: {1}"},
            {FAILED_REFERRAL, "{0}: Failed Referral: {1}"},
            {REFERRAL_ITEM, "{0}: Referral: {1}"},
            {CONNECTION_ERROR, "Unable to connect to server {0}:{1}"},
            {CONNECTION_IMPOSSIBLE, "Unable to reconnect to server, application has never called connect()"},
            {CONNECTION_WAIT, "Connection lost waiting for results from {0}:{1}"},
            {CONNECTION_FINALIZED, "Connection closed by the application finalizing the object"},
            {CONNECTION_CLOSED, "Connection closed by the application disconnecting"},
            {CONNECTION_READER, "Reader thread terminated"},
            {DUP_ERROR, "RfcLdapMessage: Cannot duplicate message built from the input stream"},
            {REFERENCE_ERROR, "Error attempting to follow a search continuation reference"},
            {REFERRAL_ERROR, "Error attempting to follow a referral"},
            {REFERRAL_LOCAL, "LdapSearchResults.{0}(): No entry found & request is not complete"},
            {REFERRAL_SEND, "Error sending request to referred server"},
            {REFERENCE_NOFOLLOW, "Search result reference received, and referral following is off"},
            {REFERRAL_BIND, "LdapBind.bind() function returned null"},
            {REFERRAL_BIND_MATCH, "Could not match LdapBind.bind() connection with Server Referral URL list"},
            {NO_DUP_REQUEST, "Cannot duplicate message to follow referral for {0} request, not allowed"},
            {SERVER_CONNECT_ERROR, "Error connecting to server {0} while attempting to follow a referral"},
            {NO_SUP_PROPERTY, "Requested property is not supported."},
            {ENTRY_PARAM_ERROR, "Invalid Entry parameter"},
            {DN_PARAM_ERROR, "Invalid DN parameter"},
            {RDN_PARAM_ERROR, "Invalid DN or RDN parameter"},
            {OP_PARAM_ERROR, "Invalid extended operation parameter, no OID specified"},
            {PARAM_ERROR, "Invalid parameter"},
            {DECODING_ERROR, "Error Decoding responseValue"},
            {ENCODING_ERROR, "Encoding Error"},
            {IO_EXCEPTION, "I/O Exception on host {0}, port {1}"},
            {INVALID_ESCAPE, "Invalid value in escape sequence \"{0}\""},
            {SHORT_ESCAPE, "Incomplete escape sequence"},
            {UNEXPECTED_END, "Unexpected end of filter"},
            {MISSING_LEFT_PAREN, "Unmatched parentheses, left parenthesis missing"},
            {NO_OPTION, "Semicolon present, but no option specified"},
            {MISSING_RIGHT_PAREN, "Unmatched parentheses, right parenthesis missing"},
            {EXPECTING_RIGHT_PAREN, "Expecting right parenthesis, found \"{0}\""},
            {EXPECTING_LEFT_PAREN, "Expecting left parenthesis, found \"{0}\""},
            {NO_ATTRIBUTE_NAME, "Missing attribute description"},
            {NO_DN_NOR_MATCHING_RULE, "DN and matching rule not specified"},
            {NO_MATCHING_RULE, "Missing matching rule"},
            {INVALID_FILTER_COMPARISON, "Invalid comparison operator"},
            {INVALID_CHAR_IN_FILTER, "The invalid character \"{0}\" needs to be escaped as \"{1}\""},
            {INVALID_ESC_IN_DESCR, "Escape sequence not allowed in attribute description"},
            {INVALID_CHAR_IN_DESCR, "Invalid character \"{0}\" in attribute description"},
            {NOT_AN_ATTRIBUTE, "Schema element is not an LdapAttributeSchema object"},
            {UNEQUAL_LENGTHS, "Length of attribute Name array does not equal length of Flags array"},
            {IMPROPER_REFERRAL, "Referral not supported for command {0}"},
            {NOT_IMPLEMENTED, "Method LdapConnection.startTLS not implemented"},
            {NO_MEMORY, "All results could not be stored in memory, sort failed"},
            {SERVER_SHUTDOWN_REQ, "Received unsolicited notification from server {0}:{1} to shutdown"},
            {INVALID_ADDRESS, "Invalid syntax for address with port; {0}"},
            {UNKNOWN_RESULT, "Unknown Ldap result code {0}"},
            {
                OUTSTANDING_OPERATIONS,
                "Cannot start or stop TLS because outstanding Ldap operations exist on this connection"
            },
            {
                WRONG_FACTORY,
                "StartTLS cannot use the set socket factory because it does not implement LdapTLSSocketFactory"
            },
            {NO_TLS_FACTORY, "StartTLS failed because no LdapTLSSocketFactory has been set for this Connection"},
            {NO_STARTTLS, "An attempt to stopTLS on a connection where startTLS had not been called"},
            {STOPTLS_ERROR, "Error stopping TLS: Error getting input & output streams from the original socket"},
            {MULTIPLE_SCHEMA, "Multiple schema found when reading the subschemaSubentry for {0}"},
            {NO_SCHEMA, "No schema found when reading the subschemaSubentry for {0}"},
            {READ_MULTIPLE, "Read response is ambiguous, multiple entries returned"},
            {CANNOT_BIND, "Cannot bind. Use PoolManager.getBoundConnection()"},
            {SSL_PROVIDER_MISSING, "Please ensure that SSL Provider is properly installed."}
        };

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public static string GetErrorMessage(string code)
            => MessageMap.ContainsKey(code) ? MessageMap[code] : code;

        /// <summary>
        /// Returns the message stored in the ExceptionMessages resource for the
        /// specified locale using messageOrKey and argments passed into the
        /// constructor.  If no string exists in the resource then this returns
        /// the string stored in message.  (This method is identical to
        /// getLdapErrorMessage(Locale locale).)
        /// </summary>
        /// <param name="messageOrKey">Key string for the resource.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// the text for the message specified by the MessageKey or the Key
        /// if it there is no message for that key.
        /// </returns>
        public static string GetMessage(string messageOrKey, object[] arguments)
        {
            var pattern = GetErrorMessage(messageOrKey ?? string.Empty);

            // Format the message if arguments were passed
            return arguments != null ? string.Format(pattern, arguments) : pattern;
        }
    }

    /// <summary>
    /// Thrown to indicate that an Ldap exception has occurred. This is a general
    /// exception which includes a message and an Ldap result code.
    /// An LdapException can result from physical problems (such as
    /// network errors) as well as problems with Ldap operations detected
    /// by the server. For example, if an Ldap add operation fails because of a
    /// duplicate entry, the server returns a result code.
    /// Five possible sources of information are available from LdapException:
    /// <dl><dt>Result Code:</dt><dd>
    /// The <code>getResultCode</code> method returns a result code,
    /// which can be compared against standard Ldap result codes.
    /// </dd><dt>Message:</dt><dd>
    /// The <code>getMessage</code> method returns a localized message
    /// from the message resource that corresponds to the result code.
    /// </dd><dt>Ldap server Message:</dt><dd>
    /// The <code>getLdapErrorMessage</code> method returns any error
    /// message received from the Ldap server.
    /// </dd><dt>Matched DN:</dt><dd>
    /// The <code>getMatchedDN</code> method retrieves the part of a
    /// submitted distinguished name which could be matched by the server
    /// </dd><dt>Root Cause:</dt><dd>
    /// The <code>getCause</code> method returns the a nested exception
    /// that was the original cause for the error.
    /// </dd></dl>
    /// The <code>ToString</code> method returns a string containing all
    /// the above sources of information, if they have a value.
    /// Exceptions generated by the API, i.e. that are not a result
    /// of a server response, can be identified as
    /// <tt>
    /// instanceof
    /// {@link LdapLocalException}
    /// </tt>
    /// The following table lists the standard Ldap result codes.
    /// See RFC2251 for a discussion of the meanings of the result codes.
    /// The corresponding ASN.1 definition from RFC2251 is provided in parentheses.
    /// <table><tr><td><b>Value</b></td><td><b>Result Code</b></td></tr><tr><td> 0</td><td>{@link #SUCCESS} (success) </td></tr><tr><td> 1</td><td>{@link #OPERATIONS_ERROR} (operationsError) </td></tr><tr><td> 2</td><td>{@link #PROTOCOL_ERROR} (protocolError) </td></tr><tr><td> 3</td><td>{@link #TIME_LIMIT_EXCEEDED} (timeLimitExceeded) </td></tr><tr><td> 4</td><td>{@link #SIZE_LIMIT_EXCEEDED} (sizeLimitExceeded) </td></tr><tr><td> 5</td><td>{@link #COMPARE_FALSE} (compareFalse) </td></tr><tr><td> 6</td><td>{@link #COMPARE_TRUE} (compareTrue) </td></tr><tr><td> 7</td><td>{@link #AUTH_METHOD_NOT_SUPPORTED} (authMethodNotSupported) </td></tr><tr><td> 8</td><td>{@link #STRONG_AUTH_REQUIRED} (strongAuthRequired) </td></tr><tr><td> 10</td><td>{@link #REFERRAL} (referral) </td></tr><tr><td> 11</td><td>{@link #ADMIN_LIMIT_EXCEEDED} (adminLimitExceeded) </td></tr><tr><td> 12</td><td>{@link #UNAVAILABLE_CRITICAL_EXTENSION} (unavailableCriticalExtension) </td></tr><tr><td> 13</td><td>{@link #CONFIDENTIALITY_REQUIRED} (confidentialityRequired) </td></tr><tr><td> 14</td><td>{@link #SASL_BIND_IN_PROGRESS} (saslBindInProgress) </td></tr><tr><td> 16</td><td>{@link #NO_SUCH_ATTRIBUTE} (noSuchAttribute) </td></tr><tr><td> 17</td><td>{@link #UNDEFINED_ATTRIBUTE_TYPE} (undefinedAttributeType) </td></tr><tr><td> 18</td><td>{@link #INAPPROPRIATE_MATCHING} (inappropriateMatching) </td></tr><tr><td> 19</td><td>{@link #CONSTRAINT_VIOLATION} (constraintViolation) </td></tr><tr><td> 20</td><td>{@link #ATTRIBUTE_OR_VALUE_EXISTS} (AttributeOrValueExists) </td></tr><tr><td> 21</td><td>{@link #INVALID_ATTRIBUTE_SYNTAX} (invalidAttributeSyntax) </td></tr><tr><td> 32</td><td>{@link #NO_SUCH_OBJECT} (noSuchObject) </td></tr><tr><td> 33</td><td>{@link #ALIAS_PROBLEM} (aliasProblem) </td></tr><tr><td> 34</td><td>{@link #INVALID_DN_SYNTAX} (invalidDNSyntax) </td></tr><tr><td> 35</td><td>{@link #IS_LEAF} (isLeaf) </td></tr><tr><td> 36</td><td>{@link #ALIAS_DEREFERENCING_PROBLEM} (aliasDereferencingProblem) </td></tr><tr><td> 48</td><td>{@link #INAPPROPRIATE_AUTHENTICATION} (inappropriateAuthentication) </td></tr><tr><td> 49</td><td>{@link #INVALID_CREDENTIALS} (invalidCredentials) </td></tr><tr><td> 50</td><td>{@link #INSUFFICIENT_ACCESS_RIGHTS} (insufficientAccessRights) </td></tr><tr><td> 51</td><td>{@link #BUSY} (busy) </td></tr><tr><td> 52</td><td>{@link #UNAVAILABLE} (unavailable) </td></tr><tr><td> 53</td><td>{@link #UNWILLING_TO_PERFORM} (unwillingToPerform) </td></tr><tr><td> 54</td><td>{@link #LOOP_DETECT} (loopDetect) </td></tr><tr><td> 64</td><td>{@link #NAMING_VIOLATION} (namingViolation) </td></tr><tr><td> 65</td><td>{@link #OBJECT_CLASS_VIOLATION} (objectClassViolation) </td></tr><tr><td> 66</td><td>{@link #NOT_ALLOWED_ON_NONLEAF} (notAllowedOnNonLeaf) </td></tr><tr><td> 67</td><td>{@link #NOT_ALLOWED_ON_RDN} (notAllowedOnRDN) </td></tr><tr><td> 68</td><td>{@link #ENTRY_ALREADY_EXISTS} (entryAlreadyExists) </td></tr><tr><td> 69</td><td>{@link #OBJECT_CLASS_MODS_PROHIBITED} (objectClassModsProhibited) </td></tr><tr><td> 71</td><td>{@link #AFFECTS_MULTIPLE_DSAS} (affectsMultipleDSAs </td></tr><tr><td> 80</td><td>{@link #OTHER} (other) </td></tr></table>
    /// Local errors, resulting from actions other than an operation on a
    /// server.
    /// <table><tr><td><b>Value</b></td><td><b>Result Code</b></td></tr><tr><td>81</td><td>{@link #SERVER_DOWN}</td></tr><tr><td>82</td><td>{@link #LOCAL_ERROR}</td></tr><tr><td>83</td><td>{@link #ENCODING_ERROR}</td></tr><tr><td>84</td><td>{@link #DECODING_ERROR}</td></tr><tr><td>85</td><td>{@link #Ldap_TIMEOUT}</td></tr><tr><td>86</td><td>{@link #AUTH_UNKNOWN}</td></tr><tr><td>87</td><td>{@link #FILTER_ERROR}</td></tr><tr><td>88</td><td>{@link #USER_CANCELLED}</td></tr><tr><td>90</td><td>{@link #NO_MEMORY}</td></tr><tr><td>91</td><td>{@link #CONNECT_ERROR}</td></tr><tr><td>92</td><td>{@link #Ldap_NOT_SUPPORTED}</td></tr><tr><td>93</td><td>{@link #CONTROL_NOT_FOUND}</td></tr><tr><td>94</td><td>{@link #NO_RESULTS_RETURNED}</td></tr><tr><td>95</td><td>{@link #MORE_RESULTS_TO_RETURN}</td></tr><tr><td>96</td><td>{@link #CLIENT_LOOP}</td></tr><tr><td>97</td><td>{@link #REFERRAL_LIMIT_EXCEEDED}</td></tr><tr><td>100</td><td>{@link #INVALID_RESPONSE}</td></tr><tr><td>101</td><td>{@link #AMBIGUOUS_RESPONSE}</td></tr><tr><td>112</td><td>{@link #TLS_NOT_SUPPORTED}</td></tr></table>
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class LdapException : Exception
    {
        // The Result Code
        private readonly LdapStatusCode resultCode;

        // The Matched DN
        private readonly string matchedDN;

        // The Root Cause
        private readonly Exception rootException;

        // A message from the server
        private readonly string serverMessage;

        /// <summary>
        /// Constructs a default exception with no specific error information.
        /// </summary>
        public LdapException()
        {
        }

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
        /// <param name="messageOrKey">Key to addition result information, a key into
        /// ExceptionMessages, or the information
        /// itself if the key doesn't exist.</param>
        /// <param name="resultCode">The result code returned.</param>
        /// <param name="serverMsg">Error message specifying additional information
        /// from the server</param>
        /// <param name="matchedDN">The maximal subset of a specified DN which could
        /// be matched by the server on a search operation.</param>
        /// <param name="rootException">The root exception.</param>
        public LdapException(string messageOrKey, LdapStatusCode resultCode, string serverMsg = null,
            string matchedDN = null, Exception rootException = null)
            : this(messageOrKey, null, resultCode, serverMsg, matchedDN, rootException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException" /> class.
        /// Constructs an exception with a detailed message obtained from the
        /// specified <code>MessageOrKey</code> String and modifying arguments.
        /// Additional parameters specify the result code, a message returned
        /// from the server, a matchedDN returned from
        /// the server, and a rootException which is the underlying cause of an error
        /// on the client.
        /// The String is used either as a message key to obtain a localized
        /// messsage from ExceptionMessages, or if there is no key in the
        /// resource matching the text, it is used as the detailed message itself.
        /// The message in the default locale is built with the supplied arguments,
        /// which are saved to be used for building messages for other locales.
        /// </summary>
        /// <param name="messageOrKey">Key to addition result information, a key into
        /// ExceptionMessages, or the information
        /// itself if the key doesn't exist.</param>
        /// <param name="arguments">The modifying arguments to be included in the
        /// message string.</param>
        /// <param name="resultCode">The result code returned.</param>
        /// <param name="serverMsg">Error message specifying additional information
        /// from the server</param>
        /// <param name="matchedDN">The maximal subset of a specified DN which could
        /// be matched by the server on a search operation.</param>
        /// <param name="rootException">A throwable which is the underlying cause
        /// of the LdapException.</param>
        internal LdapException(string messageOrKey, object[] arguments, LdapStatusCode resultCode,
            string serverMsg = null, string matchedDN = null, Exception rootException = null)
            : base(ExceptionMessages.GetMessage(messageOrKey, arguments))
        {
            this.resultCode = resultCode;
            this.rootException = rootException;
            this.matchedDN = matchedDN;
            serverMessage = serverMsg;
        }

        /// <summary>
        ///     Returns the error message from the Ldap server, if this message is
        ///     available (that is, if this message was set). If the message was not set,
        ///     this method returns null.
        /// </summary>
        /// <returns>
        ///     The error message or null if the message was not set.
        /// </returns>
        public virtual string LdapErrorMessage
        {
            get
            {
                if (serverMessage != null && serverMessage.Length == 0)
                {
                    return null;
                }

                return serverMessage;
            }
        }

        /// <summary>
        ///     Returns the lower level Exception which caused the failure, if any.
        ///     For example, an IOException with additional information may be returned
        ///     on a CONNECT_ERROR failure.
        /// </summary>
        public virtual Exception Cause => rootException;

        /// <summary>
        ///     Returns the result code from the exception.
        ///     The codes are defined as <code>public final static int</code> members
        ///     of the Ldap Exception class. If the exception is a
        ///     result of error information returned from a directory operation, the
        ///     code will be one of those defined for the class. Otherwise, a local error
        ///     code is returned.
        /// </summary>
        public virtual LdapStatusCode ResultCode => resultCode;

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
        public virtual string MatchedDN => matchedDN;

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message => resultCode.ToString().Humanize();

        /// <summary>
        ///     returns a string of information about the exception and the
        ///     the nested exceptions, if any.
        /// </summary>
        public override string ToString()
            => GetExceptionString("LdapException");

        /// <summary>
        /// builds a string of information about the exception and the
        /// the nested exceptions, if any.
        /// </summary>
        /// <param name="exception">The name of the exception class</param>
        /// <returns></returns>
        internal virtual string GetExceptionString(string exception)
        {
            string tmsg;

            // Craft a string from the resouce file
            var msg = ExceptionMessages.GetMessage("TOSTRING",
                new object[] {exception, base.Message, resultCode, resultCode.ToString().Humanize()});

            // If found no string from resource file, use a default string
            if (msg.ToUpper().Equals("TOSTRING".ToUpper()))
            {
                msg = exception + ": (" + resultCode + ") " + resultCode.ToString().Humanize();
            }

            // Add server message
            if (!string.IsNullOrEmpty(serverMessage))
            {
                tmsg = ExceptionMessages.GetMessage("SERVER_MSG", new object[] {exception, serverMessage});

                // If found no string from resource file, use a default string
                if (tmsg.ToUpper().Equals("SERVER_MSG".ToUpper()))
                {
                    tmsg = exception + ": Server Message: " + serverMessage;
                }

                msg = msg + '\n' + tmsg;
            }

            // Add Matched DN message
            if (matchedDN != null)
            {
                tmsg = ExceptionMessages.GetMessage("MATCHED_DN", new object[] {exception, matchedDN});

                // If found no string from resource file, use a default string
                if (tmsg.ToUpper().Equals("MATCHED_DN".ToUpper()))
                {
                    tmsg = exception + ": Matched DN: " + matchedDN;
                }

                msg = msg + '\n' + tmsg;
            }

            if (rootException != null)
            {
                msg = msg + '\n' + rootException;
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
        private string[] referrals;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException"/> class.
        /// Constructs a default exception with no specific error information.
        /// </summary>
        public LdapReferralException()
        {
        }
        
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
        public LdapReferralException(string message, LdapStatusCode resultCode, string serverMessage,
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
            string tmsg;

            // Format the basic exception information
            var msg = GetExceptionString("LdapReferralException");

            // Add failed referral information
            if (FailedReferral != null)
            {
                tmsg = ExceptionMessages.GetMessage("FAILED_REFERRAL",
                    new object[] {"LdapReferralException", FailedReferral});

                // If found no string from resource file, use a default string
                if (tmsg.ToUpper().Equals("SERVER_MSG".ToUpper()))
                {
                    tmsg = $"LdapReferralException: Failed Referral: {FailedReferral}";
                }

                msg = msg + '\n' + tmsg;
            }

            // Add referral information, display all the referrals in the list
            if (referrals != null)
            {
                foreach (var referral in referrals)
                {
                    tmsg = ExceptionMessages.GetMessage("REFERRAL_ITEM",
                        new object[] {"LdapReferralException", referral});

                    // If found no string from resource file, use a default string
                    if (tmsg.ToUpper().Equals("SERVER_MSG".ToUpper()))
                    {
                        tmsg = $"LdapReferralException: Referral: {referral}";
                    }

                    msg = msg + '\n' + tmsg;
                }
            }

            return msg;
        }

        /// <summary>
        ///     Sets the list of referrals
        /// </summary>
        /// <param name="urls">
        ///     the list of referrals returned by the Ldap server in a
        ///     single response.
        /// </param>
        internal void SetReferrals(string[] urls)
            => referrals = urls;

    }
}
#endif