#if !UWP
namespace Unosquare.Swan.Networking
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a network connection either on the server or on the client. It wraps a TcpClient
    /// and its corresponding network streams. It is capable of working in 2 modes. Typically on the server side
    /// you will need to enable continuous reading and events. On the client side you may want to disable continuous reading
    /// and use the Read methods available. In continuous reading mode Read methods are not available and will throw
    /// an invalid operation exceptions if they are used.
    /// Continuous Reading Mode: Subscribe to data reception events, it runs a background thread, don't use Read methods
    /// Manual Reading Mode: Data reception events are NEVER fired. No background threads are used. Use Read methods to receive data
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class Connection : IDisposable
    {
        // New Line definitions for reading. This applies to both, events and read methods
        private readonly string _newLineSequence;

        private readonly byte[] _newLineSequenceBytes;
        private readonly char[] _newLineSequenceChars;
        private readonly string[] _newLineSequenceLineSplitter;
        private readonly byte[] _receiveBuffer;
        private readonly TimeSpan _continuousReadingInterval = TimeSpan.FromMilliseconds(5);
        private readonly Queue<string> _readLineBuffer = new Queue<string>();
        private readonly ManualResetEvent _writeDone = new ManualResetEvent(true);

        // Disconnect and Dispose
        private bool _hasDisposed;

        private int _disconnectCalls;

        // Continuous Reading
        private Thread _continuousReadingThread;

        private int _receiveBufferPointer;

        // Reading and writing
        private Task<int> _readTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="newLineSequence">The new line sequence used for read and write operations.</param>
        /// <param name="disableContinuousReading">if set to <c>true</c> [disable continuous reading].</param>
        /// <param name="blockSize">Size of the block. -- set to 0 or less to disable</param>
        public Connection(
            TcpClient client,
            Encoding textEncoding,
            string newLineSequence,
            bool disableContinuousReading,
            int blockSize)
        {
            // Setup basic properties
            Id = Guid.NewGuid();
            TextEncoding = textEncoding;

            // Setup new line sequence
            if (string.IsNullOrEmpty(newLineSequence))
                throw new ArgumentException("Argument cannot be null", nameof(newLineSequence));

            _newLineSequence = newLineSequence;
            _newLineSequenceBytes = TextEncoding.GetBytes(_newLineSequence);
            _newLineSequenceChars = _newLineSequence.ToCharArray();
            _newLineSequenceLineSplitter = new[] {_newLineSequence};

            // Setup Connection timers
            ConnectionStartTimeUtc = DateTime.UtcNow;
            DataReceivedLastTimeUtc = ConnectionStartTimeUtc;
            DataSentLastTimeUtc = ConnectionStartTimeUtc;

            // Setup connection properties
            RemoteClient = client;
            LocalEndPoint = client.Client.LocalEndPoint as IPEndPoint;
            NetworkStream = RemoteClient.GetStream();
            RemoteEndPoint = RemoteClient.Client.RemoteEndPoint as IPEndPoint;

            // Setup buffers
            _receiveBuffer = new byte[RemoteClient.ReceiveBufferSize * 2];
            ProtocolBlockSize = blockSize;
            _receiveBufferPointer = 0;

            // Setup continuous reading mode if enabled
            if (disableContinuousReading) return;

#if NETSTANDARD1_3
            ThreadPool.QueueUserWorkItem(PerformContinuousReading, this);
#else
            ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out _);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

            var activeThreadPoolTreads = maxWorkerThreads - availableWorkerThreads;

            if (activeThreadPoolTreads < Environment.ProcessorCount / 4)
            {
                ThreadPool.QueueUserWorkItem(PerformContinuousReading, this);
            }
            else
            {
                new Thread(PerformContinuousReading) {IsBackground = true}.Start();
            }
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class in continuous reading mode.
        /// It uses UTF8 encoding, CRLF as a new line sequence and disables a protocol block size
        /// </summary>
        /// <param name="client">The client.</param>
        public Connection(TcpClient client)
            : this(client, Encoding.UTF8, "\r\n", false, 0)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class in continuous reading mode.
        /// It uses UTF8 encoding, disables line sequences, and uses a protocol block size instead
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="blockSize">Size of the block.</param>
        public Connection(TcpClient client, int blockSize)
            : this(client, Encoding.UTF8, new string('\n', blockSize + 1), false, blockSize)
        {
            // placeholder
        }

        #region Events

        /// <summary>
        /// Occurs when the receive buffer has encounters a new line sequence, the buffer is flushed or the buffer is full.
        /// </summary>
        public event EventHandler<ConnectionDataReceivedEventArgs> DataReceived = (s, e) => { };

        /// <summary>
        /// Occurs when an error occurs while upgrading, sending, or receiving data in this client
        /// </summary>
        public event EventHandler<ConnectionFailureEventArgs> ConnectionFailure = (s, e) => { };

        /// <summary>
        /// Occurs when a client is disconnected
        /// </summary>
        public event EventHandler ClientDisconnected = (s, e) => { };

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique identifier of this connection.
        /// This field is filled out upon instantiation of this class
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; }

        /// <summary>
        /// Gets the active stream. Returns an SSL stream if the connection is secure, otherwise returns
        /// the underlying NetworkStream
        /// </summary>
        /// <value>
        /// The active stream.
        /// </value>
        public Stream ActiveStream => SecureStream ?? NetworkStream as Stream;

        /// <summary>
        /// Gets a value indicating whether the current connection stream is an SSL stream.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active stream secure; otherwise, <c>false</c>.
        /// </value>
        public bool IsActiveStreamSecure => SecureStream != null;

        /// <summary>
        /// Gets the text encoding for send and receive operations.
        /// </summary>
        /// <value>
        /// The text encoding.
        /// </value>
        public Encoding TextEncoding { get; }

        /// <summary>
        /// Gets the remote end point of this TCP connection.
        /// </summary>
        /// <value>
        /// The remote end point.
        /// </value>
        public IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the local end point of this TCP connection.
        /// </summary>
        /// <value>
        /// The local end point.
        /// </value>
        public IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// Gets the remote client of this TCP connection.
        /// </summary>
        /// <value>
        /// The remote client.
        /// </value>
        public TcpClient RemoteClient { get; private set; }

        /// <summary>
        /// When in continuous reading mode, and if set to greater than 0,
        /// a Data reception event will be fired whenever the amount of bytes
        /// determined by this property has been received. Useful for fixed-length message protocols.
        /// </summary>
        /// <value>
        /// The size of the protocol block.
        /// </value>
        public int ProtocolBlockSize { get; }

        /// <summary>
        /// Gets a value indicating whether this connection is in continuous reading mode.
        /// Remark: Whenever a disconnect event occurs, the background thread is terminated
        /// and this property will return false whenever the reading thread is not active.
        /// Therefore, even if continuous reading was not disabled in the constructor, this property
        /// might return false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is continuous reading enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsContinuousReadingEnabled => _continuousReadingThread != null;

        /// <summary>
        /// Gets the start time at which the connection was started in UTC.
        /// </summary>
        /// <value>
        /// The connection start time UTC.
        /// </value>
        public DateTime ConnectionStartTimeUtc { get; }

        /// <summary>
        /// Gets the start time at which the connection was started in local time.
        /// </summary>
        /// <value>
        /// The connection start time.
        /// </value>
        public DateTime ConnectionStartTime => ConnectionStartTimeUtc.ToLocalTime();

        /// <summary>
        /// Gets the duration of the connection.
        /// </summary>
        /// <value>
        /// The duration of the connection.
        /// </value>
        public TimeSpan ConnectionDuration => DateTime.UtcNow.Subtract(ConnectionStartTimeUtc);

        /// <summary>
        /// Gets the last time data was received at in UTC.
        /// </summary>
        /// <value>
        /// The data received last time UTC.
        /// </value>
        public DateTime DataReceivedLastTimeUtc { get; private set; }

        /// <summary>
        /// Gets how long has elapsed since data was last received.
        /// </summary>
        public TimeSpan DataReceivedIdleDuration => DateTime.UtcNow.Subtract(DataReceivedLastTimeUtc);

        /// <summary>
        /// Gets the last time at which data was sent in UTC.
        /// </summary>
        /// <value>
        /// The data sent last time UTC.
        /// </value>
        public DateTime DataSentLastTimeUtc { get; private set; }

        /// <summary>
        /// Gets how long has elapsed since data was last sent
        /// </summary>
        /// <value>
        /// The duration of the data sent idle.
        /// </value>
        public TimeSpan DataSentIdleDuration => DateTime.UtcNow.Subtract(DataSentLastTimeUtc);

        /// <summary>
        /// Gets a value indicating whether this connection is connected.
        /// Remarks: This property polls the socket internally and checks if it is available to read data from it.
        /// If disconnect has been called, then this property will return false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                if (_disconnectCalls > 0)
                    return false;

                try
                {
                    var socket = RemoteClient.Client;
                    var pollResult = !((socket.Poll(1000, SelectMode.SelectRead)
                                        && (NetworkStream.DataAvailable == false)) || !socket.Connected);

                    if (pollResult == false)
                        Disconnect();

                    return pollResult;
                }
                catch
                {
                    Disconnect();
                    return false;
                }
            }
        }

        private NetworkStream NetworkStream { get; set; }

        private SslStream SecureStream { get; set; }

        #endregion

        #region Read Methods

        /// <summary>
        /// Reads data from the remote client asynchronously and with the given timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters</returns>
        /// <exception cref="InvalidOperationException">Read methods have been disabled because continuous reading is enabled.</exception>
        /// <exception cref="TimeoutException">Reading data from {ActiveStream} timed out in {timeout.TotalMilliseconds} m</exception>
        public async Task<byte[]> ReadDataAsync(TimeSpan timeout, CancellationToken ct = default(CancellationToken))
        {
            if (IsContinuousReadingEnabled)
            {
                throw new InvalidOperationException(
                    "Read methods have been disabled because continuous reading is enabled.");
            }

            if (RemoteClient == null)
            {
                throw new InvalidOperationException("An open connection is required");
            }

            var receiveBuffer = new byte[RemoteClient.ReceiveBufferSize * 2];
            var receiveBuilder = new List<byte>(receiveBuffer.Length);

            try
            {
                var startTime = DateTime.UtcNow;

                while (receiveBuilder.Count <= 0)
                {
                    if (DateTime.UtcNow.Subtract(startTime) >= timeout)
                    {
                        throw new TimeoutException(
                            $"Reading data from {ActiveStream} timed out in {timeout.TotalMilliseconds} ms");
                    }

                    if (_readTask == null)
                        _readTask = ActiveStream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length, ct);

                    if (_readTask.Wait(_continuousReadingInterval))
                    {
                        var bytesReceivedCount = _readTask.Result;
                        if (bytesReceivedCount > 0)
                        {
                            DataReceivedLastTimeUtc = DateTime.UtcNow;
                            var buffer = new byte[bytesReceivedCount];
                            Array.Copy(receiveBuffer, 0, buffer, 0, bytesReceivedCount);
                            receiveBuilder.AddRange(buffer);
                        }

                        _readTask = null;
                    }
                    else
                    {
                        await Task.Delay(_continuousReadingInterval, ct);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Error(typeof(Connection).FullName, "Error while reading network stream data asynchronously.");
                throw;
            }

            return receiveBuilder.ToArray();
        }

        /// <summary>
        /// Reads data asynchronously from the remote stream with a 5000 millisecond timeout.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A byte array containing the results the specified sequence of bytes</returns>
        public Task<byte[]> ReadDataAsync(CancellationToken ct = default(CancellationToken)) 
            => ReadDataAsync(TimeSpan.FromSeconds(5), ct);

        /// <summary>
        /// Asynchronously reads data as text with the given timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A <see cref="System.String" /> that contains the results of decoding the specified sequence of bytes</returns>
        public async Task<string> ReadTextAsync(TimeSpan timeout, CancellationToken ct = default(CancellationToken))
        {
            var buffer = await ReadDataAsync(timeout, ct);
            return buffer == null ? null : TextEncoding.GetString(buffer);
        }

        /// <summary>
        /// Asynchronously reads data as text with a 5000 millisecond timeout.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>When this method completes successfully, it returns the contents of the file as a text string</returns>
        public Task<string> ReadTextAsync(CancellationToken ct = default(CancellationToken))
            => ReadTextAsync(TimeSpan.FromSeconds(5), ct);

        /// <summary>
        /// Performs the same task as this method's overload but it defaults to a read timeout of 30 seconds.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult parameter 
        /// contains the next line from the stream, or is null if all the characters have been read
        /// </returns>
        public Task<string> ReadLineAsync(CancellationToken ct = default(CancellationToken))
            => ReadLineAsync(TimeSpan.FromSeconds(30), ct);

        /// <summary>
        /// Reads the next available line of text in queue. Return null when no text is read.
        /// This method differs from the rest of the read methods because it keeps an internal
        /// queue of lines that are read from the stream and only returns the one line next in the queue.
        /// It is only recommended to use this method when you are working with text-based protocols
        /// and the rest of the read methods are not called.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task with a string line from the queue</returns>
        /// <exception cref="InvalidOperationException">Read methods have been disabled because continuous reading is enabled.</exception>
        public async Task<string> ReadLineAsync(TimeSpan timeout, CancellationToken ct = default(CancellationToken))
        {
            if (IsContinuousReadingEnabled)
            {
                throw new InvalidOperationException(
                    "Read methods have been disabled because continuous reading is enabled.");
            }

            if (_readLineBuffer.Count > 0)
                return _readLineBuffer.Dequeue();

            var builder = new StringBuilder();

            while (true)
            {
                var text = await ReadTextAsync(timeout, ct);
                if (text.Length == 0)
                    break;

                builder.Append(text);

                if (text.EndsWith(_newLineSequence) == false) continue;

                var lines = builder.ToString().TrimEnd(_newLineSequenceChars)
                    .Split(_newLineSequenceLineSplitter, StringSplitOptions.None);
                foreach (var item in lines)
                    _readLineBuffer.Enqueue(item);

                break;
            }

            return _readLineBuffer.Count > 0 ? _readLineBuffer.Dequeue() : null;
        }

        #endregion

        #region Write Methods

        /// <summary>
        /// Writes data asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="forceFlush">if set to <c>true</c> [force flush].</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous write operation</returns>
        public async Task WriteDataAsync(byte[] buffer, bool forceFlush, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                _writeDone.WaitOne();
                _writeDone.Reset();
                await ActiveStream.WriteAsync(buffer, 0, buffer.Length, ct);
                if (forceFlush)
                    await ActiveStream.FlushAsync(ct);

                DataSentLastTimeUtc = DateTime.UtcNow;
            }
            finally
            {
                _writeDone.Set();
            }
        }

        /// <summary>
        /// Writes text asynchronously.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous write operation</returns>
        public Task WriteTextAsync(string text, CancellationToken ct = default(CancellationToken))
            => WriteTextAsync(text, TextEncoding, ct);

        /// <summary>
        /// Writes text asynchronously.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous write operation</returns>
        public Task WriteTextAsync(string text, Encoding encoding, CancellationToken ct = default(CancellationToken))
            => WriteDataAsync(encoding.GetBytes(text), true, ct);

        /// <summary>
        /// Writes a line of text asynchronously.
        /// The new line sequence is added automatically at the end of the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous write operation</returns>
        public async Task WriteLineAsync(string line, Encoding encoding, CancellationToken ct = default(CancellationToken))
        {
            var buffer = encoding.GetBytes($"{line}{_newLineSequence}");
            await WriteDataAsync(buffer, true, ct);
        }

        /// <summary>
        /// Writes a line of text asynchronously.
        /// The new line sequence is added automatically at the end of the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous write operation</returns>
        public Task WriteLineAsync(string line, CancellationToken ct = default(CancellationToken))
            => WriteLineAsync(line, TextEncoding, ct);

        #endregion

        #region Socket Methods

        /// <summary>
        /// Upgrades the active stream to an SSL stream if this connection object is hosted in the server.
        /// </summary>
        /// <param name="serverCertificate">The server certificate.</param>
        /// <returns><c>true</c> if the object is hosted in the server; otherwise, <c>false</c>.</returns>
        public async Task<bool> UpgradeToSecureAsServerAsync(X509Certificate2 serverCertificate)
        {
            if (IsActiveStreamSecure)
                return true;

            _writeDone.WaitOne();

            SslStream secureStream = null;

            try
            {
                secureStream = new SslStream(NetworkStream, true);
                await secureStream.AuthenticateAsServerAsync(serverCertificate);
                SecureStream = secureStream;
                return true;
            }
            catch (Exception ex)
            {
                ConnectionFailure(this, new ConnectionFailureEventArgs(ex));
                secureStream?.Dispose();

                return false;
            }
        }

        /// <summary>
        /// Upgrades the active stream to an SSL stream if this connection object is hosted in the client.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>A tasks with <c>true</c> if the upgrade to SSL was successful; otherwise, <c>false</c></returns>
        public async Task<bool> UpgradeToSecureAsClientAsync(
            string hostname,
            RemoteCertificateValidationCallback callback)
        {
            if (IsActiveStreamSecure)
                return true;

            var secureStream = new SslStream(NetworkStream, true, callback);

            try
            {
                await secureStream.AuthenticateAsClientAsync(hostname);
                SecureStream = secureStream;
            }
            catch (Exception ex)
            {
                secureStream.Dispose();
                ConnectionFailure(this, new ConnectionFailureEventArgs(ex));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Upgrades the active stream to an SSL stream if this connection object is hosted in the client.
        /// Remarks: DO NOT use this method in production. It accepts ALL server certificates without even checking them!
        /// </summary>
        /// <returns>A tasks with <c>true</c> if the upgrade to SSL was successful; otherwise, <c>false</c></returns>
        public Task<bool> UpgradeToSecureAsClientAsync()
            => UpgradeToSecureAsClientAsync(Network.HostName.ToLowerInvariant(), (a, b, c, d) => true);

        /// <summary>
        /// Disconnects this connection.
        /// </summary>
        public void Disconnect()
        {
            if (_disconnectCalls > 0)
                return;

            _disconnectCalls++;
            _writeDone.WaitOne();

            try
            {
                ClientDisconnected(this, EventArgs.Empty);
            }
            catch
            {
                // ignore
            }

            try
            {
#if !NET452
                RemoteClient.Dispose();
                SecureStream?.Dispose();
                NetworkStream?.Dispose();
#else
                RemoteClient.Close();
                SecureStream?.Close();
                NetworkStream?.Close();
#endif
            }
            catch
            {
                // ignored
            }
            finally
            {
                NetworkStream = null;
                SecureStream = null;
                RemoteClient = null;
                _continuousReadingThread = null;
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_hasDisposed)
                return;

            // Release managed resources
            Disconnect();
            _continuousReadingThread = null;
            _writeDone.Dispose();

            _hasDisposed = true;
        }

        #endregion

        #region Continuous Read Methods

        /// <summary>
        /// Raises the receive buffer events.
        /// </summary>
        /// <param name="receivedData">The received data.</param>
        /// <exception cref="Exception">Split function failed! This is terribly wrong!</exception>
        private void RaiseReceiveBufferEvents(byte[] receivedData)
        {
            var moreAvailable = RemoteClient.Available > 0;

            for (var i = 0; i < receivedData.Length; i++)
            {
                ProcessReceivedBlock(receivedData, i, moreAvailable);
            }

            // Check if we are left with some more stuff to handle
            if (_receiveBufferPointer <= 0)
                return;

            // Extract the segments split by newline terminated bytes
            var sequences = _receiveBuffer.Skip(0).Take(_receiveBufferPointer).ToArray()
                .Split(0, _newLineSequenceBytes);

            // Something really wrong happened
            if (sequences.Count == 0)
                throw new Exception("Split function failed! This is terribly wrong!");

            // We only have one sequence and it is not newline-terminated
            // we don't have to do anything.
            if (sequences.Count == 1 && sequences[0].EndsWith(_newLineSequenceBytes) == false)
                return;

            // Process the events for each sequence
            for (var i = 0; i < sequences.Count; i++)
            {
                var sequenceBytes = sequences[i];
                var isNewLineTerminated = sequences[i].EndsWith(_newLineSequenceBytes);
                var isLast = i == sequences.Count - 1;

                // Log.Trace($"    ~ {i:00} ~ TERM: {isNewLineTerminated,-6} LAST: {isLast,-6} LEN: {sequenceBytes.Length,-4} {TextEncoding.GetString(sequenceBytes).TrimEnd(NewLineSequenceChars)}");

                if (isNewLineTerminated)
                {
                    var eventArgs = new ConnectionDataReceivedEventArgs(
                        sequenceBytes,
                        ConnectionDataReceivedTrigger.NewLineSequenceEncountered,
                        isLast == false);
                    DataReceived(this, eventArgs);
                }

                // Depending on the last segment determine what to do with the receive buffer
                if (!isLast) continue;

                if (isNewLineTerminated)
                {
                    // Simply reset the buffer pointer if the last segment was also terminated
                    _receiveBufferPointer = 0;
                }
                else
                {
                    // If we have not received the termination sequence, then just shift the receive buffer to the left
                    // and adjust the pointer
                    Array.Copy(sequenceBytes, _receiveBuffer, sequenceBytes.Length);
                    _receiveBufferPointer = sequenceBytes.Length;
                }
            }
        }

        private void ProcessReceivedBlock(byte[] receivedData, int i, bool moreAvailable)
        {
            _receiveBuffer[_receiveBufferPointer] = receivedData[i];
            _receiveBufferPointer++;

            // Block size reached
            if (ProtocolBlockSize > 0 && _receiveBufferPointer >= ProtocolBlockSize)
            {
                var eventBuffer = new byte[_receiveBuffer.Length];
                Array.Copy(_receiveBuffer, eventBuffer, eventBuffer.Length);

                DataReceived(this,
                    new ConnectionDataReceivedEventArgs(
                        eventBuffer,
                        ConnectionDataReceivedTrigger.BlockSizeReached,
                        moreAvailable));
                _receiveBufferPointer = 0;
                return;
            }

            // The receive buffer is full. Time to flush
            if (_receiveBufferPointer >= _receiveBuffer.Length)
            {
                var eventBuffer = new byte[_receiveBuffer.Length];
                Array.Copy(_receiveBuffer, eventBuffer, eventBuffer.Length);

                DataReceived(this,
                    new ConnectionDataReceivedEventArgs(
                        eventBuffer,
                        ConnectionDataReceivedTrigger.BufferFull,
                        moreAvailable));
                _receiveBufferPointer = 0;
            }
        }

        /// <summary>
        /// This is the body of the thread when performing continuous reading
        /// </summary>
        /// <param name="threadContext">The thread context.</param>
        private void PerformContinuousReading(object threadContext)
        {
            _continuousReadingThread = Thread.CurrentThread;

            // Check if the RemoteClient is still there
            if (RemoteClient == null) return;

            var receiveBuffer = new byte[RemoteClient.ReceiveBufferSize * 2];

            while (IsConnected && _disconnectCalls <= 0)
            {
                var doThreadSleep = false;

                try
                {
                    if (_readTask == null)
                        _readTask = ActiveStream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

                    if (_readTask.Wait(_continuousReadingInterval))
                    {
                        var bytesReceivedCount = _readTask.Result;
                        if (bytesReceivedCount > 0)
                        {
                            DataReceivedLastTimeUtc = DateTime.UtcNow;
                            var buffer = new byte[bytesReceivedCount];
                            Array.Copy(receiveBuffer, 0, buffer, 0, bytesReceivedCount);
                            RaiseReceiveBufferEvents(buffer);
                        }

                        _readTask = null;
                    }
                    else
                    {
                        doThreadSleep = _disconnectCalls <= 0;
                    }
                }
                catch (Exception ex)
                {
                    ex.Log(nameof(Connection), "Continuous Read operation errored");
                }
                finally
                {
                    if (doThreadSleep)
                        Thread.Sleep(_continuousReadingInterval);
                }
            }
        }

        #endregion
    }
}
#endif