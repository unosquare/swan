using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swan
{
    /// <summary>
    /// Provides methods to perform self-checks in library or application code.
    /// </summary>
    public static class SelfCheck
    {
        /// <summary>
        /// <para>Creates and returns an exception telling that an internal self-check has failed.</para>
        /// <para>The returned exception will be of type <see cref="InternalErrorException"/>; its
        /// <see cref="Exception.Message">Message</see> property will contain the specified
        /// <paramref name="message"/>, preceded by an indication of the assembly, source file,
        /// and line number of the failed check.</para>
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="filePath">The path of the source file where this method is called.
        /// This parameter is automatically added by the compiler amd should never be provided explicitly.</param>
        /// <param name="lineNumber">The line number in source where this method is called.
        /// This parameter is automatically added by the compiler amd should never be provided explicitly.</param>
        /// <returns>
        /// A newly-created instance of <see cref="InternalErrorException"/>.
        /// </returns>
        public static InternalErrorException Failure(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
            => new InternalErrorException(BuildMessage(message, filePath, lineNumber));

        private static string BuildMessage(string message, string filePath, int lineNumber)
        {
            var frames = new StackTrace().GetFrames();
            if (frames == null)
                return message;

            try
            {
                filePath = Path.GetFileName(filePath);
            }
            catch (ArgumentException)
            {
            }

            var frame = frames.FirstOrDefault(f => f.GetMethod().ReflectedType != typeof(SelfCheck));
            var sb = new StringBuilder()
                .Append('[')
                .Append(frame?.GetType().Assembly.GetName().Name ?? "<unknown>");

            if (!string.IsNullOrEmpty(filePath))
            {
                sb.Append(": ").Append(filePath);
                if (lineNumber > 0)
                    sb.Append('(').Append(lineNumber).Append(')');
            }

            return sb.Append("] ").Append(message).ToString();
        }
    }
}