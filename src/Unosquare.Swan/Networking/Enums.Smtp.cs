// ReSharper disable InconsistentNaming
namespace Unosquare.Swan.Networking
{
#if NETSTANDARD1_3 || UWP

    /// <summary>
    /// Defines the different SMTP status codes
    /// </summary>
    public enum SmtpStatusCode
    {
        /// <summary>
        /// System code
        /// </summary>
        SystemStatus = 211,
        
        /// <summary>
        /// Help message code
        /// </summary>
        HelpMessage = 214,
        
        /// <summary>
        /// Service ready code
        /// </summary>
        ServiceReady = 220,
        
        /// <summary>
        /// Service closing channel code
        /// </summary>
        ServiceClosingTransmissionChannel = 221,
        
        /// <summary>
        /// OK Code
        /// </summary>
        Ok = 250,
        
        /// <summary>
        /// User not local code
        /// </summary>
        UserNotLocalWillForward = 251,
        
        /// <summary>
        /// Cannot verify user code
        /// </summary>
        CannotVerifyUserWillAttemptDelivery = 252,
        
        /// <summary>
        /// Start Mail Input code
        /// </summary>
        StartMailInput = 354,
        
        /// <summary>
        /// Service Not Available code
        /// </summary>
        ServiceNotAvailable = 421,
        
        /// <summary>
        /// Mailbox Busy code
        /// </summary>
        MailboxBusy = 450,
        
        /// <summary>
        /// Local Error code
        /// </summary>
        LocalErrorInProcessing = 451,
        
        /// <summary>
        /// Insufficient storage code
        /// </summary>
        InsufficientStorage = 452,
        
        /// <summary>
        /// Client not permitted code
        /// </summary>
        ClientNotPermitted = 454,
        
        /// <summary>
        /// 
        /// </summary>
        CommandUnrecognized = 500,
        
        /// <summary>
        /// Syntax error
        /// </summary>
        SyntaxError = 501,
        
        /// <summary>
        /// 
        /// </summary>
        CommandNotImplemented = 502,
        
        /// <summary>
        /// 
        /// </summary>
        BadCommandSequence = 503,
        
        /// <summary>
        /// 
        /// </summary>
        MustIssueStartTlsFirst = 530,
        
        /// <summary>
        /// 
        /// </summary>
        CommandParameterNotImplemented = 504,
        
        /// <summary>
        /// 
        /// </summary>
        MailboxUnavailable = 550,
        
        /// <summary>
        /// 
        /// </summary>
        UserNotLocalTryAlternatePath = 551,

        /// <summary>
        /// Exceeded Storage Allocation code
        /// </summary>
        ExceededStorageAllocation = 552,
        
        /// <summary>
        /// Mailbox name not allowed code
        /// </summary>
        MailboxNameNotAllowed = 553,
        
        /// <summary>
        /// Transaction failed code
        /// </summary>
        TransactionFailed = 554,
        
        /// <summary>
        /// General Failure code
        /// </summary>
        GeneralFailure = -1,
    }
#endif

    /// <summary>
    /// Enumerates all of the well-known SMTP command names.
    /// </summary>
    public enum SmtpCommandNames
    {
        /// <summary>
        /// An unknown command
        /// </summary>
        Unknown,
        
        /// <summary>
        /// The helo command
        /// </summary>
        HELO,
        
        /// <summary>
        /// The ehlo command
        /// </summary>
        EHLO,
        
        /// <summary>
        /// The quit command
        /// </summary>
        QUIT,
        
        /// <summary>
        /// The help command
        /// </summary>
        HELP,
        
        /// <summary>
        /// The noop command
        /// </summary>
        NOOP,
        
        /// <summary>
        /// The rset command
        /// </summary>
        RSET,
        
        /// <summary>
        /// The mail command
        /// </summary>
        MAIL,
        
        /// <summary>
        /// The data command
        /// </summary>
        DATA,
        
        /// <summary>
        /// The send command
        /// </summary>
        SEND,
        
        /// <summary>
        /// The soml command
        /// </summary>
        SOML,
        
        /// <summary>
        /// The saml command
        /// </summary>
        SAML,
        
        /// <summary>
        /// The RCPT command
        /// </summary>
        RCPT,
        
        /// <summary>
        /// The vrfy command
        /// </summary>
        VRFY,
        
        /// <summary>
        /// The expn command
        /// </summary>
        EXPN,
        
        /// <summary>
        /// The starttls command
        /// </summary>
        STARTTLS,
        
        /// <summary>
        /// The authentication command
        /// </summary>
        AUTH,
    }

    /// <summary>
    /// Enumerates the reply code severities
    /// </summary>
    public enum SmtpReplyCodeSeverities
    {
        /// <summary>
        /// The unknown severity
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// The positive completion severity
        /// </summary>
        PositiveCompletion = 200,
        
        /// <summary>
        /// The positive intermediate severity
        /// </summary>
        PositiveIntermediate = 300,
        
        /// <summary>
        /// The transient negative severity
        /// </summary>
        TransientNegative = 400,
        
        /// <summary>
        /// The permanent negative severity
        /// </summary>
        PermanentNegative = 500
    }

    /// <summary>
    /// Enumerates the reply code categories
    /// </summary>
    public enum SmtpReplyCodeCategories
    {
        /// <summary>
        /// The unknown category
        /// </summary>
        Unknown = -1,
        
        /// <summary>
        /// The syntax category
        /// </summary>
        Syntax = 0,
        
        /// <summary>
        /// The information category
        /// </summary>
        Information = 1,
        
        /// <summary>
        /// The connections category
        /// </summary>
        Connections = 2,
        
        /// <summary>
        /// The unspecified a category
        /// </summary>
        UnspecifiedA = 3,
        
        /// <summary>
        /// The unspecified b category
        /// </summary>
        UnspecifiedB = 4,
        
        /// <summary>
        /// The system category
        /// </summary>
        System = 5,
    }
}