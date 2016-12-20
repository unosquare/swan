namespace Unosquare.Swan
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a callback to be invoked asynchronously when a logging message arrives to the terminal.
    /// </summary>
    /// <param name="sequence">The logging message sequence.</param>
    /// <param name="messageType">Type of the message.</param>
    /// <param name="utcDate">The UTC date.</param>
    /// <param name="source">The source.</param>
    /// <param name="message">The text.</param>
    public delegate void OnMessageLoggedCallback(ulong sequence, LoggingMessageType messageType, DateTime utcDate, string source, string message);

    partial class Terminal
    {
        private static ulong LoggingSequence = 0;

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        private static void LogMessage(LoggingMessageType messageType, string text, string source)
        {
            lock (SyncLock)
            {
                var color = Settings.DefaultColor;
                string prefix = string.Empty;

                switch (messageType)
                {
                    case LoggingMessageType.Debug:
                        color = Settings.DebugColor;
                        prefix = Settings.DebugPrefix;
                        break;
                    case LoggingMessageType.Error:
                        color = Settings.ErrorColor;
                        prefix = Settings.ErrorPrefix;
                        break;
                    case LoggingMessageType.Info:
                        color = Settings.InfoColor;
                        prefix = Settings.InfoPrefix;
                        break;
                    case LoggingMessageType.Trace:
                        color = Settings.TraceColor;
                        prefix = Settings.TracePrefix;
                        break;
                    case LoggingMessageType.Warning:
                        color = Settings.WarnColor;
                        prefix = Settings.WarnPrefix;
                        break;
                    default:
                        color = Settings.DefaultColor;
                        prefix = new string(' ', Settings.InfoPrefix.Length);
                        break;
                }

                var sequence = LoggingSequence;
                var date = DateTime.UtcNow;
                LoggingSequence++;

                var output = string.IsNullOrWhiteSpace(text) ? string.Empty : text.RemoveControlChars().Trim();
                var outputWithSource = string.IsNullOrWhiteSpace(source) ? output : $"[{source}] {output}";
                var outputText = string.IsNullOrWhiteSpace(Settings.LoggingTimeFormat) ?
                    $" {prefix} >> {outputWithSource}" :
                    $" {date.ToLocalTime().ToString(Settings.LoggingTimeFormat)} {prefix} >> {outputWithSource}";

                // Log the message asynchronously
                if (Settings.OnMessageLogged != null)
                    Task.Factory.StartNew(() =>
                    {
                        try { Settings.OnMessageLogged?.Invoke(sequence, messageType, date, source, output); }
                        catch
                        {
                            // ignored
                        }
                    });

                // Enqueue the message to the console (out or error)
                if (IsConsolePresent && Settings.ConsoleOptions.HasFlag(messageType))
                {
                    var writer = messageType == LoggingMessageType.Error ? Console.Error : Console.Out;
                    outputText.WriteLine(color, writer);
                }
            }
        }

        /// <summary>
        /// Logs a debug message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Debug(this string text)
        {
            LogMessage(LoggingMessageType.Debug, text, null);
        }

        /// <summary>
        /// Logs a debug message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public static void Debug(this string text, string source)
        {
            LogMessage(LoggingMessageType.Debug, text, source);
        }

        /// <summary>
        /// Logs a trace message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Trace(this string text)
        {
            LogMessage(LoggingMessageType.Trace, text, null);
        }

        /// <summary>
        /// Logs a trace message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public static void Trace(this string text, string source)
        {
            LogMessage(LoggingMessageType.Trace, text, source);
        }

        /// <summary>
        /// Logs a warning message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Warn(this string text)
        {
            LogMessage(LoggingMessageType.Warning, text, null);
        }

        /// <summary>
        /// Logs a warning message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public static void Warn(this string text, string source)
        {
            LogMessage(LoggingMessageType.Warning, text, source);
        }

        /// <summary>
        /// Logs an info message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Info(this string text)
        {
            LogMessage(LoggingMessageType.Info, text, null);
        }

        /// <summary>
        /// Logs an info message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public static void Info(this string text, string source)
        {
            LogMessage(LoggingMessageType.Info, text, source);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Error(this string text)
        {
            LogMessage(LoggingMessageType.Error, text, null);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public static void Error(this string text, string source)
        {
            LogMessage(LoggingMessageType.Error, text, source);
        }
    }
}
