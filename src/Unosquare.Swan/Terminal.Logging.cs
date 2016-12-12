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

            switch(messageType)
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
                if (string.IsNullOrWhiteSpace(Settings.LoggingTimeFormat))
                    $" {prefix} >> {output}".WriteLine(color);
                else
                    $" {DateTime.Now.ToString(Settings.LoggingTimeFormat)} {prefix} >> {output}".WriteLine(color);
            }

            // TODO: Implement a logging callback
        }

        public static void Debug(this string text)
        {
            LogMessage(LoggingMessageType.Debug, text, null);
        }

        public static void Debug(this string text, string source)
        {
            LogMessage(LoggingMessageType.Debug, text, source);
        }

        public static void Trace(this string text)
        {
            LogMessage(LoggingMessageType.Trace, text, null);
        }

        public static void Trace(this string text, string source)
        {
            LogMessage(LoggingMessageType.Trace, text, source);
        }

        public static void Warn(this string text)
        {
            LogMessage(LoggingMessageType.Warning, text, null);
        }

        public static void Warn(this string text, string source)
        {
            LogMessage(LoggingMessageType.Warning, text, source);
        }

        public static void Info(this string text)
        {
            LogMessage(LoggingMessageType.Info, text, null);
        }

        public static void Info(this string text, string source)
        {
            LogMessage(LoggingMessageType.Info, text, source);
        }

        public static void Error(this string text)
        {
            LogMessage(LoggingMessageType.Error, text, null);
        }

        public static void Error(this string text, string source)
        {
            LogMessage(LoggingMessageType.Error, text, source);
        }
    }
}
