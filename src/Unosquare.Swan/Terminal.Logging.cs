namespace Unosquare.Swan
{
    using System;

    partial class Terminal
    {
        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        private static void LogMessage(LoggingMessageType messageType, string text, string source)
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
            }

            var output = text;
            if (string.IsNullOrWhiteSpace(source) == false)
                output = $"[{source}] {text}";

            if (IsConsolePresent && Settings.ConsoleOptions.HasFlag(messageType))
            {
                var writer = messageType == LoggingMessageType.Error ? Console.Error : Console.Out;
                if (string.IsNullOrWhiteSpace(Settings.LoggingTimeFormat))
                    $" {prefix} >> {output}".WriteLine(color, writer);
                else
                    $" {DateTime.Now.ToString(Settings.LoggingTimeFormat)} {prefix} >> {output}".WriteLine(color, writer);
            }

            // TODO: Implement a logging callback
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
