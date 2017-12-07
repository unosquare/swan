namespace Unosquare.Swan
{
    using System;
    using System.Text;
    
    #region Connection

    /// <summary>
    /// The event arguments for connection failure events
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionFailureEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionFailureEventArgs"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public ConnectionFailureEventArgs(Exception ex)
        {
            Error = ex;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public Exception Error { get; }
    }

    /// <summary>
    /// Event arguments for when data is received.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDataReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="moreAvailable">if set to <c>true</c> [more available].</param>
        public ConnectionDataReceivedEventArgs(byte[] buffer, ConnectionDataReceivedTrigger trigger, bool moreAvailable)
        {
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            Trigger = trigger;
            HasMoreAvailable = moreAvailable;
        }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        public byte[] Buffer { get; }

        /// <summary>
        /// Gets the cause as to why this event was thrown
        /// </summary>
        /// <value>
        /// The trigger.
        /// </value>
        public ConnectionDataReceivedTrigger Trigger { get; }

        /// <summary>
        /// Gets a value indicating whether the receive buffer has more bytes available
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has more available; otherwise, <c>false</c>.
        /// </value>
        public bool HasMoreAvailable { get; }

        /// <summary>
        /// Gets the string from the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>A <see cref="System.String" /> that contains the results of decoding the specified sequence of bytes</returns>
        public static string GetStringFromBuffer(byte[] buffer, Encoding encoding)
            => encoding.GetString(buffer).TrimEnd('\r', '\n');

        /// <summary>
        /// Gets the string from buffer.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>A <see cref="System.String" /> that contains the results of decoding the specified sequence of bytes</returns>
        public string GetStringFromBuffer(Encoding encoding) 
            => GetStringFromBuffer(Buffer, encoding);
    }

    #endregion
}
