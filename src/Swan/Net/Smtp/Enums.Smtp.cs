// ReSharper disable InconsistentNaming
namespace Swan.Net.Smtp
{
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
    /// Enumerates the reply code severities.
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
        PermanentNegative = 500,
    }

    /// <summary>
    /// Enumerates the reply code categories.
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