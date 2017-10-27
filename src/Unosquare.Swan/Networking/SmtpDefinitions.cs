namespace Unosquare.Swan.Networking
{
    /// <summary>
    /// Contains useful constants and definitions
    /// </summary>
    public static class SmtpDefinitions
    {
        /// <summary>
        /// The string sequence that delimits the end of the DATA command
        /// </summary>
        public const string SmtpDataCommandTerminator = "\r\n.\r\n";
        
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
