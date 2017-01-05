namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    partial class Terminal
    {
        const string InnerMessage = nameof(InnerMessage);

        #region Private Declarations

        private static ulong LoggingSequence;

        #endregion

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
        /// Message filtering only works with logging methods such as Trace, Debug, Info, Warn, Error and Dump
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
        /// <param name="properties">The properties.</param>
        private static void LogMessage(LogMessageType messageType, string message, string source, Exception ex,
            string callerMemberName,
            string callerFilePath,
            int callerLineNumber,
            IDictionary<string, object> properties = null)
        {
            lock (SyncLock)
            {
                #region Color and Prefix

                ConsoleColor color;
                string prefix;

                // Select color and prefix based on message type
                // and settings
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

                #endregion

                #region Create and Format the Output

                var sequence = LoggingSequence;
                var date = DateTime.UtcNow;
                LoggingSequence++;

                var loggerMessage = string.IsNullOrWhiteSpace(message) ? 
                    string.Empty : message.RemoveControlCharsExcept('\n');

                var outputMessage = string.IsNullOrWhiteSpace(source) ? loggerMessage : $"[{source}] {loggerMessage}";
                outputMessage = string.IsNullOrWhiteSpace(Settings.LoggingTimeFormat) ?
                    $" {prefix} >> {outputMessage}" :
                    $" {date.ToLocalTime().ToString(Settings.LoggingTimeFormat)} {prefix} >> {outputMessage}";

                // Log the message asynchronously with the appropriate event args
                var eventArgs = new LogMessageReceivedEventArgs(sequence, messageType, date, source, loggerMessage, ex, callerMemberName,
                    callerFilePath, callerLineNumber, properties);

                #endregion

                #region Fire Up External Logging Logic (Asynchronously)

                if (OnLogMessageReceived != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try { OnLogMessageReceived?.Invoke(source, eventArgs); }
                        catch { /* Ignore */ }
                    });
                }

                #endregion

                #region Display the Message by Writing to the Output Queue

                // Check if we are skipping these messages to be displayed based on settings
                if (Settings.DisplayLoggingMessageType.HasFlag(messageType) == false)
                    return;

                // Select the writer based on the message type
                var writer = IsConsolePresent ?
                    messageType.HasFlag(LogMessageType.Error) ?
                        TerminalWriters.StandardError : TerminalWriters.StandardOutput
                    : TerminalWriters.None;

                // Set the writer to Diagnostics if appropriate (Error and Debugging data go to the Diagnostics debugger
                // if it is attached at all
                if (IsDebuggerAttached
                    && (IsConsolePresent == false || messageType.HasFlag(LogMessageType.Debug) || messageType.HasFlag(LogMessageType.Error)))
                    writer = writer | TerminalWriters.Diagnostics;

                // Check if we really need to write this out
                if (writer == TerminalWriters.None) return;

                // Further format the output in the case there is an exception being logged
                if (writer.HasFlag(TerminalWriters.StandardError) && ex != null)
                {
                    try { outputMessage = $"{outputMessage}{Environment.NewLine}{ex.Stringify().Indent(4)}"; }
                    catch { /* Ignore */ }
                }

                // Filter output messages via events
                var displayingEventArgs = new LogMessageDisplayingEventArgs(eventArgs);
                OnLogMessageDisplaying?.Invoke(source, displayingEventArgs);
                if (displayingEventArgs.CancelOutput == false)
                    outputMessage.WriteLine(color, writer);

                #endregion
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

        /// <summary>
        /// Add a param previous to send to a log method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="paramKey">The parameter key.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <returns></returns>
        public static IDictionary<string, object> WithParam(this string message, string paramKey, object paramValue)
        {
            return new Dictionary<string, object>
            {
                { InnerMessage, message },
                { paramKey, paramValue }
            };
        }

        /// <summary>
        /// Logs an info message to the console using a dictionary
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Info(this IDictionary<string, object> values, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (values.ContainsKey(InnerMessage))
            {
                var message = values[InnerMessage].ToString();
                values.Remove(InnerMessage);

                LogMessage(LogMessageType.Info, message, source, null, callerMemberName, callerFilePath, callerLineNumber, values);
            }
        }

        /// <summary>
        /// Logs a debug message to the console using a dictionary
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Debug(this IDictionary<string, object> values, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (values.ContainsKey(InnerMessage))
            {
                var message = values[InnerMessage].ToString();
                values.Remove(InnerMessage);

                LogMessage(LogMessageType.Debug, message, source, null, callerMemberName, callerFilePath, callerLineNumber, values);
            }
        }

        /// <summary>
        /// Logs an error message to the console using a dictionary
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Error(this IDictionary<string, object> values, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (values.ContainsKey(InnerMessage))
            {
                var message = values[InnerMessage].ToString();
                values.Remove(InnerMessage);

                LogMessage(LogMessageType.Error, message, source, null, callerMemberName, callerFilePath, callerLineNumber, values);
            }
        }

        /// <summary>
        /// Logs a warn message to the console using a dictionary
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Warn(this IDictionary<string, object> values, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (values.ContainsKey(InnerMessage))
            {
                var message = values[InnerMessage].ToString();
                values.Remove(InnerMessage);

                LogMessage(LogMessageType.Warning, message, source, null, callerMemberName, callerFilePath, callerLineNumber, values);
            }
        }

        /// <summary>
        /// Logs a trace message to the console using a dictionary
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="source">The source.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Trace(this IDictionary<string, object> values, string source = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (values.ContainsKey(InnerMessage))
            {
                var message = values[InnerMessage].ToString();
                values.Remove(InnerMessage);

                LogMessage(LogMessageType.Trace, message, source, null, callerMemberName, callerFilePath, callerLineNumber, values);
            }
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
        /// <param name="properties">The properties.</param>
        public static void Log(this string message, string source, LogMessageType messageType,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0,
            IDictionary<string, object> properties = null)
        {
            LogMessage(messageType, message, source, null, callerMemberName, callerFilePath, callerLineNumber, properties);
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
        /// <param name="properties">The properties.</param>
        public static void Log(this Exception ex, string source = null, string message = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0,
            IDictionary<string, object> properties = null)
        {
            LogMessage(LogMessageType.Error, message ?? ex.Message, source ?? ex.Source, ex, callerMemberName, callerFilePath, callerLineNumber, properties);
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
        /// <param name="properties">The properties.</param>
        public static void Dump(this object obj, string text = "Object Data", string source = nameof(Dump),
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0,
            IDictionary<string, object> properties = null)
        {
            if (obj == null) return;
            var message = $"{text} ({obj.GetType()}): {Environment.NewLine}{obj.Stringify().Indent(5)}";
            LogMessage(LogMessageType.Trace, message, source, null, callerMemberName, callerFilePath, callerLineNumber, properties);
        }

        #endregion
    }
}
