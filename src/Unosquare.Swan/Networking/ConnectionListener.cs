namespace Unosquare.Swan.Networking
{
    using Swan;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// TCP Listener manager with built-in events and asynchronous functionality.
    /// This networking component is typically used when writing server software
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class ConnectionListener : IDisposable
    {
        #region Private Declarations

        private readonly object _stateLock = new object();
        private TcpListener _listenerSocket;
        private bool _cancellationPending;
        private CancellationTokenSource _cancelListening;
        private Task _backgroundWorkerTask;
        private bool _hasDisposed;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a new connection requests a socket from the listener.
        /// Set Cancel = true to prevent the TCP client from being accepted.
        /// </summary>
        public event EventHandler<ConnectionAcceptingEventArgs> OnConnectionAccepting = (s, e) => { };

        /// <summary>
        /// Occurs when a new connection is accepted.
        /// </summary>
        public event EventHandler<ConnectionAcceptedEventArgs> OnConnectionAccepted = (s, e) => { };

        /// <summary>
        /// Occurs when a connection fails to get accepted
        /// </summary>
        public event EventHandler<ConnectionFailureEventArgs> OnConnectionFailure = (s, e) => { };

        /// <summary>
        /// Occurs when the listener stops.
        /// </summary>
        public event EventHandler<ConnectionListenerStoppedEventArgs> OnListenerStopped = (s, e) => { };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListener"/> class.
        /// </summary>
        /// <param name="listenEndPoint">The listen end point.</param>
        public ConnectionListener(IPEndPoint listenEndPoint)
        {
            Id = Guid.NewGuid();
            LocalEndPoint = listenEndPoint ?? throw new ArgumentNullException(nameof(listenEndPoint));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListener"/> class.
        /// It uses the loopback address for listening
        /// </summary>
        /// <param name="listenPort">The listen port.</param>
        public ConnectionListener(int listenPort)
            : this(new IPEndPoint(IPAddress.Loopback, listenPort))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionListener"/> class.
        /// </summary>
        /// <param name="listenAddress">The listen address.</param>
        /// <param name="listenPort">The listen port.</param>
        public ConnectionListener(IPAddress listenAddress, int listenPort)
            : this(new IPEndPoint(listenAddress, listenPort))
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConnectionListener"/> class.
        /// </summary>
        ~ConnectionListener()
        {
            Dispose(false);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the local end point on which we are listening.
        /// </summary>
        /// <value>
        /// The local end point.
        /// </value>
        public IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets a value indicating whether this listener is active
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is listening; otherwise, <c>false</c>.
        /// </value>
        public bool IsListening => _backgroundWorkerTask != null;

        /// <summary>
        /// Gets a unique identifier that gets automatically assigned upon instantiation of this class.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Id { get; }

        #endregion

        #region Start and Stop

        /// <summary>
        /// Starts the listener in an asynchronous, non-blocking fashion.
        /// Subscribe to the events of this class to gain access to connected client sockets.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Cancellation has already been requested. This listener is not reusable.</exception>
        public void Start()
        {
            lock (_stateLock)
            {
                if (_backgroundWorkerTask != null)
                {
                    return;
                }

                if (_cancellationPending)
                {
                    throw new InvalidOperationException(
                        "Cancellation has already been requested. This listener is not reusable.");
                }

                _backgroundWorkerTask = DoWorkAsync();
            }
        }

        /// <summary>
        /// Continuously checks for client connections until the Close method has been called.
        /// </summary>
        /// <returns>A task that represents the asynchronous connection operation</returns>
        private async Task DoWorkAsync()
        {
            _cancellationPending = false;
            _listenerSocket = new TcpListener(LocalEndPoint);
            _listenerSocket.Start();
            _cancelListening = new CancellationTokenSource();

            try
            {
                while (_cancellationPending == false)
                {
                    try
                    {
                        var client = await Task.Run(() => _listenerSocket.AcceptTcpClientAsync(), _cancelListening.Token);
                        var acceptingArgs = new ConnectionAcceptingEventArgs(client);
                        OnConnectionAccepting(this, acceptingArgs);

                        if (acceptingArgs.Cancel)
                        {
#if !NET452
                            client.Dispose();
#else
                            client.Close();
#endif
                            continue;
                        }

                        OnConnectionAccepted(this, new ConnectionAcceptedEventArgs(client));
                    }
                    catch (Exception ex)
                    {
                        OnConnectionFailure(this, new ConnectionFailureEventArgs(ex));
                    }
                }

                OnListenerStopped(this, new ConnectionListenerStoppedEventArgs(LocalEndPoint));
            }
            catch (ObjectDisposedException)
            {
                OnListenerStopped(this, new ConnectionListenerStoppedEventArgs(LocalEndPoint));
            }
            catch (Exception ex)
            {
                OnListenerStopped(this,
                    new ConnectionListenerStoppedEventArgs(LocalEndPoint, _cancellationPending ? null : ex));
            }
            finally
            {
                _backgroundWorkerTask = null;
                _cancellationPending = false;
            }
        }

        /// <summary>
        /// Stops the listener from receiving new connections.
        /// This does not prevent the listener from 
        /// </summary>
        public void Stop()
        {
            lock (_stateLock)
            {
                _cancellationPending = true;
                _listenerSocket?.Stop();
                _cancelListening?.Cancel();
                _backgroundWorkerTask?.Wait();
                _backgroundWorkerTask = null;
                _cancellationPending = false;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => LocalEndPoint.ToString();

        #endregion

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_hasDisposed)
                return;

            if (disposing)
            {
                // Release managed resources
                Stop();
            }

            _hasDisposed = true;
        }

        #endregion
    }
}
