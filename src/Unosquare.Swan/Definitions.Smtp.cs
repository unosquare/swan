namespace Unosquare.Swan
{
    partial class Definitions
    {
        /// <summary>
        /// The string sequence that delimits the end of the DATA command
        /// </summary>
        public const string SmtpDataCommandTerminator = "\r\n.\r\n";

        /// <summary>
        /// Defines the groupable command names according to:
        /// https://tools.ietf.org/html/rfc2920
        /// </summary>
        public static readonly SmtpCommandNames[] SmtpGroupableCommandNames =
        {
            SmtpCommandNames.RSET,
            SmtpCommandNames.MAIL, // FROM:
            SmtpCommandNames.SEND, // FROM:
            SmtpCommandNames.SOML, // FROM:
            SmtpCommandNames.SAML, // FROM:
            SmtpCommandNames.RCPT // TO:
        };

        /// <summary>
        /// The stateless command names (i.e. the commands that don't require initiation from the client)
        /// </summary>
        public static readonly SmtpCommandNames[] SmtpStatelessCommandNames =
        {
            SmtpCommandNames.NOOP,
            SmtpCommandNames.HELP,
            SmtpCommandNames.EXPN,
            SmtpCommandNames.VRFY,
            SmtpCommandNames.RSET
        };

        /// <summary>
        /// Lists the AUTH methods supported by default.
        /// </summary>
        public class SmtpAuthMethods
        {
            /// <summary>
            /// The plain method
            /// </summary>
            public const string Plain = "PLAIN";
            /// <summary>
            /// The login method
            /// </summary>
            public const string Login = "LOGIN";
        }
    }
}
