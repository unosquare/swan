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
        /// Gets the client.
        /// </summary>
        public TcpClient Client { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionAcceptedEventArgs"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public ConnectionAcceptedEventArgs(TcpClient client)
        {
            Client = client;
        }
    }

    /// <summary>
    /// Occurs before a connection is accepted. Set the Cancel property to true to prevent the connection from being accepted.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.ConnectionAcceptedEventArgs" />
    public class ConnectionAcceptingEventArgs : ConnectionAcceptedEventArgs
    {
        /// <summary>
        /// Setting Cancel to true rejects the new TcpClient
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionAcceptingEventArgs"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public ConnectionAcceptingEventArgs(TcpClient client)
            : base(client)
        {

        }
    }

    /// <summary>
    /// Event arguments for when a server listener is started
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionListenerStartedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the end point.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListenerStartedEventArgs"/> class.
        /// </summary>
        /// <param name="listenerEndPoint">The listener end point.</param>
        public ConnectionListenerStartedEventArgs(IPEndPoint listenerEndPoint)
        {
            EndPoint = listenerEndPoint;
        }
    }

    /// <summary>
    /// Event arguments for when a server listener fails to start
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionListenerFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the end point.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListenerFailedEventArgs"/> class.
        /// </summary>
        /// <param name="listenerEndPoint">The listener end point.</param>
        /// <param name="ex">The ex.</param>
        public ConnectionListenerFailedEventArgs(IPEndPoint listenerEndPoint, Exception ex)
        {
            EndPoint = listenerEndPoint;
            Error = ex;
        }
    }

    /// <summary>
    /// Event arguments for when a server listener stopped
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ConnectionListenerStoppedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the end point.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListenerFailedEventArgs"/> class.
        /// </summary>
        /// <param name="listenerEndPoint">The listener end point.</param>
        /// <param name="ex">The ex.</param>
        public ConnectionListenerStoppedEventArgs(IPEndPoint listenerEndPoint, Exception ex = null)
        {
            EndPoint = listenerEndPoint;
            Error = ex;
        }
    }
}
