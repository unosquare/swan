namespace Unosquare.Swan
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    partial class Terminal
    {
        private static ulong LoggingSequence;

        #region Events

        /// <summary>
        /// Occurs asynchronously, whenever a logging message is received by the terminal.
        /// Only called when Terminal writes data via Info, Error, Trace, Warn, Debug methods, regardless of whether or not
        /// the console is present. Subscribe to this event to pass data on to your own logger.
        /// </summary>
        public static event LogMessageReceivedEventHandler OnLogMessageReceived;

        /// <summary>
        /// Occurs synchronously (so handle quickly), whenever a logging message is about to be enqueued to the
        /// console output. Setting the CancelOutput to true in the event arguments prevents the
        /// logging message to be written out to the console.
        /// Message filtering only works with loggign methods such as Trace, Debug, Info, Warn, Error and Dump
        /// Standard Write methods do not get filtering capabilities.
        /// </summary>
        public static event LogMessageDisplayingEventHandler OnLogMessageDisplaying;

        #endregion

        #region Main Logging Method

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="ex">The optional exception.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        private static void LogMessage(LogMessageType messageType, string message, string source, Exception ex,
            string callerMemberName, 
            string callerFilePath, 
            int callerLineNumber)
        {
            lock (SyncLock)
            {
                ConsoleColor color;
                string prefix;

                switch (messageType)
                {
                    case LogMessageType.Debug:
                        color = Settings.DebugColor;
                        prefix = Settings.DebugPrefix;
                        break;
                    case LogMessageType.Error:
                        color = Settings.ErrorColor;
                        prefix = Settings.ErrorPrefix;
                        break;
                    case LogMessageType.Info:
                        color = Settings.InfoColor;
                        prefix = Settings.InfoPrefix;
                        break;
                    case LogMessageType.Trace:
                        color = Settings.TraceColor;
                        prefix = Settings.TracePrefix;
                        break;
                    case LogMessageType.Warning:
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

                var output = string.IsNullOrWhiteSpace(message) ? string.Empty : message.RemoveControlCharsExcept('\n');
                var outputWithSource = string.IsNullOrWhiteSpace(source) ? output : $"[{source}] {output}";
                var outputText = string.IsNullOrWhiteSpace(Settings.LoggingTimeFormat) ?
                    $" {prefix} >> {outputWithSource}" :
                    $" {date.ToLocalTime().ToString(Settings.LoggingTimeFormat)} {prefix} >> {outputWithSource}";

                // Log the message asynchronously
                var eventArgs = new LogMessageReceivedEventArgs(sequence, messageType, date, source, output, ex, callerMemberName, 
                    callerFilePath, callerLineNumber);

                if (OnLogMessageReceived != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try { OnLogMessageReceived(source, eventArgs); }
                        catch
                        {
                            // ignored
                        }
                    });
                }


                // Enqueue the message to the console (out or error)
                // If we don't have the display flag on the message type, don't wnqueue it.
                if (!IsConsolePresent || !Settings.DisplayLoggingMessageType.HasFlag(messageType))
                    return;

                // Select and format error output
                var writer = Console.Out;
                if (messageType.HasFlag(LogMessageType.Error))
                {
                    writer = Console.Error;
                    try
                    {
                        if (ex != null)
                            outputText = $"{outputText}{Environment.NewLine}{ex.Stringify().Indent(4)}";
                    }
                    catch { /* Ignore */ }

                }

                // Filter output messages via events
                var displayingEventArgs = new LogMessageDisplayingEventArgs(eventArgs);
                OnLogMessageDisplaying?.Invoke(source, displayingEventArgs);
                if (displayingEventArgs.CancelOutput == false)
                    outputText.WriteLine(color, writer);
            }
        }

        #endregion

        #region Standard Public API

        /// <summary>
        /// Logs a debug message to the console
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Debug(this string message, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Debug, message, source, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a debug message to the console
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Debug(this Exception ex, string source, string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Debug, message, source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a trace message to the console
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Trace(this string message, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Trace, message, source, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a trace message to the console
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Trace(this Exception ex, string source, string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Trace, message, source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a warning message to the console
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Warn(this string message, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Warning, message, source, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a warning message to the console
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Warn(this Exception ex, string source, string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Warning, message, source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an info message to the console
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Info(this string message, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Info, message, source, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an info message to the console
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Info(this Exception ex, string source, string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Info, message, source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Error(this string message, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Error, message, source, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Error(this Exception ex, string source, string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Error, message, source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion

        #region Extended Public API

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Log(this string message, string source, LogMessageType messageType,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(messageType, message, source, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an error message to the console's standard error
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Log(this Exception ex, string source = null, string message = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogMessageType.Error, message ?? ex.Message, source ?? ex.Source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a trace message showing all possible non-null properties of the given object
        /// This method is expensive as it uses Stringify internally
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="text">The title.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Dump(this object obj, string text = "Object Data", string source = nameof(Dump),
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (obj == null) return;
            var message = $"{text} ({obj.GetType()}): {Environment.NewLine}{obj.Stringify().Indent(5)}";
            LogMessage(LogMessageType.Trace, message, source, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion
    }
}
