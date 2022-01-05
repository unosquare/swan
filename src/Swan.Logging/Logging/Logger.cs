namespace Swan.Logging
{
    using Swan.Extensions;
    using Swan.Formatters;
    using Swan.Platform;
    using Swan.Reflection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Entry-point for logging. Use this static class to register/unregister
    /// loggers instances. By default, the <see cref="ConsoleLogger"/> is registered
    /// if the presence of a console is detected.
    /// </summary>
    public static class Logger
    {
        private static readonly object SyncLock = new();
        private static readonly List<ILogger> Loggers = new();

        private static ulong _loggingSequence;

        static Logger()
        {
            if (Terminal.IsConsolePresent)
                Loggers.Add(ConsoleLogger.Instance);

            if (DebugLogger.IsDebuggerAttached)
                Loggers.Add(DebugLogger.Instance);
        }

        #region Standard Public API

        /// <summary>
        /// Registers the logger.
        /// </summary>
        /// <typeparam name="T">The type of logger to register.</typeparam>
        /// <exception cref="InvalidOperationException">There is already a logger with that class registered.</exception>
        public static void RegisterLogger<T>()
            where T : ILogger
        {
            lock (SyncLock)
            {
                var loggerInstance = Loggers.FirstOrDefault(x => x.GetType() == typeof(T));

                if (loggerInstance != null)
                    throw new InvalidOperationException("There is already a logger with that class registered.");

                Loggers.Add(TypeManager.CreateInstance<T>());
            }
        }

        /// <summary>
        /// Registers the logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public static void RegisterLogger(ILogger logger)
        {
            lock (SyncLock)
                Loggers.Add(logger);
        }

        /// <summary>
        /// Unregisters the logger.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentOutOfRangeException">logger.</exception>
        public static void UnregisterLogger(ILogger logger) => RemoveLogger(x => x == logger);

        /// <summary>
        /// Unregisters the logger.
        /// </summary>
        /// <typeparam name="T">The type of logger to unregister.</typeparam>
        public static void UnregisterLogger<T>() => RemoveLogger(x => x.GetType() == typeof(T));

        /// <summary>
        /// Remove all the loggers.
        /// </summary>
        public static void NoLogging()
        {
            lock (SyncLock)
                Loggers.Clear();
        }

        #region Debug

        /// <summary>
        /// Logs a debug message to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Debug(
            this string message,
            string? source = null,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Debug, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a debug message to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Debug(
            this string message,
            Type source,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Debug, message, source?.FullName, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a debug message to the console.
        /// </summary>
        /// <param name="extendedData">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Debug(
            this Exception extendedData,
            string source,
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Debug, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion

        #region Trace

        /// <summary>
        /// Logs a trace message to the console.
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Trace(
            this string message,
            string? source = null,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Trace, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a trace message to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Trace(
            this string message,
            Type source,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Trace, message, source?.FullName, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a trace message to the console.
        /// </summary>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Trace(
            this Exception extendedData,
            string source,
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Trace, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion

        #region Warn

        /// <summary>
        /// Logs a warning message to the console.
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Warn(
            this string message,
            string? source = null,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Warning, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a warning message to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Warn(
            this string message,
            Type source,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Warning, message, source?.FullName, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a warning message to the console.
        /// </summary>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Warn(
            this Exception extendedData,
            string source,
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Warning, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion

        #region Fatal

        /// <summary>
        /// Logs a warning message to the console.
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Fatal(
            this string message,
            string? source = null,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Fatal, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a warning message to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Fatal(
            this string message,
            Type source,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Fatal, message, source?.FullName, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a warning message to the console.
        /// </summary>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Fatal(
            this Exception extendedData,
            string source,
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Fatal, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion

        #region Info

        /// <summary>
        /// Logs an info message to the console.
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Info(
            this string message,
            string? source = null,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Info, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an info message to the console.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Info(
            this string message,
            Type source,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Info, message, source?.FullName, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an info message to the console.
        /// </summary>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Info(
            this Exception extendedData,
            string source,
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Info, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion

        #region Error

        /// <summary>
        /// Logs an error message to the console's standard error.
        /// </summary>
        /// <param name="message">The text.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Error(
            this string message,
            string? source = null,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Error, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an error message to the console's standard error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Error(
            this string message,
            Type source,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Error, message, source?.FullName, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an error message to the console's standard error.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Error(
            this Exception ex,
            string source,
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(LogLevel.Error, message, source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion

        #endregion

        #region Extended Public API

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Log(
            this string message,
            string source,
            LogLevel messageType,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(messageType, message, source, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Log(
            this string message,
            Type source,
            LogLevel messageType,
            object? extendedData = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            LogMessage(messageType, message, source?.FullName, extendedData, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an error message to the console's standard error.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Log(
            this Exception? ex,
            string? source = null,
            string? message = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (ex is null)
                return;

            LogMessage(LogLevel.Error, message ?? ex.Message, source ?? ex.Source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs an error message to the console's standard error.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Log(
            this Exception? ex,
            Type? source = null,
            string? message = null,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (ex is null)
                return;

            LogMessage(LogLevel.Error, message ?? ex.Message, source?.FullName ?? ex.Source, ex, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a trace message showing all possible non-null properties of the given object
        /// This method is expensive as it uses Stringify internally.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="text">The title.</param>
        /// <param name="callerMemberName">Name of the caller member. This is automatically populated.</param>
        /// <param name="callerFilePath">The caller file path. This is automatically populated.</param>
        /// <param name="callerLineNumber">The caller line number. This is automatically populated.</param>
        public static void Dump(
            this object? obj,
            string source,
            string text = "Object Dump",
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (obj == null)
                return;

            var message = $"{text} ({obj.GetType()}): {Environment.NewLine}{obj.Stringify().Indent(5)}";
            LogMessage(LogLevel.Trace, message, source, obj, callerMemberName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        /// Logs a trace message showing all possible non-null properties of the given object
        /// This method is expensive as it uses Stringify internally.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="source">The source.</param>
        /// <param name="text">The text.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public static void Dump(
            this object? obj,
            Type source,
            string text = "Object Dump",
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            if (obj == null)
                return;

            var message = $"{text} ({obj.GetType()}): {Environment.NewLine}{obj.Stringify().Indent(5)}";
            LogMessage(LogLevel.Trace, message, source?.FullName, obj, callerMemberName, callerFilePath, callerLineNumber);
        }

        #endregion

        /// <summary>
        /// Indents the specified multi-line text with the given amount of leading spaces
        /// per line.
        /// </summary>
        /// <param name="value">The text.</param>
        /// <param name="spaces">The spaces.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        internal static string Indent(this string? value, int spaces = 4)
        {
            value ??= string.Empty;
            if (spaces <= 0) return value;

            var lines = value.ToLines();
            var builder = new StringBuilder();
            var indentStr = new string(' ', spaces);

            foreach (var line in lines)
            {
                builder.AppendLine($"{indentStr}{line}");
            }

            return builder.ToString().TrimEnd();
        }

        private static void RemoveLogger(Func<ILogger, bool> criteria)
        {
            lock (SyncLock)
            {
                var loggerInstance = Loggers.FirstOrDefault(criteria);

                if (loggerInstance == null)
                    throw new InvalidOperationException("The logger is not registered.");

                loggerInstance.Dispose();

                Loggers.Remove(loggerInstance);
            }
        }

        private static void LogMessage(
            LogLevel logLevel,
            string message,
            string? sourceName,
            object? extendedData,
            string callerMemberName,
            string callerFilePath,
            int callerLineNumber)
        {
            var sequence = _loggingSequence;
            var date = DateTime.UtcNow;
            _loggingSequence++;

            var loggerMessage = string.IsNullOrWhiteSpace(message) ?
                string.Empty : message.RemoveControlChars('\n');

            var eventArgs = new LogMessageReceivedEventArgs(
                sequence,
                logLevel,
                date,
                sourceName,
                loggerMessage,
                extendedData,
                callerMemberName,
                callerFilePath,
                callerLineNumber);

            Parallel.ForEach(Loggers, (logger) =>
            {
                if (logLevel < logger.LogLevel)
                    return;

                logger.Log(eventArgs);
            });
        }
    }
}
