namespace Unosquare.Swan
{
    using System;
    using System.IO;
    using System.Net.Mail;
    using System.Reflection;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class SmtpExtensions
    {
        private static readonly BindingFlags PrivateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>
        /// The raw contents of this MailMessage as a MemoryStream.
        /// </summary>
        /// <param name="this">The caller.</param>
        /// <returns>A MemoryStream with the raw contents of this MailMessage.</returns>
        public static MemoryStream ToMimeMessage(this MailMessage @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            
            var result = new MemoryStream();
            var mailWriter = MimeMessageConstants.MailWriterConstructor.Invoke(new object[] { result });
            MimeMessageConstants.SendMethod.Invoke(
                @this, 
                PrivateInstanceFlags, 
                null, 
                MimeMessageConstants.IsRunningInDotNetFourPointFive ? new[] { mailWriter, true, true } : new[] { mailWriter, true }, 
                null);

            result = new MemoryStream(result.ToArray());
            MimeMessageConstants.CloseMethod.Invoke(
                mailWriter, 
                PrivateInstanceFlags, 
                null,
                Array.Empty<object>(), 
                null);
            result.Position = 0;
            return result;
        }

        internal static class MimeMessageConstants
        {
#pragma warning disable DE0005 // API is deprecated
            public static readonly Type MailWriter = typeof(SmtpClient).Assembly.GetType("System.Net.Mail.MailWriter");
#pragma warning restore DE0005 // API is deprecated
            public static readonly ConstructorInfo MailWriterConstructor = MailWriter.GetConstructor(PrivateInstanceFlags, null, new[] { typeof(Stream) }, null);
            public static readonly MethodInfo CloseMethod = MailWriter.GetMethod("Close", PrivateInstanceFlags);
            public static readonly MethodInfo SendMethod = typeof(MailMessage).GetMethod("Send", PrivateInstanceFlags);
            public static readonly bool IsRunningInDotNetFourPointFive = SendMethod.GetParameters().Length == 3;
        }
    }
}