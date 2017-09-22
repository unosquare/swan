﻿#if !UWP

namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    /// LDAP Connection Sttus Code
    /// </summary>
    public enum LdapStatusCode
    {
        /// <summary>
        /// Indicates the requested client operation completed successfully.
        /// SUCCESS = 0<p />
        /// </summary>
        Success = 0,

        /// <summary>
        ///     Indicates an internal error.
        ///     The server is unable to respond with a more specific error and is
        ///     also unable to properly respond to a request. It does not indicate
        ///     that the client has sent an erroneous message.
        ///     OPERATIONS_ERROR = 1
        /// </summary>
        OperationsError = 1,

        /// <summary>
        ///     Indicates that the server has received an invalid or malformed request
        ///     from the client.
        ///     PROTOCOL_ERROR = 2
        /// </summary>
        ProtocolError = 2,

        /// <summary>
        ///     Indicates that the operation's time limit specified by either the
        ///     client or the server has been exceeded.
        ///     On search operations, incomplete results are returned.
        ///     TIME_LIMIT_EXCEEDED = 3
        /// </summary>
        TimeLimitExceeded = 3,

        /// <summary>
        ///     Indicates that in a search operation, the size limit specified by
        ///     the client or the server has been exceeded. Incomplete results are
        ///     returned.
        ///     SIZE_LIMIT_EXCEEDED = 4
        /// </summary>
        SizeLimitExceeded = 4,

        /// <summary>
        ///     Does not indicate an error condition. Indicates that the results of
        ///     a compare operation are false.
        ///     COMPARE_FALSE = 5
        /// </summary>
        CompareFalse = 5,

        /// <summary>
        ///     Does not indicate an error condition. Indicates that the results of a
        ///     compare operation are true.
        ///     COMPARE_TRUE = 6
        /// </summary>
        CompareTrue = 6,

        /// <summary>
        ///     Indicates that during a bind operation the client requested an
        ///     authentication method not supported by the Ldap server.
        ///     AUTH_METHOD_NOT_SUPPORTED = 7
        /// </summary>
        AuthMethodNotSupported = 7,

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
        StrongAuthRequired = 8,

        /// <summary>
        ///     Returned by some Ldap servers to Ldapv2 clients to indicate that a referral
        ///     has been returned in the error string.
        ///     Ldap_PARTIAL_RESULTS = 9
        /// </summary>
        LdapPartialResults = 9,

        /// <summary>
        ///     Does not indicate an error condition. In Ldapv3, indicates that the server
        ///     does not hold the target entry of the request, but that the servers in the
        ///     referral field may.
        ///     REFERRAL = 10
        /// </summary>
        Referral = 10,

        /// <summary>
        ///     Indicates that an Ldap server limit set by an administrative authority
        ///     has been exceeded.
        ///     ADMIN_LIMIT_EXCEEDED = 11
        /// </summary>
        AdminLimitExceeded = 11,

        /// <summary>
        ///     Indicates that the Ldap server was unable to satisfy a request because
        ///     one or more critical extensions were not available.
        ///     Either the server does not support the control or the control is not
        ///     appropriate for the operation type.
        ///     UNAVAILABLE_CRITICAL_EXTENSION = 12
        /// </summary>
        UnavailableCriticalExtension = 12,

        /// <summary>
        ///     Indicates that the session is not protected by a protocol such as
        ///     Transport Layer Security (TLS), which provides session confidentiality.
        ///     CONFIDENTIALITY_REQUIRED = 13
        /// </summary>
        ConfidentialityRequired = 13,

        /// <summary>
        ///     Does not indicate an error condition, but indicates that the server is
        ///     ready for the next step in the process. The client must send the server
        ///     the same SASL mechanism to continue the process.
        ///     SASL_BIND_IN_PROGRESS = 14
        /// </summary>
        SaslBindInProgress = 14,

        /// <summary>
        ///     Indicates that the attribute specified in the modify or compare
        ///     operation does not exist in the entry.
        ///     NO_SUCH_ATTRIBUTE = 16
        /// </summary>
        NoSuchAttribute = 16,

        /// <summary>
        ///     Indicates that the attribute specified in the modify or add operation
        ///     does not exist in the Ldap server's schema.
        ///     UNDEFINED_ATTRIBUTE_TYPE = 17
        /// </summary>
        UndefinedAttributeType = 17,

        /// <summary>
        ///     Indicates that the matching rule specified in the search filter does
        ///     not match a rule defined for the attribute's syntax.
        ///     INAPPROPRIATE_MATCHING = 18
        /// </summary>
        InappropriateMatching = 18,

        /// <summary>
        ///     Indicates that the attribute value specified in a modify, add, or
        ///     modify DN operation violates constraints placed on the attribute. The
        ///     constraint can be one of size or content (for example, string only,
        ///     no binary data).
        ///     CONSTRAINT_VIOLATION = 19
        /// </summary>
        ConstraintViolation = 19,

        /// <summary>
        ///     Indicates that the attribute value specified in a modify or add
        ///     operation already exists as a value for that attribute.
        ///     ATTRIBUTE_OR_VALUE_EXISTS = 20
        /// </summary>
        AttributeOrValueExists = 20,

        /// <summary>
        ///     Indicates that the attribute value specified in an add, compare, or
        ///     modify operation is an unrecognized or invalid syntax for the attribute.
        ///     INVALID_ATTRIBUTE_SYNTAX = 21
        /// </summary>
        InvalidAttributeSyntax = 21,

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
        NoSuchObject = 32,

        /// <summary>
        ///     Indicates that an error occurred when an alias was dereferenced.
        ///     ALIAS_PROBLEM = 33
        /// </summary>
        AliasProblem = 33,

        /// <summary>
        ///     Indicates that the syntax of the DN is incorrect.
        ///     If the DN syntax is correct, but the Ldap server's structure
        ///     rules do not permit the operation, the server returns
        ///     Ldap_UNWILLING_TO_PERFORM.
        ///     INVALID_DN_SYNTAX = 34
        /// </summary>
        InvalidDnSyntax = 34,

        /// <summary>
        ///     Indicates that the specified operation cannot be performed on a
        ///     leaf entry.
        ///     This code is not currently in the Ldap specifications, but is
        ///     reserved for this constant.
        ///     IS_LEAF = 35
        /// </summary>
        IsLeaf = 35,

        /// <summary>
        ///     Indicates that during a search operation, either the client does not
        ///     have access rights to read the aliased object's name or dereferencing
        ///     is not allowed.
        ///     ALIAS_DEREFERENCING_PROBLEM = 36
        /// </summary>
        AliasDereferencingProblem = 36,

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
        InappropriateAuthentication = 48,

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
        InvalidCredentials = 49,

        /// <summary>
        ///     Indicates that the caller does not have sufficient rights to perform
        ///     the requested operation.
        ///     INSUFFICIENT_ACCESS_RIGHTS = 50
        /// </summary>
        InsufficientAccessRights = 50,

        /// <summary>
        ///     Indicates that the Ldap server is too busy to process the client request
        ///     at this time, but if the client waits and resubmits the request, the
        ///     server may be able to process it then.
        ///     BUSY = 51
        /// </summary>
        Busy = 51,

        /// <summary>
        ///     Indicates that the Ldap server cannot process the client's bind
        ///     request, usually because it is shutting down.
        ///     UNAVAILABLE = 52
        /// </summary>
        Unavailable = 52,

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
        UnwillingToPerform = 53,

        /// <summary>
        ///     Indicates that the client discovered an alias or referral loop,
        ///     and is thus unable to complete this request.
        ///     LOOP_DETECT = 54
        /// </summary>
        LoopDetect = 54,

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
        NamingViolation = 64,

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
        ObjectClassViolation = 65,

        /// <summary>
        ///     Indicates that the requested operation is permitted only on leaf entries.
        ///     For example, the following types of requests return this error:
        ///     <ul>
        ///         <li>The client requests a delete operation on a parent entry.</li>
        ///         <li> The client request a modify DN operation on a parent entry.</li>
        ///     </ul>
        ///     NOT_ALLOWED_ON_NONLEAF = 66
        /// </summary>
        NotAllowedOnNonleaf = 66,

        /// <summary>
        ///     Indicates that the modify operation attempted to remove an attribute
        ///     value that forms the entry's relative distinguished name.
        ///     NOT_ALLOWED_ON_RDN = 67
        /// </summary>
        NotAllowedOnRdn = 67,

        /// <summary>
        ///     Indicates that the add operation attempted to add an entry that already
        ///     exists, or that the modify operation attempted to rename an entry to the
        ///     name of an entry that already exists.
        ///     ENTRY_ALREADY_EXISTS = 68
        /// </summary>
        EntryAlreadyExists = 68,

        /// <summary>
        ///     Indicates that the modify operation attempted to modify the structure
        ///     rules of an object class.
        ///     OBJECT_CLASS_MODS_PROHIBITED = 69
        /// </summary>
        ObjectClassModsProhibited = 69,

        /// <summary>
        ///     Indicates that the modify DN operation moves the entry from one Ldap
        ///     server to another and thus requires more than one Ldap server.
        ///     AFFECTS_MULTIPLE_DSAS = 71
        /// </summary>
        AffectsMultipleDsas = 71,

        /// <summary>
        ///     Indicates an unknown error condition.
        ///     OTHER = 80
        /// </summary>
        Other = 80,

        /////////////////////////////////////////////////////////////////////////////
        // Local Errors, resulting from actions other than an operation on a server
        /////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     Indicates that the Ldap libraries cannot establish an initial connection
        ///     with the Ldap server. Either the Ldap server is down or the specified
        ///     host name or port number is incorrect.
        ///     SERVER_DOWN = 81
        /// </summary>
        ServerDown = 81,

        /// <summary>
        ///     Indicates that the Ldap client has an error. This is usually a failed
        ///     dynamic memory allocation error.
        ///     LOCAL_ERROR = 82
        /// </summary>
        LocalError = 82,

        /// <summary>
        ///     Indicates that the Ldap client encountered errors when encoding an
        ///     Ldap request intended for the Ldap server.
        ///     ENCODING_ERROR = 83
        /// </summary>
        EncodingError = 83,

        /// <summary>
        ///     Indicates that the Ldap client encountered errors when decoding an
        ///     Ldap response from the Ldap server.
        ///     DECODING_ERROR = 84
        /// </summary>
        DecodingError = 84,

        /// <summary>
        ///     Indicates that the time limit of the Ldap client was exceeded while
        ///     waiting for a result.
        ///     Ldap_TIMEOUT = 85
        /// </summary>
        LdapTimeout = 85,

        /// <summary>
        ///     Indicates that a bind method was called with an unknown
        ///     authentication method.
        ///     AUTH_UNKNOWN = 86
        /// </summary>
        AuthUnknown = 86,

        /// <summary>
        ///     Indicates that the search method was called with an invalid
        ///     search filter.
        ///     FILTER_ERROR = 87
        /// </summary>
        FilterError = 87,

        /// <summary>
        ///     Indicates that the user cancelled the Ldap operation.
        ///     USER_CANCELLED = 88
        /// </summary>
        UserCancelled = 88,

        /// <summary>
        ///     Indicates that a dynamic memory allocation method failed when calling
        ///     an Ldap method.
        ///     NO_MEMORY = 90
        /// </summary>
        NoMemory = 90,

        /// <summary>
        ///     Indicates that the Ldap client has lost either its connection or
        ///     cannot establish a connection to the Ldap server.
        ///     CONNECT_ERROR = 91
        /// </summary>
        ConnectError = 91,

        /// <summary>
        ///     Indicates that the requested functionality is not supported by the
        ///     client. For example, if the Ldap client is established as an Ldapv2
        ///     client, the libraries set this error code when the client requests
        ///     Ldapv3 functionality.
        ///     Ldap_NOT_SUPPORTED = 92
        /// </summary>
        LdapNotSupported = 92,

        /// <summary>
        ///     Indicates that the client requested a control that the libraries
        ///     cannot find in the list of supported controls sent by the Ldap server.
        ///     CONTROL_NOT_FOUND = 93
        /// </summary>
        ControlNotFound = 93,

        /// <summary>
        ///     Indicates that the Ldap server sent no results.
        ///     NO_RESULTS_RETURNED = 94
        /// </summary>
        NoResultsReturned = 94,

        /// <summary>
        ///     Indicates that more results are chained in the result message.
        ///     MORE_RESULTS_TO_RETURN = 95
        /// </summary>
        MoreResultsToReturn = 95,

        /// <summary>
        ///     Indicates the Ldap libraries detected a loop. Usually this happens
        ///     when following referrals.
        ///     CLIENT_LOOP = 96
        /// </summary>
        ClientLoop = 96,

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
        ReferralLimitExceeded = 97,

        /// <summary>
        ///     Indicates that the server response to a request is invalid.
        ///     INVALID_RESPONSE = 100
        /// </summary>
        InvalidResponse = 100,

        /// <summary>
        ///     Indicates that the server response to a request is ambiguous.
        ///     AMBIGUOUS_RESPONSE = 101
        /// </summary>
        AmbiguousResponse = 101,

        /// <summary>
        ///     Indicates that TLS is not supported on the server.
        ///     TLS_NOT_SUPPORTED = 112
        /// </summary>
        TlsNotSupported = 112,

        /// <summary>
        ///     Indicates that SSL Handshake could not succeed.
        ///     SSL_HANDSHAKE_FAILED = 113
        /// </summary>
        SslHandshakeFailed = 113,

        /// <summary>
        ///     Indicates that SSL Provider could not be found.
        ///     SSL_PROVIDER_NOT_FOUND = 114
        /// </summary>
        SslProviderNotFound = 114,
    }

}
#endif