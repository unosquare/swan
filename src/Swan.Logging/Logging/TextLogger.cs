﻿namespace Swan.Logging;

using Extensions;
using Formatters;
using Platform;

/// <summary>
/// Use this class for text-based loggers.
/// </summary>
public abstract class TextLogger
{
    /// <summary>
    /// Gets or sets the logging time format.
    /// set to null or empty to prevent output.
    /// </summary>
    /// <value>
    /// The logging time format.
    /// </value>
    public static string LoggingTimeFormat { get; set; } = "HH:mm:ss.fff";

    /// <summary>
    /// Gets the color of the output of the message (the output message has a new line char in the end).
    /// </summary>
    /// <param name="logEvent">The <see cref="LogMessageReceivedEventArgs" /> instance containing the event data.</param>
    /// <returns>
    /// The output message formatted and the color of the console to be used.
    /// </returns>
    protected static (string outputMessage, ConsoleColor color) GetOutputAndColor(LogMessageReceivedEventArgs logEvent)
    {
        var (prefix, color) = GetConsoleColorAndPrefix(logEvent.MessageType);

        var loggerMessage = string.IsNullOrWhiteSpace(logEvent.Message)
            ? string.Empty
            : logEvent.Message.RemoveControlChars('\n');

        var outputMessage = CreateOutputMessage(logEvent.Source, loggerMessage, prefix, logEvent.UtcDate);

        // Further format the output in the case there is an exception being logged
        if (logEvent.MessageType == LogLevel.Error && logEvent.Exception != null)
        {
            try
            {
                outputMessage += $"{logEvent.Exception.Stringify().Indent()}{Environment.NewLine}";
            }
            catch
            {
                // Ignore  
            }
        }

        return (outputMessage, color);
    }

    private static (string Prefix, ConsoleColor color) GetConsoleColorAndPrefix(LogLevel messageType) =>
        messageType switch
        {
            LogLevel.Debug => (ConsoleLogger.DebugPrefix, ConsoleLogger.DebugColor),
            LogLevel.Error => (ConsoleLogger.ErrorPrefix, ConsoleLogger.ErrorColor),
            LogLevel.Info => (ConsoleLogger.InfoPrefix, ConsoleLogger.InfoColor),
            LogLevel.Trace => (ConsoleLogger.TracePrefix, ConsoleLogger.TraceColor),
            LogLevel.Warning => (ConsoleLogger.WarnPrefix, ConsoleLogger.WarnColor),
            LogLevel.Fatal => (ConsoleLogger.FatalPrefix, ConsoleLogger.FatalColor),
            _ => (new(' ', ConsoleLogger.InfoPrefix.Length), Terminal.Settings.DefaultColor)
        };

    private static string CreateOutputMessage(string sourceName, string loggerMessage, string prefix, DateTime date)
    {
        var friendlySourceName = string.IsNullOrWhiteSpace(sourceName)
            ? string.Empty
            : sourceName.SliceLength(sourceName.LastIndexOf('.') + 1, sourceName.Length);

        var outputMessage = string.IsNullOrWhiteSpace(sourceName)
            ? loggerMessage
            : $"[{friendlySourceName}] {loggerMessage}";

        return string.IsNullOrWhiteSpace(LoggingTimeFormat)
            ? $" {prefix} >> {outputMessage}{Environment.NewLine}"
            : $" {date.ToLocalTime().ToString(LoggingTimeFormat)} {prefix} >> {outputMessage}{Environment.NewLine}";
    }
}
