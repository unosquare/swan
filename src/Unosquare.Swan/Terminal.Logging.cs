using System.IO;

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
    /// <param name="ex">The optional exception.</param>
    public delegate void OnMessageLoggedCallback(ulong sequence, LoggingMessageType messageType, DateTime utcDate, string source, string message, Exception ex);

    partial class Terminal
    {
        private static ulong LoggingSequence;

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="ex">The optional exception.</param>
        private static void LogMessage(LoggingMessageType messageType, string text, string source, Exception ex)
        {
            lock (SyncLock)
            {
                ConsoleColor color;
                string prefix;

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

                var output = string.IsNullOrWhiteSpace(text) ? string.Empty : text.RemoveControlCharsExcept('\n');
                var outputWithSource = string.IsNullOrWhiteSpace(source) ? output : $"[{source}] {output}";
                var outputText = string.IsNullOrWhiteSpace(Settings.LoggingTimeFormat) ?
                    $" {prefix} >> {outputWithSource}" :
                    $" {date.ToLocalTime().ToString(Settings.LoggingTimeFormat)} {prefix} >> {outputWithSource}";

                // Log the message asynchronously
                if (Settings.OnMessageLogged != null)
                    Task.Factory.StartNew(() =>
                    {
                        try { Settings.OnMessageLogged?.Invoke(sequence, messageType, date, source, output, ex); }
                        catch
                        {
                            // ignored
                        }
                    });

                // Enqueue the message to the console (out or error)
                if (IsConsolePresent && Settings.ConsoleOptions.HasFlag(messageType))
                {
                    var writer = Console.Out;
                    if (messageType.HasFlag(LoggingMessageType.Error))
                    {
                        writer = Console.Error;
                        try
                        {
                            if (ex != null)
                                outputText = $"{outputText}{Environment.NewLine}{ex.Stringify().Indent(4)}";
                        }
                        catch
                        {
                            // Ignore
                        }

                    }

                    outputText.WriteLine(color, writer);
                }
            }
        }

        /// <summary>
        /// Logs a debug message to the console
        /// </summary>
        /// <param name="message">The text.</param>
        public static void Debug(this string message)
        {
            LogMessage(LoggingMessageType.Debug, message, null, null);
        }

        /// <summary>
        /// Logs a debug message to the console
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        public static void Debug(this string message, string source)
        {
            LogMessage(LoggingMessageType.Debug, message, source, null);
        }

        /// <summary>
        /// Logs a debug message to the console
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        public static void Debug(this Exception ex, string source, string message)
        {
            LogMessage(LoggingMessageType.Debug, message, source, ex);
        }

        /// <summary>
        /// Logs a trace message to the console
        /// </summary>
        /// <param name="message">The text.</param>
        public static void Trace(this string message)
        {
            LogMessage(LoggingMessageType.Trace, message, null, null);
        }

        /// <summary>
        /// Logs a trace message to the console
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        public static void Trace(this string message, string source)
        {
            LogMessage(LoggingMessageType.Trace, message, source, null);
        }

        /// <summary>
        /// Logs a trace message to the console
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        public static void Trace(this Exception ex, string source, string message)
        {
            LogMessage(LoggingMessageType.Trace, message, source, ex);
        }

        /// <summary>
        /// Logs a warning message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Warn(this string text)
        {
            LogMessage(LoggingMessageType.Warning, text, null, null);
        }

        /// <summary>
        /// Logs a warning message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public static void Warn(this string text, string source)
        {
            LogMessage(LoggingMessageType.Warning, text, source, null);
        }

        /// <summary>
        /// Logs a warning message to the console
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        public static void Warn(this Exception ex, string source, string message)
        {
            LogMessage(LoggingMessageType.Warning, message, source, ex);
        }

        /// <summary>
        /// Logs an info message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Info(this string text)
        {
            LogMessage(LoggingMessageType.Info, text, null, null);
        }

        /// <summary>
        /// Logs an info message to the console
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public static void Info(this string text, string source)
        {
            LogMessage(LoggingMessageType.Info, text, source, null);
        }

        /// <summary>
        /// Logs an info message to the console
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        public static void Info(this Exception ex, string source, string message)
        {
            LogMessage(LoggingMessageType.Info, message, source, ex);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Error(this string text)
        {
            LogMessage(LoggingMessageType.Error, text, null, null);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public static void Error(this string text, string source)
        {
            LogMessage(LoggingMessageType.Error, text, source, null);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        public static void Error(this Exception ex, string source, string message)
        {
            LogMessage(LoggingMessageType.Error, message, source, ex);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        public static void Log(this Exception ex, string source = null, string message = null)
        {
            LogMessage(LoggingMessageType.Error, message ?? ex.Message, source ?? ex.Source, ex);
        }
    }
}
