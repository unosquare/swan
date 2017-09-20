﻿#if !UWP

namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Represents an Ldap exception that is not a result of a server response.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapException" />
    public class LdapLocalException : LdapException
    {
        /// <summary>
        /// Constructs a default exception with no specific error information.
        /// </summary>
        public LdapLocalException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapLocalException"/> class.
        /// Constructs a local exception with a detailed message obtained from the
        /// specified <code>MessageOrKey</code> String and the result code.
        /// The String is used either as a message key to obtain a localized
        /// messsage from ExceptionMessages, or if there is no key in the
        /// resource matching the text, it is used as the detailed message itself.
        /// </summary>
        /// <param name="messageOrKey">Key to addition result information, a key into
        /// ExceptionMessages, or the information
        /// itself if the key doesn't exist.</param>
        /// <param name="resultCode">The result code returned.</param>
        public LdapLocalException(string messageOrKey, int resultCode)
            : base(messageOrKey, resultCode, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapLocalException"/> class.
        /// Constructs a local exception with a detailed message obtained from the
        /// specified <code>MessageOrKey</code> String and modifying arguments.
        /// Additional parameters specify the result code.
        /// The String is used either as a message key to obtain a localized
        /// message from ExceptionMessages, or if there is no key in the
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
        public LdapLocalException(string messageOrKey, object[] arguments, int resultCode)
            : base(messageOrKey, arguments, resultCode, null)
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
        public LdapLocalException(string messageOrKey, int resultCode, Exception rootException)
            : base(messageOrKey, resultCode, null, rootException)
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
        public LdapLocalException(string messageOrKey, object[] arguments, int resultCode, Exception rootException)
            : base(messageOrKey, arguments, resultCode, null, rootException)
        {
        }

        /// <summary>
        /// Returns a string of information about the exception and the
        /// the nested exceptions, if any.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // Format the basic exception information
            return GetExceptionString("LdapLocalException");
        }
    }

    /// <summary>
    ///     This class contains strings that may be associated with Exceptions generated
    ///     by the Ldap API libraries.
    ///     Two entries are made for each message, a String identifier, and the
    ///     actual error string.  Parameters are identified as {0}, {1}, etc.
    /// </summary>
    public class ExceptionMessages // : System.Resources.ResourceManager
    {
        //static strings to aide lookup and guarantee accuracy:
        //DO NOT include these strings in other Locales
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
        {
            if (MessageMap.ContainsKey(code))
                return MessageMap[code];
            return code;
        }
    }

    // End ExceptionMessages

    /// <summary>
    ///     This class contains strings corresponding to Ldap Result Codes.
    ///     The resources are accessed by the String representation of the result code.
    /// </summary>
    public class ResultCodeMessages // : System.Resources.ResourceManager
    {
        internal static readonly Dictionary<string, string> ErrorCodes = new Dictionary<string, string>
        {
            {"0", "Success"},
            {"1", "Operations Error"},
            {"2", "Protocol Error"},
            {"3", "Timelimit Exceeded"},
            {"4", "Sizelimit Exceeded"},
            {"5", "Compare False"},
            {"6", "Compare True"},
            {"7", "Authentication Method Not Supported"},
            {"8", "Strong Authentication Required"},
            {"9", "Partial Results"},
            {"10", "Referral"},
            {"11", "Administrative Limit Exceeded"},
            {"12", "Unavailable Critical Extension"},
            {"13", "Confidentiality Required"},
            {"14", "SASL Bind In Progress"},
            {"16", "No Such Attribute"},
            {"17", "Undefined Attribute Type"},
            {"18", "Inappropriate Matching"},
            {"19", "Constraint Violation"},
            {"20", "Attribute Or Value Exists"},
            {"21", "Invalid Attribute Syntax"},
            {"32", "No Such Object"},
            {"33", "Alias Problem"},
            {"34", "Invalid DN Syntax"},
            {"35", "Is Leaf"},
            {"36", "Alias Dereferencing Problem"},
            {"48", "Inappropriate Authentication"},
            {"49", "Invalid Credentials"},
            {"50", "Insufficient Access Rights"},
            {"51", "Busy"},
            {"52", "Unavailable"},
            {"53", "Unwilling To Perform"},
            {"54", "Loop Detect"},
            {"64", "Naming Violation"},
            {"65", "Object Class Violation"},
            {"66", "Not Allowed On Non-leaf"},
            {"67", "Not Allowed On RDN"},
            {"68", "Entry Already Exists"},
            {"69", "Object Class Modifications Prohibited"},
            {"71", "Affects Multiple DSAs"},
            {"80", "Other"},
            {"81", "Server Down"},
            {"82", "Local Error"},
            {"83", "Encoding Error"},
            {"84", "Decoding Error"},
            {"85", "Ldap Timeout"},
            {"86", "Authentication Unknown"},
            {"87", "Filter Error"},
            {"88", "User Cancelled"},
            {"89", "Parameter Error"},
            {"90", "No Memory"},
            {"91", "Connect Error"},
            {"92", "Ldap Not Supported"},
            {"93", "Control Not Found"},
            {"94", "No Results Returned"},
            {"95", "More Results To Return"},
            {"96", "Client Loop"},
            {"97", "Referral Limit Exceeded"},
            {"112", "TLS not supported"},
            {"113", "SSL handshake failed"},
            {"114", "SSL Provider not found"}
        };

        /// <summary>
        /// Gets the result code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>String code</returns>
        public static string GetResultCode(string code)
        {
            return ErrorCodes[code];
        }
    }

    // End ResultCodeMessages

    /// <summary>
    ///     A utility class to get strings from the ExceptionMessages and
    ///     ResultCodeMessages resources.
    /// </summary>
    public class ResourcesHandler
    {
        // Cannot create an instance of this class
        private ResourcesHandler()
        {
        }

        /// <summary>
        /// The default Locale
        /// </summary>
        private static CultureInfo defaultLocale;
        
        /// <summary>
        /// Returns the message stored in the ExceptionMessages resource for the
        /// specified locale using messageOrKey and argments passed into the
        /// constructor.  If no string exists in the resource then this returns
        /// the string stored in message.  (This method is identical to
        /// getLdapErrorMessage(Locale locale).)
        /// </summary>
        /// <param name="messageOrKey">Key string for the resource.</param>
        /// <param name="arguments">The arguments.</param>
        /// strings out of ExceptionMessages.</param>
        /// <returns>
        /// the text for the message specified by the MessageKey or the Key
        /// if it there is no message for that key.
        /// </returns>
        public static string GetMessage(string messageOrKey, object[] arguments)
        {
            if (defaultLocale == null)
                defaultLocale = CultureInfo.CurrentUICulture;
            
            if (messageOrKey == null)
            {
                messageOrKey = "";
            }

            var pattern = ExceptionMessages.GetErrorMessage(messageOrKey);

            // Format the message if arguments were passed
            if (arguments != null)
            {
                var strB = new StringBuilder();
                strB.AppendFormat(pattern, arguments);
                pattern = strB.ToString();
            }

            return pattern;
        }
        
        /// <summary>
        ///     Returns a string representing the Ldap result code.  The message
        ///     is obtained from the locale specific ResultCodeMessage resource.
        /// </summary>
        /// <param name="code">
        ///     the result code
        /// </param>
        /// <param name="locale">
        ///     The Locale that should be used to pull message
        ///     strings out of ResultMessages.
        /// </param>
        /// <returns>
        ///     the String representing the result code.
        /// </returns>
        public static string GetResultString(int code)
        {
            string result;
            try
            {
                result = ResultCodeMessages.GetResultCode(Convert.ToString(code));
            }
            catch (ArgumentNullException)
            {
                result = GetMessage(ExceptionMessages.UNKNOWN_RESULT, new object[] {code});
            }

            return result;
        }

        static ResourcesHandler()
        {
            defaultLocale = CultureInfo.CurrentUICulture;
        }
    }

    // end class ResourcesHandler

    /// <summary>
    ///     Thrown to indicate that an Ldap exception has occurred. This is a general
    ///     exception which includes a message and an Ldap result code.
    ///     An LdapException can result from physical problems (such as
    ///     network errors) as well as problems with Ldap operations detected
    ///     by the server. For example, if an Ldap add operation fails because of a
    ///     duplicate entry, the server returns a result code.
    ///     Five possible sources of information are available from LdapException:
    ///     <dl>
    ///         <dt>Result Code:</dt>
    ///         <dd>
    ///             The <code>getResultCode</code> method returns a result code,
    ///             which can be compared against standard Ldap result codes.
    ///         </dd>
    ///         <dt>Message:</dt>
    ///         <dd>
    ///             The <code>getMessage</code> method returns a localized message
    ///             from the message resource that corresponds to the result code.
    ///         </dd>
    ///         <dt>Ldap server Message:</dt>
    ///         <dd>
    ///             The <code>getLdapErrorMessage</code> method returns any error
    ///             message received from the Ldap server.
    ///         </dd>
    ///         <dt>Matched DN:</dt>
    ///         <dd>
    ///             The <code>getMatchedDN</code> method retrieves the part of a
    ///             submitted distinguished name which could be matched by the server
    ///         </dd>
    ///         <dt>Root Cause:</dt>
    ///         <dd>
    ///             The <code>getCause</code> method returns the a nested exception
    ///             that was the original cause for the error.
    ///         </dd>
    ///     </dl>
    ///     The <code>ToString</code> method returns a string containing all
    ///     the above sources of information, if they have a value.
    ///     Exceptions generated by the API, i.e. that are not a result
    ///     of a server response, can be identified as
    ///     <tt>
    ///         instanceof
    ///         {@link LdapLocalException}
    ///     </tt>
    ///     The following table lists the standard Ldap result codes.
    ///     See RFC2251 for a discussion of the meanings of the result codes.
    ///     The corresponding ASN.1 definition from RFC2251 is provided in parentheses.
    ///     <table>
    ///         <tr>
    ///             <td>
    ///                 <b>Value</b>
    ///             </td>
    ///             <td>
    ///                 <b>Result Code</b>
    ///             </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 0</td><td>{@link #SUCCESS} (success) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 1</td><td>{@link #OPERATIONS_ERROR} (operationsError) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 2</td><td>{@link #PROTOCOL_ERROR} (protocolError) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 3</td><td>{@link #TIME_LIMIT_EXCEEDED} (timeLimitExceeded) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 4</td><td>{@link #SIZE_LIMIT_EXCEEDED} (sizeLimitExceeded) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 5</td><td>{@link #COMPARE_FALSE} (compareFalse) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 6</td><td>{@link #COMPARE_TRUE} (compareTrue) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 7</td><td>{@link #AUTH_METHOD_NOT_SUPPORTED} (authMethodNotSupported) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 8</td><td>{@link #STRONG_AUTH_REQUIRED} (strongAuthRequired) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 10</td><td>{@link #REFERRAL} (referral) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 11</td><td>{@link #ADMIN_LIMIT_EXCEEDED} (adminLimitExceeded) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 12</td><td>{@link #UNAVAILABLE_CRITICAL_EXTENSION} (unavailableCriticalExtension) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 13</td><td>{@link #CONFIDENTIALITY_REQUIRED} (confidentialityRequired) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 14</td><td>{@link #SASL_BIND_IN_PROGRESS} (saslBindInProgress) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 16</td><td>{@link #NO_SUCH_ATTRIBUTE} (noSuchAttribute) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 17</td><td>{@link #UNDEFINED_ATTRIBUTE_TYPE} (undefinedAttributeType) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 18</td><td>{@link #INAPPROPRIATE_MATCHING} (inappropriateMatching) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 19</td><td>{@link #CONSTRAINT_VIOLATION} (constraintViolation) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 20</td><td>{@link #ATTRIBUTE_OR_VALUE_EXISTS} (AttributeOrValueExists) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 21</td><td>{@link #INVALID_ATTRIBUTE_SYNTAX} (invalidAttributeSyntax) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 32</td><td>{@link #NO_SUCH_OBJECT} (noSuchObject) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 33</td><td>{@link #ALIAS_PROBLEM} (aliasProblem) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 34</td><td>{@link #INVALID_DN_SYNTAX} (invalidDNSyntax) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 35</td><td>{@link #IS_LEAF} (isLeaf) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 36</td><td>{@link #ALIAS_DEREFERENCING_PROBLEM} (aliasDereferencingProblem) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 48</td><td>{@link #INAPPROPRIATE_AUTHENTICATION} (inappropriateAuthentication) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 49</td><td>{@link #INVALID_CREDENTIALS} (invalidCredentials) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 50</td><td>{@link #INSUFFICIENT_ACCESS_RIGHTS} (insufficientAccessRights) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 51</td><td>{@link #BUSY} (busy) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 52</td><td>{@link #UNAVAILABLE} (unavailable) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 53</td><td>{@link #UNWILLING_TO_PERFORM} (unwillingToPerform) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 54</td><td>{@link #LOOP_DETECT} (loopDetect) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 64</td><td>{@link #NAMING_VIOLATION} (namingViolation) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 65</td><td>{@link #OBJECT_CLASS_VIOLATION} (objectClassViolation) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 66</td><td>{@link #NOT_ALLOWED_ON_NONLEAF} (notAllowedOnNonLeaf) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 67</td><td>{@link #NOT_ALLOWED_ON_RDN} (notAllowedOnRDN) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 68</td><td>{@link #ENTRY_ALREADY_EXISTS} (entryAlreadyExists) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 69</td><td>{@link #OBJECT_CLASS_MODS_PROHIBITED} (objectClassModsProhibited) </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 71</td><td>{@link #AFFECTS_MULTIPLE_DSAS} (affectsMultipleDSAs </td>
    ///         </tr>
    ///         <tr>
    ///             <td> 80</td><td>{@link #OTHER} (other) </td>
    ///         </tr>
    ///     </table>
    ///     Local errors, resulting from actions other than an operation on a
    ///     server.
    ///     <table>
    ///         <tr>
    ///             <td>
    ///                 <b>Value</b>
    ///             </td>
    ///             <td>
    ///                 <b>Result Code</b>
    ///             </td>
    ///         </tr>
    ///         <tr>
    ///             <td>81</td><td>{@link #SERVER_DOWN}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>82</td><td>{@link #LOCAL_ERROR}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>83</td><td>{@link #ENCODING_ERROR}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>84</td><td>{@link #DECODING_ERROR}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>85</td><td>{@link #Ldap_TIMEOUT}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>86</td><td>{@link #AUTH_UNKNOWN}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>87</td><td>{@link #FILTER_ERROR}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>88</td><td>{@link #USER_CANCELLED}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>90</td><td>{@link #NO_MEMORY}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>91</td><td>{@link #CONNECT_ERROR}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>92</td><td>{@link #Ldap_NOT_SUPPORTED}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>93</td><td>{@link #CONTROL_NOT_FOUND}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>94</td><td>{@link #NO_RESULTS_RETURNED}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>95</td><td>{@link #MORE_RESULTS_TO_RETURN}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>96</td><td>{@link #CLIENT_LOOP}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>97</td><td>{@link #REFERRAL_LIMIT_EXCEEDED}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>100</td><td>{@link #INVALID_RESPONSE}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>101</td><td>{@link #AMBIGUOUS_RESPONSE}</td>
    ///         </tr>
    ///         <tr>
    ///             <td>112</td><td>{@link #TLS_NOT_SUPPORTED}</td>
    ///         </tr>
    ///     </table>
    /// </summary>
    public class LdapException : Exception
    {
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
                if ((object) serverMessage != null && serverMessage.Length == 0)
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
        public virtual int ResultCode => resultCode;

        /// <summary>
        ///     Returns the part of a submitted distinguished name which could be
        ///     matched by the server.
        ///     If the exception was caused by a local error, such as no server
        ///     available, the return value is null. If the exception resulted from
        ///     an operation being executed on a server, the value is an empty string
        ///     except when the result of the operation was one of the following:
        ///     <ul>
        ///         <li>NO_SUCH_OBJECT</li>
        ///         <li>ALIAS_PROBLEM</li>
        ///         <li>INVALID_DN_SYNTAX</li>
        ///         <li>ALIAS_DEREFERENCING_PROBLEM</li>
        ///     </ul>
        /// </summary>
        /// <returns>
        ///     The part of a submitted distinguished name which could be
        ///     matched by the server or null if the error is a local error.
        /// </returns>
        public virtual string MatchedDN => matchedDN;

        public override string Message => resultCodeToString();

        // The Result Code
        private readonly int resultCode;
        // The localized message
        private string messageOrKey;
        // The arguments associated with the localized message
        private object[] arguments;
        // The Matched DN
        private readonly string matchedDN;
        // The Root Cause
        private readonly Exception rootException;
        // A message from the server
        private readonly string serverMessage;

        /// <summary>
        ///     Indicates the requested client operation completed successfully.
        ///     SUCCESS = 0<p />
        /// </summary>
        public const int SUCCESS = 0;

        /// <summary>
        ///     Indicates an internal error.
        ///     The server is unable to respond with a more specific error and is
        ///     also unable to properly respond to a request. It does not indicate
        ///     that the client has sent an erroneous message.
        ///     OPERATIONS_ERROR = 1
        /// </summary>
        public const int OPERATIONS_ERROR = 1;

        /// <summary>
        ///     Indicates that the server has received an invalid or malformed request
        ///     from the client.
        ///     PROTOCOL_ERROR = 2
        /// </summary>
        public const int PROTOCOL_ERROR = 2;

        /// <summary>
        ///     Indicates that the operation's time limit specified by either the
        ///     client or the server has been exceeded.
        ///     On search operations, incomplete results are returned.
        ///     TIME_LIMIT_EXCEEDED = 3
        /// </summary>
        public const int TIME_LIMIT_EXCEEDED = 3;

        /// <summary>
        ///     Indicates that in a search operation, the size limit specified by
        ///     the client or the server has been exceeded. Incomplete results are
        ///     returned.
        ///     SIZE_LIMIT_EXCEEDED = 4
        /// </summary>
        public const int SIZE_LIMIT_EXCEEDED = 4;

        /// <summary>
        ///     Does not indicate an error condition. Indicates that the results of
        ///     a compare operation are false.
        ///     COMPARE_FALSE = 5
        /// </summary>
        public const int COMPARE_FALSE = 5;

        /// <summary>
        ///     Does not indicate an error condition. Indicates that the results of a
        ///     compare operation are true.
        ///     COMPARE_TRUE = 6
        /// </summary>
        public const int COMPARE_TRUE = 6;

        /// <summary>
        ///     Indicates that during a bind operation the client requested an
        ///     authentication method not supported by the Ldap server.
        ///     AUTH_METHOD_NOT_SUPPORTED = 7
        /// </summary>
        public const int AUTH_METHOD_NOT_SUPPORTED = 7;

        /// <summary>
        /// Indicates a problem with the level of authentication.
        /// One of the following has occurred:
        /// <ul><li>
        /// In bind requests, the Ldap server accepts only strong
        /// authentication.
        /// </li><li>
        /// In a client request, the client requested an operation such as delete
        /// that requires strong authentication.
        /// </li><li>
        /// In an unsolicited notice of disconnection, the Ldap server discovers
        /// the security protecting the communication between the client and
        /// server has unexpectedly failed or been compromised.
        /// </li></ul>
        /// STRONG_AUTH_REQUIRED = 8
        /// </summary>
        public const int STRONG_AUTH_REQUIRED = 8;

        /// <summary>
        ///     Returned by some Ldap servers to Ldapv2 clients to indicate that a referral
        ///     has been returned in the error string.
        ///     Ldap_PARTIAL_RESULTS = 9
        /// </summary>
        public const int Ldap_PARTIAL_RESULTS = 9;

        /// <summary>
        ///     Does not indicate an error condition. In Ldapv3, indicates that the server
        ///     does not hold the target entry of the request, but that the servers in the
        ///     referral field may.
        ///     REFERRAL = 10
        /// </summary>
        public const int REFERRAL = 10;

        /// <summary>
        ///     Indicates that an Ldap server limit set by an administrative authority
        ///     has been exceeded.
        ///     ADMIN_LIMIT_EXCEEDED = 11
        /// </summary>
        public const int ADMIN_LIMIT_EXCEEDED = 11;

        /// <summary>
        ///     Indicates that the Ldap server was unable to satisfy a request because
        ///     one or more critical extensions were not available.
        ///     Either the server does not support the control or the control is not
        ///     appropriate for the operation type.
        ///     UNAVAILABLE_CRITICAL_EXTENSION = 12
        /// </summary>
        public const int UNAVAILABLE_CRITICAL_EXTENSION = 12;

        /// <summary>
        ///     Indicates that the session is not protected by a protocol such as
        ///     Transport Layer Security (TLS), which provides session confidentiality.
        ///     CONFIDENTIALITY_REQUIRED = 13
        /// </summary>
        public const int CONFIDENTIALITY_REQUIRED = 13;

        /// <summary>
        ///     Does not indicate an error condition, but indicates that the server is
        ///     ready for the next step in the process. The client must send the server
        ///     the same SASL mechanism to continue the process.
        ///     SASL_BIND_IN_PROGRESS = 14
        /// </summary>
        public const int SASL_BIND_IN_PROGRESS = 14;

        /// <summary>
        ///     Indicates that the attribute specified in the modify or compare
        ///     operation does not exist in the entry.
        ///     NO_SUCH_ATTRIBUTE = 16
        /// </summary>
        public const int NO_SUCH_ATTRIBUTE = 16;

        /// <summary>
        ///     Indicates that the attribute specified in the modify or add operation
        ///     does not exist in the Ldap server's schema.
        ///     UNDEFINED_ATTRIBUTE_TYPE = 17
        /// </summary>
        public const int UNDEFINED_ATTRIBUTE_TYPE = 17;

        /// <summary>
        ///     Indicates that the matching rule specified in the search filter does
        ///     not match a rule defined for the attribute's syntax.
        ///     INAPPROPRIATE_MATCHING = 18
        /// </summary>
        public const int INAPPROPRIATE_MATCHING = 18;

        /// <summary>
        ///     Indicates that the attribute value specified in a modify, add, or
        ///     modify DN operation violates constraints placed on the attribute. The
        ///     constraint can be one of size or content (for example, string only,
        ///     no binary data).
        ///     CONSTRAINT_VIOLATION = 19
        /// </summary>
        public const int CONSTRAINT_VIOLATION = 19;

        /// <summary>
        ///     Indicates that the attribute value specified in a modify or add
        ///     operation already exists as a value for that attribute.
        ///     ATTRIBUTE_OR_VALUE_EXISTS = 20
        /// </summary>
        public const int ATTRIBUTE_OR_VALUE_EXISTS = 20;

        /// <summary>
        ///     Indicates that the attribute value specified in an add, compare, or
        ///     modify operation is an unrecognized or invalid syntax for the attribute.
        ///     INVALID_ATTRIBUTE_SYNTAX = 21
        /// </summary>
        public const int INVALID_ATTRIBUTE_SYNTAX = 21;

        /// <summary>
        ///     Indicates the target object cannot be found.
        ///     This code is not returned on the following operations:
        ///     <ul>
        ///         <li>
        ///             Search operations that find the search base but cannot find any
        ///             entries that match the search filter.
        ///         </li>
        ///         <li>Bind operations.</li>
        ///     </ul>
        ///     NO_SUCH_OBJECT = 32
        /// </summary>
        public const int NO_SUCH_OBJECT = 32;

        /// <summary>
        ///     Indicates that an error occurred when an alias was dereferenced.
        ///     ALIAS_PROBLEM = 33
        /// </summary>
        public const int ALIAS_PROBLEM = 33;

        /// <summary>
        ///     Indicates that the syntax of the DN is incorrect.
        ///     If the DN syntax is correct, but the Ldap server's structure
        ///     rules do not permit the operation, the server returns
        ///     Ldap_UNWILLING_TO_PERFORM.
        ///     INVALID_DN_SYNTAX = 34
        /// </summary>
        public const int INVALID_DN_SYNTAX = 34;

        /// <summary>
        ///     Indicates that the specified operation cannot be performed on a
        ///     leaf entry.
        ///     This code is not currently in the Ldap specifications, but is
        ///     reserved for this constant.
        ///     IS_LEAF = 35
        /// </summary>
        public const int IS_LEAF = 35;

        /// <summary>
        ///     Indicates that during a search operation, either the client does not
        ///     have access rights to read the aliased object's name or dereferencing
        ///     is not allowed.
        ///     ALIAS_DEREFERENCING_PROBLEM = 36
        /// </summary>
        public const int ALIAS_DEREFERENCING_PROBLEM = 36;

        /// <summary>
        ///     Indicates that during a bind operation, the client is attempting to use
        ///     an authentication method that the client cannot use correctly.
        ///     For example, either of the following cause this error:
        ///     <ul>
        ///         <li>
        ///             The client returns simple credentials when strong credentials are
        ///             required.
        ///         </li>
        ///         <li>
        ///             The client returns a DN and a password for a simple bind when the
        ///             entry does not have a password defined.
        ///         </li>
        ///     </ul>
        ///     INAPPROPRIATE_AUTHENTICATION = 48
        /// </summary>
        public const int INAPPROPRIATE_AUTHENTICATION = 48;

        /// <summary>
        ///     Indicates that invalid information was passed during a bind operation.
        ///     One of the following occurred:
        ///     <ul>
        ///         <li> The client passed either an incorrect DN or password.</li>
        ///         <li>
        ///             The password is incorrect because it has expired, intruder detection
        ///             has locked the account, or some other similar reason.
        ///         </li>
        ///     </ul>
        ///     INVALID_CREDENTIALS = 49
        /// </summary>
        public const int INVALID_CREDENTIALS = 49;

        /// <summary>
        ///     Indicates that the caller does not have sufficient rights to perform
        ///     the requested operation.
        ///     INSUFFICIENT_ACCESS_RIGHTS = 50
        /// </summary>
        public const int INSUFFICIENT_ACCESS_RIGHTS = 50;

        /// <summary>
        ///     Indicates that the Ldap server is too busy to process the client request
        ///     at this time, but if the client waits and resubmits the request, the
        ///     server may be able to process it then.
        ///     BUSY = 51
        /// </summary>
        public const int BUSY = 51;

        /// <summary>
        ///     Indicates that the Ldap server cannot process the client's bind
        ///     request, usually because it is shutting down.
        ///     UNAVAILABLE = 52
        /// </summary>
        public const int UNAVAILABLE = 52;

        /// <summary>
        ///     Indicates that the Ldap server cannot process the request because of
        ///     server-defined restrictions.
        ///     This error is returned for the following reasons:
        ///     <ul>
        ///         <li>The add entry request violates the server's structure rules.</li>
        ///         <li>
        ///             The modify attribute request specifies attributes that users
        ///             cannot modify.
        ///         </li>
        ///     </ul>
        ///     UNWILLING_TO_PERFORM = 53
        /// </summary>
        public const int UNWILLING_TO_PERFORM = 53;

        /// <summary>
        ///     Indicates that the client discovered an alias or referral loop,
        ///     and is thus unable to complete this request.
        ///     LOOP_DETECT = 54
        /// </summary>
        public const int LOOP_DETECT = 54;

        /// <summary>
        ///     Indicates that the add or modify DN operation violates the schema's
        ///     structure rules.
        ///     For example,
        ///     <ul>
        ///         <li>The request places the entry subordinate to an alias.</li>
        ///         <li>
        ///             The request places the entry subordinate to a container that
        ///             is forbidden by the containment rules.
        ///         </li>
        ///         <li>The RDN for the entry uses a forbidden attribute type.</li>
        ///     </ul>
        ///     NAMING_VIOLATION = 64
        /// </summary>
        public const int NAMING_VIOLATION = 64;

        /// <summary>
        ///     Indicates that the add, modify, or modify DN operation violates the
        ///     object class rules for the entry.
        ///     For example, the following types of request return this error:
        ///     <ul>
        ///         <li>
        ///             The add or modify operation tries to add an entry without a value
        ///             for a required attribute.
        ///         </li>
        ///         <li>
        ///             The add or modify operation tries to add an entry with a value for
        ///             an attribute which the class definition does not contain.
        ///         </li>
        ///         <li>
        ///             The modify operation tries to remove a required attribute without
        ///             removing the auxiliary class that defines the attribute as required.
        ///         </li>
        ///     </ul>
        ///     OBJECT_CLASS_VIOLATION = 65
        /// </summary>
        public const int OBJECT_CLASS_VIOLATION = 65;

        /// <summary>
        ///     Indicates that the requested operation is permitted only on leaf entries.
        ///     For example, the following types of requests return this error:
        ///     <ul>
        ///         <li>The client requests a delete operation on a parent entry.</li>
        ///         <li> The client request a modify DN operation on a parent entry.</li>
        ///     </ul>
        ///     NOT_ALLOWED_ON_NONLEAF = 66
        /// </summary>
        public const int NOT_ALLOWED_ON_NONLEAF = 66;

        /// <summary>
        ///     Indicates that the modify operation attempted to remove an attribute
        ///     value that forms the entry's relative distinguished name.
        ///     NOT_ALLOWED_ON_RDN = 67
        /// </summary>
        public const int NOT_ALLOWED_ON_RDN = 67;

        /// <summary>
        ///     Indicates that the add operation attempted to add an entry that already
        ///     exists, or that the modify operation attempted to rename an entry to the
        ///     name of an entry that already exists.
        ///     ENTRY_ALREADY_EXISTS = 68
        /// </summary>
        public const int ENTRY_ALREADY_EXISTS = 68;

        /// <summary>
        ///     Indicates that the modify operation attempted to modify the structure
        ///     rules of an object class.
        ///     OBJECT_CLASS_MODS_PROHIBITED = 69
        /// </summary>
        public const int OBJECT_CLASS_MODS_PROHIBITED = 69;

        /// <summary>
        ///     Indicates that the modify DN operation moves the entry from one Ldap
        ///     server to another and thus requires more than one Ldap server.
        ///     AFFECTS_MULTIPLE_DSAS = 71
        /// </summary>
        public const int AFFECTS_MULTIPLE_DSAS = 71;

        /// <summary>
        ///     Indicates an unknown error condition.
        ///     OTHER = 80
        /// </summary>
        public const int OTHER = 80;

        /////////////////////////////////////////////////////////////////////////////
        // Local Errors, resulting from actions other than an operation on a server
        /////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     Indicates that the Ldap libraries cannot establish an initial connection
        ///     with the Ldap server. Either the Ldap server is down or the specified
        ///     host name or port number is incorrect.
        ///     SERVER_DOWN = 81
        /// </summary>
        public const int SERVER_DOWN = 81;

        /// <summary>
        ///     Indicates that the Ldap client has an error. This is usually a failed
        ///     dynamic memory allocation error.
        ///     LOCAL_ERROR = 82
        /// </summary>
        public const int LOCAL_ERROR = 82;

        /// <summary>
        ///     Indicates that the Ldap client encountered errors when encoding an
        ///     Ldap request intended for the Ldap server.
        ///     ENCODING_ERROR = 83
        /// </summary>
        public const int ENCODING_ERROR = 83;

        /// <summary>
        ///     Indicates that the Ldap client encountered errors when decoding an
        ///     Ldap response from the Ldap server.
        ///     DECODING_ERROR = 84
        /// </summary>
        public const int DECODING_ERROR = 84;

        /// <summary>
        ///     Indicates that the time limit of the Ldap client was exceeded while
        ///     waiting for a result.
        ///     Ldap_TIMEOUT = 85
        /// </summary>
        public const int Ldap_TIMEOUT = 85;

        /// <summary>
        ///     Indicates that a bind method was called with an unknown
        ///     authentication method.
        ///     AUTH_UNKNOWN = 86
        /// </summary>
        public const int AUTH_UNKNOWN = 86;

        /// <summary>
        ///     Indicates that the search method was called with an invalid
        ///     search filter.
        ///     FILTER_ERROR = 87
        /// </summary>
        public const int FILTER_ERROR = 87;

        /// <summary>
        ///     Indicates that the user cancelled the Ldap operation.
        ///     USER_CANCELLED = 88
        /// </summary>
        public const int USER_CANCELLED = 88;

        /// <summary>
        ///     Indicates that a dynamic memory allocation method failed when calling
        ///     an Ldap method.
        ///     NO_MEMORY = 90
        /// </summary>
        public const int NO_MEMORY = 90;

        /// <summary>
        ///     Indicates that the Ldap client has lost either its connection or
        ///     cannot establish a connection to the Ldap server.
        ///     CONNECT_ERROR = 91
        /// </summary>
        public const int CONNECT_ERROR = 91;

        /// <summary>
        ///     Indicates that the requested functionality is not supported by the
        ///     client. For example, if the Ldap client is established as an Ldapv2
        ///     client, the libraries set this error code when the client requests
        ///     Ldapv3 functionality.
        ///     Ldap_NOT_SUPPORTED = 92
        /// </summary>
        public const int Ldap_NOT_SUPPORTED = 92;

        /// <summary>
        ///     Indicates that the client requested a control that the libraries
        ///     cannot find in the list of supported controls sent by the Ldap server.
        ///     CONTROL_NOT_FOUND = 93
        /// </summary>
        public const int CONTROL_NOT_FOUND = 93;

        /// <summary>
        ///     Indicates that the Ldap server sent no results.
        ///     NO_RESULTS_RETURNED = 94
        /// </summary>
        public const int NO_RESULTS_RETURNED = 94;

        /// <summary>
        ///     Indicates that more results are chained in the result message.
        ///     MORE_RESULTS_TO_RETURN = 95
        /// </summary>
        public const int MORE_RESULTS_TO_RETURN = 95;

        /// <summary>
        ///     Indicates the Ldap libraries detected a loop. Usually this happens
        ///     when following referrals.
        ///     CLIENT_LOOP = 96
        /// </summary>
        public const int CLIENT_LOOP = 96;

        /// <summary>
        ///     Indicates that the referral exceeds the hop limit. The default hop
        ///     limit is ten.
        ///     The hop limit determines how many servers the client can hop through
        ///     to retrieve data. For example, suppose the following conditions:
        ///     <ul>
        ///         <li>Suppose the hop limit is two.</li>
        ///         <li>
        ///             If the referral is to server D which can be contacted only through
        ///             server B (1 hop) which contacts server C (2 hops) which contacts
        ///             server D (3 hops).
        ///         </li>
        ///     </ul>
        ///     With these conditions, the hop limit is exceeded and the Ldap
        ///     libraries set this code.
        ///     REFERRAL_LIMIT_EXCEEDED = 97
        /// </summary>
        public const int REFERRAL_LIMIT_EXCEEDED = 97;

        /// <summary>
        ///     Indicates that the server response to a request is invalid.
        ///     INVALID_RESPONSE = 100
        /// </summary>
        public const int INVALID_RESPONSE = 100;

        /// <summary>
        ///     Indicates that the server response to a request is ambiguous.
        ///     AMBIGUOUS_RESPONSE = 101
        /// </summary>
        public const int AMBIGUOUS_RESPONSE = 101;

        /// <summary>
        ///     Indicates that TLS is not supported on the server.
        ///     TLS_NOT_SUPPORTED = 112
        /// </summary>
        public const int TLS_NOT_SUPPORTED = 112;

        /// <summary>
        ///     Indicates that SSL Handshake could not succeed.
        ///     SSL_HANDSHAKE_FAILED = 113
        /// </summary>
        public const int SSL_HANDSHAKE_FAILED = 113;

        /// <summary>
        ///     Indicates that SSL Provider could not be found.
        ///     SSL_PROVIDER_NOT_FOUND = 114
        /// </summary>
        public const int SSL_PROVIDER_NOT_FOUND = 114;

        /*
                * Note: Error strings have been pulled out into
                * ResultCodeMessages.txt
                */

        /// <summary>
        /// Constructs a default exception with no specific error information.
        /// </summary>
        public LdapException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException"/> class.
        ///     Constructs an exception with a detailed message obtained from the
        ///     specified <code>MessageOrKey</code> String, the result code,
        ///     and a server meessage.
        ///     The String is used either as a message key to obtain a localized
        ///     messsage from ExceptionMessages, or if there is no key in the
        ///     resource matching the text, it is used as the detailed message itself.
        /// </summary>
        /// <param name="messageOrKey">
        ///     Key to addition result information, a key into
        ///     ExceptionMessages, or the information
        ///     itself if the key doesn't exist.
        /// </param>
        /// <param name="resultCode">
        ///     The result code returned.
        /// </param>
        /// <param name="serverMsg">
        ///     Error message specifying additional information
        ///     from the server
        /// </param>
        public LdapException(string messageOrKey, int resultCode, string serverMsg)
            : this(messageOrKey, null, resultCode, serverMsg, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException"/> class.
        ///     Constructs an exception with a detailed message obtained from the
        ///     specified <code>MessageOrKey</code> String and modifying arguments.
        ///     Additional parameters specify the result code and server message.
        ///     The String is used either as a message key to obtain a localized
        ///     messsage from ExceptionMessages, or if there is no key in the
        ///     resource matching the text, it is used as the detailed message itself.
        ///     The message in the default locale is built with the supplied arguments,
        ///     which are saved to be used for building messages for other locales.
        /// </summary>
        /// <param name="messageOrKey">
        ///     Key to addition result information, a key into
        ///     ExceptionMessages, or the information
        ///     itself if the key doesn't exist.
        /// </param>
        /// <param name="arguments">
        ///     The modifying arguments to be included in the
        ///     message string.
        /// </param>
        /// <param name="serverMsg">
        ///     Error message specifying additional information
        ///     from the server
        /// </param>
        /// <param name="resultCode">
        ///     The result code returned.
        /// </param>
        public LdapException(string messageOrKey, object[] arguments, int resultCode, string serverMsg)
            : this(messageOrKey, arguments, resultCode, serverMsg, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException"/> class.
        ///     Constructs an exception with a detailed message obtained from the
        ///     specified <code>MessageOrKey</code> String.
        ///     Additional parameters specify the result code, the server message, and a
        ///     rootException which is the underlying cause of an error on the client.
        ///     The String is used either as a message key to obtain a localized
        ///     messsage from ExceptionMessages, or if there is no key in the
        ///     resource matching the text, it is used as the detailed message itself.
        /// </summary>
        /// <param name="messageOrKey">
        ///     Key to addition result information, a key into
        ///     ExceptionMessages, or the information
        ///     itself if the key doesn't exist.
        /// </param>
        /// <param name="resultCode">
        ///     The result code returned.
        /// </param>
        /// <param name="serverMsg">
        ///     Error message specifying additional information
        ///     from the server
        /// </param>
        /// <param name="rootException">
        ///     A throwable which is the underlying cause
        ///     of the LdapException.
        /// </param>
        public LdapException(string messageOrKey, int resultCode, string serverMsg, Exception rootException = null)
            : this(messageOrKey, null, resultCode, serverMsg, null, rootException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException" /> class.
        /// Constructs an exception with a detailed message obtained from the
        /// specified <code>MessageOrKey</code> String and modifying arguments.
        /// Additional parameters specify the result code, the server message,
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
        /// <param name="serverMsg">Error message specifying additional information
        /// from the server</param>
        /// <param name="rootException">A throwable which is the underlying cause
        /// of the LdapException.</param>
        public LdapException(string messageOrKey, object[] arguments, int resultCode, string serverMsg,
            Exception rootException = null) : this(messageOrKey, arguments, resultCode, serverMsg, null, rootException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException"/> class.
        ///     Constructs an exception with a detailed message obtained from the
        ///     specified <code>MessageOrKey</code> String.
        ///     Additional parameters specify the result code, the message returned
        ///     from the server, and a matchedDN returned from the server.
        ///     The String is used either as a message key to obtain a localized
        ///     messsage from ExceptionMessages, or if there is no key in the
        ///     resource matching the text, it is used as the detailed message itself.
        /// </summary>
        /// <param name="messageOrKey">
        ///     Key to addition result information, a key into
        ///     ExceptionMessages, or the information
        ///     itself if the key doesn't exist.
        /// </param>
        /// <param name="resultCode">
        ///     The result code returned.
        /// </param>
        /// <param name="serverMsg">
        ///     Error message specifying additional information
        ///     from the server
        /// </param>
        /// <param name="matchedDN">
        ///     The maximal subset of a specified DN which could
        ///     be matched by the server on a search operation.
        /// </param>
        public LdapException(string messageOrKey, int resultCode, string serverMsg, string matchedDN)
            : this(messageOrKey, null, resultCode, serverMsg, matchedDN, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException"/> class.
        ///     Constructs an exception with a detailed message obtained from the
        ///     specified <code>MessageOrKey</code> String and modifying arguments.
        ///     Additional parameters specify the result code, a message returned from
        ///     the server, and a matchedDN returned from the server.
        ///     The String is used either as a message key to obtain a localized
        ///     messsage from ExceptionMessages, or if there is no key in the
        ///     resource matching the text, it is used as the detailed message itself.
        ///     The message in the default locale is built with the supplied arguments,
        ///     which are saved to be used for building messages for other locales.
        /// </summary>
        /// <param name="messageOrKey">
        ///     Key to addition result information, a key into
        ///     ExceptionMessages, or the information
        ///     itself if the key doesn't exist.
        /// </param>
        /// <param name="arguments">
        ///     The modifying arguments to be included in the
        ///     message string.
        /// </param>
        /// <param name="resultCode">
        ///     The result code returned.
        /// </param>
        /// <param name="serverMsg">
        ///     Error message specifying additional information
        ///     from the server
        /// </param>
        /// <param name="matchedDN">
        ///     The maximal subset of a specified DN which could
        ///     be matched by the server on a search operation.
        /// </param>
        public LdapException(string messageOrKey, object[] arguments, int resultCode, string serverMsg, string matchedDN)
            : this(messageOrKey, arguments, resultCode, serverMsg, matchedDN, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException"/> class.
        ///     Constructs an exception with a detailed message obtained from the
        ///     specified <code>MessageOrKey</code> String and modifying arguments.
        ///     Additional parameters specify the result code, a message returned
        ///     from the server, a matchedDN returned from
        ///     the server, and a rootException which is the underlying cause of an error
        ///     on the client.
        ///     The String is used either as a message key to obtain a localized
        ///     messsage from ExceptionMessages, or if there is no key in the
        ///     resource matching the text, it is used as the detailed message itself.
        ///     The message in the default locale is built with the supplied arguments,
        ///     which are saved to be used for building messages for other locales.
        /// </summary>
        /// <param name="messageOrKey">
        ///     Key to addition result information, a key into
        ///     ExceptionMessages, or the information
        ///     itself if the key doesn't exist.
        /// </param>
        /// <param name="arguments">
        ///     The modifying arguments to be included in the
        ///     message string.
        /// </param>
        /// <param name="resultCode">
        ///     The result code returned.
        /// </param>
        /// <param name="serverMsg">
        ///     Error message specifying additional information
        ///     from the server
        /// </param>
        /// <param name="rootException">
        ///     A throwable which is the underlying cause
        ///     of the LdapException.
        /// </param>
        /// <param name="matchedDN">
        ///     The maximal subset of a specified DN which could
        ///     be matched by the server on a search operation.
        /// </param>
        internal LdapException(string messageOrKey, object[] arguments, int resultCode, string serverMsg,
            string matchedDN = null, Exception rootException = null)
            : base(ResourcesHandler.GetMessage(messageOrKey, arguments))
        {
            this.messageOrKey = messageOrKey;
            this.arguments = arguments;
            this.resultCode = resultCode;
            this.rootException = rootException;
            this.matchedDN = matchedDN;
            serverMessage = serverMsg;
        }

        /// <summary>
        ///     Returns a string representing the result code in the default
        ///     locale.
        /// </summary>
        /// <returns>
        ///     The message for the result code in the LdapException object.
        /// </returns>
        public virtual string resultCodeToString()
        {
            return ResourcesHandler.GetResultString(resultCode);
        }

        /// <summary>
        ///     Returns a string representing the specified result code in the default
        ///     locale.
        /// </summary>
        /// <param name="code">
        ///     The result code for which a message is to be returned.
        /// </param>
        /// <returns>
        ///     The message corresponding to the specified result code, or
        ///     or null if the message is not available for the default locale.
        /// </returns>
        public static string resultCodeToString(int code)
        {
            return ResourcesHandler.GetResultString(code);
        }
        
        /// <summary>
        ///     returns a string of information about the exception and the
        ///     the nested exceptions, if any.
        /// </summary>
        public override string ToString()
        {
            return GetExceptionString("LdapException");
        }

        /// <summary>
        ///     builds a string of information about the exception and the
        ///     the nested exceptions, if any.
        /// </summary>
        /// <param name="exception">
        ///     The name of the exception class
        /// </param>
        internal virtual string GetExceptionString(string exception)
        {
            string tmsg;

            // Format the basic exception information

            // Craft a string from the resouce file
            var msg = ResourcesHandler.GetMessage("TOSTRING",
                new object[] {exception, base.Message, resultCode, resultCodeToString()});

            // If found no string from resource file, use a default string
            if (msg.ToUpper().Equals("TOSTRING".ToUpper()))
            {
                msg = exception + ": (" + resultCode + ") " + resultCodeToString();
            }

            // Add server message
            if ((object) serverMessage != null && serverMessage.Length != 0)
            {
                tmsg = ResourcesHandler.GetMessage("SERVER_MSG", new object[] {exception, serverMessage});

                // If found no string from resource file, use a default string
                if (tmsg.ToUpper().Equals("SERVER_MSG".ToUpper()))
                {
                    tmsg = exception + ": Server Message: " + serverMessage;
                }

                msg = msg + '\n' + tmsg;
            }

            // Add Matched DN message
            if ((object) matchedDN != null)
            {
                tmsg = ResourcesHandler.GetMessage("MATCHED_DN", new object[] {exception, matchedDN});

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
    ///     Thrown when a server returns a referral and when a referral has not
    ///     been followed.  It contains a list of URL strings corresponding
    ///     to the referrals or search continuation references received on an Ldap
    ///     operation.
    /// </summary>
    public class LdapReferralException : LdapException
    {
        /// <summary>
        ///     Sets a referral that could not be processed
        /// </summary>
        public virtual string FailedReferral
        {
            get => failedReferral;

            set => failedReferral = value;
        }

        private string failedReferral;
        private string[] referrals;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException"/> class.
        /// Constructs a default exception with no specific error information.
        /// </summary>
        public LdapReferralException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException"/> class.
        ///     Constructs a default exception with a specified string as additional
        ///     information.
        ///     This form is used for lower-level errors.
        /// </summary>
        /// <param name="message">
        ///     The additional error information.
        /// </param>
        public LdapReferralException(string message)
            : base(message, REFERRAL, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException"/> class.
        ///     Constructs a default exception with a specified string as additional
        ///     information.
        ///     This form is used for lower-level errors.
        /// </summary>
        /// <param name="arguments">
        ///     The modifying arguments to be included in the
        ///     message string.
        /// </param>
        /// <param name="message">
        ///     The additional error information.
        /// </param>
        public LdapReferralException(string message, object[] arguments)
            : base(message, arguments, REFERRAL, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException"/> class.
        ///     Constructs a default exception with a specified string as additional
        ///     information and an exception that indicates a failure to follow a
        ///     referral. This exception applies only to synchronous operations and
        ///     is thrown only on receipt of a referral when the referral was not
        ///     followed.
        /// </summary>
        /// <param name="message">
        ///     The additional error information.
        /// </param>
        /// <param name="rootException">
        ///     An exception which caused referral following to fail.
        /// </param>
        public LdapReferralException(string message, Exception rootException)
            : base(message, REFERRAL, null, rootException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException"/> class.
        ///     Constructs a default exception with a specified string as additional
        ///     information and an exception that indicates a failure to follow a
        ///     referral. This excepiton applies only to synchronous operations and
        ///     is thrown only on receipt of a referral when the referral was not
        ///     followed.
        /// </summary>
        /// <param name="message">
        ///     The additional error information.
        /// </param>
        /// <param name="arguments">
        ///     The modifying arguments to be included in the
        ///     message string.
        /// </param>
        /// <param name="rootException">
        ///     An exception which caused referral following to fail.
        /// </param>
        public LdapReferralException(string message, object[] arguments, Exception rootException)
            : base(message, arguments, REFERRAL, null, rootException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException"/> class.
        ///     Constructs an exception with a specified error string, result code, and
        ///     an error message from the server.
        /// </summary>
        /// <param name="message">
        ///     The additional error information.
        /// </param>
        /// <param name="resultCode">
        ///     The result code returned.
        /// </param>
        /// <param name="serverMessage">
        ///     Error message specifying additional information
        ///     from the server.
        /// </param>
        public LdapReferralException(string message, int resultCode, string serverMessage)
            : base(message, resultCode, serverMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException"/> class.
        ///     Constructs an exception with a specified error string, result code, and
        ///     an error message from the server.
        /// </summary>
        /// <param name="message">
        ///     The additional error information.
        /// </param>
        /// <param name="arguments">
        ///     The modifying arguments to be included in the
        ///     message string.
        /// </param>
        /// <param name="resultCode">
        ///     The result code returned.
        /// </param>
        /// <param name="serverMessage">
        ///     Error message specifying additional information
        ///     from the server.
        /// </param>
        public LdapReferralException(string message, object[] arguments, int resultCode, string serverMessage)
            : base(message, arguments, resultCode, serverMessage)
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
        public LdapReferralException(string message, int resultCode, string serverMessage, Exception rootException = null)
            : base(message, resultCode, serverMessage, rootException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapReferralException" /> class.
        /// Constructs an exception with a specified error string, result code,
        /// an error message from the server, and an exception that indicates
        /// a failure to follow a referral.
        /// </summary>
        /// <param name="message">The additional error information.</param>
        /// <param name="arguments">The modifying arguments to be included in the
        /// message string.</param>
        /// <param name="resultCode">The result code returned.</param>
        /// <param name="serverMessage">Error message specifying additional information
        /// from the server.</param>
        /// <param name="rootException">The root exception.</param>
        public LdapReferralException(string message, object[] arguments, int resultCode, string serverMessage,
            Exception rootException)
            : base(message, arguments, resultCode, serverMessage, rootException)
        {
        }

        /// <summary>
        ///     Gets the list of referral URLs (Ldap URLs to other servers) returned by
        ///     the Ldap server.
        ///     The referral list may include URLs of a type other than ones for an Ldap
        ///     server (for example, a referral URL other than ldap://something).
        /// </summary>
        /// <returns>
        ///     The list of URLs that comprise this referral
        /// </returns>
        public virtual string[] GetReferrals() => referrals;

        /// <summary>
        ///     Sets the list of referrals
        /// </summary>
        /// <param name="urls">
        ///     the list of referrals returned by the Ldap server in a
        ///     single response.
        /// </param>
        internal virtual void SetReferrals(string[] urls)
        {
            referrals = urls;
        }

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
            if ((object) failedReferral != null)
            {
                tmsg = ResourcesHandler.GetMessage("FAILED_REFERRAL",
                    new object[] {"LdapReferralException", failedReferral});

                // If found no string from resource file, use a default string
                if (tmsg.ToUpper().Equals("SERVER_MSG".ToUpper()))
                {
                    tmsg = "LdapReferralException: Failed Referral: " + failedReferral;
                }

                msg = msg + '\n' + tmsg;
            }

            // Add referral information, display all the referrals in the list
            if (referrals != null)
            {
                for (var i = 0; i < referrals.Length; i++)
                {
                    tmsg = ResourcesHandler.GetMessage("REFERRAL_ITEM",
                        new object[] {"LdapReferralException", referrals[i]});

                    // If found no string from resource file, use a default string
                    if (tmsg.ToUpper().Equals("SERVER_MSG".ToUpper()))
                    {
                        tmsg = "LdapReferralException: Referral: " + referrals[i];
                    }

                    msg = msg + '\n' + tmsg;
                }
            }

            return msg;
        }
    }
}
#endif