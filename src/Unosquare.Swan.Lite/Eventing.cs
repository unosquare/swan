namespace Unosquare.Swan
{
    using System;

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
        /// <param name="extendedData">The extended data.</param>
        /// <param name="callerMemberName">Name of the caller member.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        public LogMessageReceivedEventArgs(
            ulong sequence,
            LogMessageType messageType,
            DateTime utcDate,
            string source,
            string message,
            object extendedData,
            string callerMemberName,
            string callerFilePath,
            int callerLineNumber)
        {
            Sequence = sequence;
            MessageType = messageType;
            UtcDate = utcDate;
            Source = source;
            Message = message;
            CallerMemberName = callerMemberName;
            CallerFilePath = callerFilePath;
            CallerLineNumber = callerLineNumber;
            ExtendedData = extendedData;
        }

        /// <summary>
        /// Gets logging message sequence.
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        public ulong Sequence { get; }

        /// <summary>
        /// Gets the type of the message.
        /// It can be a combination as the enumeration is a set of bitwise flags
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public LogMessageType MessageType { get; }

        /// <summary>
        /// Gets the UTC date at which the event at which the message was logged.
        /// </summary>
        /// <value>
        /// The UTC date.
        /// </value>
        public DateTime UtcDate { get; }

        /// <summary>
        /// Gets the name of the source where the logging message
        /// came from. This can come empty if the logger did not set it.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; }

        /// <summary>
        /// Gets the body of the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; }

        /// <summary>
        /// Gets the name of the caller member.
        /// </summary>
        /// <value>
        /// The name of the caller member.
        /// </value>
        public string CallerMemberName { get; }

        /// <summary>
        /// Gets the caller file path.
        /// </summary>
        /// <value>
        /// The caller file path.
        /// </value>
        public string CallerFilePath { get; }

        /// <summary>
        /// Gets the caller line number.
        /// </summary>
        /// <value>
        /// The caller line number.
        /// </value>
        public int CallerLineNumber { get; }

        /// <summary>
        /// Gets an object representing extended data.
        /// It could be an exception or anything else
        /// </summary>
        /// <value>
        /// The extended data.
        /// </value>
        public object ExtendedData { get; }

        /// <summary>
        /// Gets the Extended Data properties cast as an Exception (if possible)
        /// Otherwise, it return null
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception => ExtendedData as Exception;
    }

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
            : base(
                data.Sequence,
                data.MessageType,
                data.UtcDate,
                data.Source,
                data.Message,
                data.ExtendedData,
                data.CallerMemberName,
                data.CallerFilePath,
                data.CallerLineNumber)
        {
            CancelOutput = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the displaying of the
        /// logging message should be canceled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cancel output]; otherwise, <c>false</c>.
        /// </value>
        public bool CancelOutput { get; set; }
    }
}