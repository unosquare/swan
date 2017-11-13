namespace Unosquare.Swan
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// The event arguments for when connections are accepted
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionAcceptedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionAcceptedEventArgs" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <exception cref="ArgumentNullException">client</exception>
        public ConnectionAcceptedEventArgs(TcpClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public TcpClient Client { get; }
    }

    /// <summary>
    /// Occurs before a connection is accepted. Set the Cancel property to true to prevent the connection from being accepted.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.ConnectionAcceptedEventArgs" />
    public class ConnectionAcceptingEventArgs : ConnectionAcceptedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionAcceptingEventArgs"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public ConnectionAcceptingEventArgs(TcpClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Setting Cancel to true rejects the new TcpClient
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Event arguments for when a server listener is started
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionListenerStartedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListenerStartedEventArgs" /> class.
        /// </summary>
        /// <param name="listenerEndPoint">The listener end point.</param>
        /// <exception cref="ArgumentNullException">listenerEndPoint</exception>
        public ConnectionListenerStartedEventArgs(IPEndPoint listenerEndPoint)
        {
            EndPoint = listenerEndPoint ?? throw new ArgumentNullException(nameof(listenerEndPoint));
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public IPEndPoint EndPoint { get; }
    }

    /// <summary>
    /// Event arguments for when a server listener fails to start
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionListenerFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListenerFailedEventArgs" /> class.
        /// </summary>
        /// <param name="listenerEndPoint">The listener end point.</param>
        /// <param name="ex">The ex.</param>
        /// <exception cref="ArgumentNullException">
        /// listenerEndPoint
        /// or
        /// ex
        /// </exception>
        public ConnectionListenerFailedEventArgs(IPEndPoint listenerEndPoint, Exception ex)
        {
            EndPoint = listenerEndPoint ?? throw new ArgumentNullException(nameof(listenerEndPoint));
            Error = ex ?? throw new ArgumentNullException(nameof(ex));
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public Exception Error { get; }
    }

    /// <summary>
    /// Event arguments for when a server listener stopped
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionListenerStoppedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListenerStoppedEventArgs" /> class.
        /// </summary>
        /// <param name="listenerEndPoint">The listener end point.</param>
        /// <param name="ex">The ex.</param>
        /// <exception cref="ArgumentNullException">
        /// listenerEndPoint
        /// or
        /// ex
        /// </exception>
        public ConnectionListenerStoppedEventArgs(IPEndPoint listenerEndPoint, Exception ex = null)
        {
            EndPoint = listenerEndPoint ?? throw new ArgumentNullException(nameof(listenerEndPoint));
            Error = ex;
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public Exception Error { get; }
    }
}