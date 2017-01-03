namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Event arguments representing the message that is logged
    /// on to the terminal. Use the properties to forward the data to
    /// your logger of choice.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class LogMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageReceivedEventArgs" /> class.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="utcDate">The UTC date.</param>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        /// <param name="properties">The properties.</param>
        public LogMessageReceivedEventArgs(ulong sequence, LogMessageType messageType, DateTime utcDate, string source, 
            string message, Exception ex, string callerMemberName, string callerFilePath, int callerLineNumber, IDictionary<string, object> properties)
        {
            Sequence = sequence;
            MessageType = messageType;
            UtcDate = utcDate;
            Source = source;
            Message = message;
            Exception = ex;
            CallerMemberName = callerMemberName;
            CallerFilePath = callerFilePath;
            CallerLineNumber = callerLineNumber;
            Properties = properties;
        }

        /// <summary>
        /// Gets logging message sequence.
        /// </summary>
        public ulong Sequence { get; }

        /// <summary>
        /// Gets the type of the message.
        /// It can be a combination as the enumeration is a set of bitwise flags
        /// </summary>
        public LogMessageType MessageType { get; }

        /// <summary>
        /// Gets the UTC date at which the event at which the message was logged.
        /// </summary>
        public DateTime UtcDate { get; }

        /// <summary>
        /// Gets the name of the source where the logging message
        /// came from. This can come empty if the logger did not set it.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the body of the message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the exception if one was logged.
        /// This is typically empty for messages not categorized
        /// as errors.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets the name of the caller member.
        /// </summary>
        public string CallerMemberName { get; }
        
        /// <summary>
        /// Gets the caller file path.
        /// </summary>
        public string CallerFilePath { get; }
        
        /// <summary>
        /// Gets the caller line number.
        /// </summary>
        public int CallerLineNumber { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public IDictionary<string, object> Properties { get; }
    }

    /// <summary>
    /// An event handler representing the logging messages sent to the terminal
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="LogMessageReceivedEventArgs"/> instance containing the event data.</param>
    public delegate void LogMessageReceivedEventHandler(object sender, LogMessageReceivedEventArgs e);

    /// <summary>
    /// Event arguments representing a message logged and about to be
    /// displayed on the terminal (console). Set the CancelOutput property in the
    /// event handler to prevent the terminal from displaying the message.
    /// </summary>
    /// <seealso cref="LogMessageReceivedEventArgs" />
    public class LogMessageDisplayingEventArgs : LogMessageReceivedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageDisplayingEventArgs"/> class.
        /// </summary>
        /// <param name="data">The <see cref="LogMessageReceivedEventArgs"/> instance containing the event data.</param>
        public LogMessageDisplayingEventArgs(LogMessageReceivedEventArgs data)
            : base(data.Sequence, data.MessageType, data.UtcDate, data.Source, data.Message, data.Exception, 
                  data.CallerMemberName, data.CallerFilePath, data.CallerLineNumber, data.Properties)
        {
            CancelOutput = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the displaying of the
        /// logging message should be canceled.
        /// </summary>
        public bool CancelOutput { get; set; }
    }

    /// <summary>
    /// An event handler representing the logging messages about to be displayed on the terminal
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="LogMessageDisplayingEventArgs"/> instance containing the event data.</param>
    public delegate void LogMessageDisplayingEventHandler(object sender, LogMessageDisplayingEventArgs e);
}
