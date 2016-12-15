namespace Unosquare.Swan.Nlog
{
    using System;

    /// <summary>
    /// Basic model for application logging
    /// This model is useful for most scenarios.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets or sets the sequence identifier.
        /// </summary>
        /// <value>
        /// The sequence identifier.
        /// </value>
        public int SequenceID { get; set; }

        /// <summary>
        /// Gets or sets the entry date UTC.
        /// </summary>
        /// <value>
        /// The entry date UTC.
        /// </value>
        public DateTime EntryDateUtc { get; set; }

        /// <summary>
        /// Gets or sets thename of the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public string Logger { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the level. This is typically
        /// Debug, Trace, Info, Error, etc.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public string Level { get; set; }

        /// <summary>
        /// If the log entry is an exception, 
        /// this property hold the type of the 
        /// exception that was thrown.
        /// </summary>
        /// <value>
        /// The type of the exception.
        /// </value>
        public string ExceptionType { get; set; }

        /// <summary>
        /// In a web application it typically represents
        /// the identifies of a user's session
        /// </summary>
        public string SessionId { get; set; }
    }
}