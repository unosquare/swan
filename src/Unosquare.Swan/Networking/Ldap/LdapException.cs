﻿#if !UWP
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
    /// <table><tr><td><b>Value</b></td><td><b>Result Code</b></td></tr><tr><td> 0</td><td>{SUCCESS} (success) </td></tr><tr><td> 1</td><td>{OPERATIONS_ERROR} (operationsError) </td></tr><tr><td> 2</td><td>{PROTOCOL_ERROR} (protocolError) </td></tr><tr><td> 3</td><td>{TIME_LIMIT_EXCEEDED} (timeLimitExceeded) </td></tr><tr><td> 4</td><td>{SIZE_LIMIT_EXCEEDED} (sizeLimitExceeded) </td></tr><tr><td> 5</td><td>{COMPARE_FALSE} (compareFalse) </td></tr><tr><td> 6</td><td>{COMPARE_TRUE} (compareTrue) </td></tr><tr><td> 7</td><td>{AUTH_METHOD_NOT_SUPPORTED} (authMethodNotSupported) </td></tr><tr><td> 8</td><td>{STRONG_AUTH_REQUIRED} (strongAuthRequired) </td></tr><tr><td> 10</td><td>{REFERRAL} (referral) </td></tr><tr><td> 11</td><td>{ADMIN_LIMIT_EXCEEDED} (adminLimitExceeded) </td></tr><tr><td> 12</td><td>{UNAVAILABLE_CRITICAL_EXTENSION} (unavailableCriticalExtension) </td></tr><tr><td> 13</td><td>{CONFIDENTIALITY_REQUIRED} (confidentialityRequired) </td></tr><tr><td> 14</td><td>{SASL_BIND_IN_PROGRESS} (saslBindInProgress) </td></tr><tr><td> 16</td><td>{NO_SUCH_ATTRIBUTE} (noSuchAttribute) </td></tr><tr><td> 17</td><td>{UNDEFINED_ATTRIBUTE_TYPE} (undefinedAttributeType) </td></tr><tr><td> 18</td><td>{INAPPROPRIATE_MATCHING} (inappropriateMatching) </td></tr><tr><td> 19</td><td>{CONSTRAINT_VIOLATION} (constraintViolation) </td></tr><tr><td> 20</td><td>{ATTRIBUTE_OR_VALUE_EXISTS} (AttributeOrValueExists) </td></tr><tr><td> 21</td><td>{INVALID_ATTRIBUTE_SYNTAX} (invalidAttributeSyntax) </td></tr><tr><td> 32</td><td>{NO_SUCH_OBJECT} (noSuchObject) </td></tr><tr><td> 33</td><td>{ALIAS_PROBLEM} (aliasProblem) </td></tr><tr><td> 34</td><td>{INVALID_DN_SYNTAX} (invalidDNSyntax) </td></tr><tr><td> 35</td><td>{IS_LEAF} (isLeaf) </td></tr><tr><td> 36</td><td>{ALIAS_DEREFERENCING_PROBLEM} (aliasDereferencingProblem) </td></tr><tr><td> 48</td><td>{INAPPROPRIATE_AUTHENTICATION} (inappropriateAuthentication) </td></tr><tr><td> 49</td><td>{INVALID_CREDENTIALS} (invalidCredentials) </td></tr><tr><td> 50</td><td>{INSUFFICIENT_ACCESS_RIGHTS} (insufficientAccessRights) </td></tr><tr><td> 51</td><td>{BUSY} (busy) </td></tr><tr><td> 52</td><td>{UNAVAILABLE} (unavailable) </td></tr><tr><td> 53</td><td>{UNWILLING_TO_PERFORM} (unwillingToPerform) </td></tr><tr><td> 54</td><td>{LOOP_DETECT} (loopDetect) </td></tr><tr><td> 64</td><td>{NAMING_VIOLATION} (namingViolation) </td></tr><tr><td> 65</td><td>{OBJECT_CLASS_VIOLATION} (objectClassViolation) </td></tr><tr><td> 66</td><td>{NOT_ALLOWED_ON_NONLEAF} (notAllowedOnNonLeaf) </td></tr><tr><td> 67</td><td>{NOT_ALLOWED_ON_RDN} (notAllowedOnRDN) </td></tr><tr><td> 68</td><td>{ENTRY_ALREADY_EXISTS} (entryAlreadyExists) </td></tr><tr><td> 69</td><td>{OBJECT_CLASS_MODS_PROHIBITED} (objectClassModsProhibited) </td></tr><tr><td> 71</td><td>{AFFECTS_MULTIPLE_DSAS} (affectsMultipleDSAs </td></tr><tr><td> 80</td><td>{OTHER} (other) </td></tr></table>
    /// Local errors, resulting from actions other than an operation on a
    /// server.
    /// <table><tr><td><b>Value</b></td><td><b>Result Code</b></td></tr><tr><td>81</td><td>{SERVER_DOWN}</td></tr><tr><td>82</td><td>{LOCAL_ERROR}</td></tr><tr><td>83</td><td>{ENCODING_ERROR}</td></tr><tr><td>84</td><td>{DECODING_ERROR}</td></tr><tr><td>85</td><td>{Ldap_TIMEOUT}</td></tr><tr><td>86</td><td>{AUTH_UNKNOWN}</td></tr><tr><td>87</td><td>{FILTER_ERROR}</td></tr><tr><td>88</td><td>{USER_CANCELLED}</td></tr><tr><td>90</td><td>{NO_MEMORY}</td></tr><tr><td>91</td><td>{CONNECT_ERROR}</td></tr><tr><td>92</td><td>{Ldap_NOT_SUPPORTED}</td></tr><tr><td>93</td><td>{CONTROL_NOT_FOUND}</td></tr><tr><td>94</td><td>{NO_RESULTS_RETURNED}</td></tr><tr><td>95</td><td>{MORE_RESULTS_TO_RETURN}</td></tr><tr><td>96</td><td>{CLIENT_LOOP}</td></tr><tr><td>97</td><td>{REFERRAL_LIMIT_EXCEEDED}</td></tr><tr><td>100</td><td>{INVALID_RESPONSE}</td></tr><tr><td>101</td><td>{AMBIGUOUS_RESPONSE}</td></tr><tr><td>112</td><td>{TLS_NOT_SUPPORTED}</td></tr></table>
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

        private readonly LdapStatusCode resultCode;
        private readonly string matchedDN;
        private readonly Exception rootException;
        private readonly string serverMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapException"/> class.
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
        public virtual string LdapErrorMessage => serverMessage != null && serverMessage.Length == 0 ? null : serverMessage;

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
        /// <returns></returns>
        internal string GetExceptionString(string exception)
        {
            // Craft a string from the resouce file
            var msg = $"{exception}: {base.Message} ({resultCode}) {resultCode.ToString().Humanize()}";
            
            // Add server message
            if (!string.IsNullOrEmpty(serverMessage))
            {
                msg += $"\n{exception}: Server Message: {serverMessage}";
            }

            // Add Matched DN message
            if (matchedDN != null)
            {
                msg += $"\n{exception}: Matched DN: {matchedDN}";
            }

            if (rootException != null)
            {
                msg += "\n" + rootException;
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
            if (_referrals != null)
            {
                msg = _referrals.Aggregate(msg, (current, referral) => current + $"\nServer Message: {referral}");
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
            => _referrals = urls;

    }
}
#endif