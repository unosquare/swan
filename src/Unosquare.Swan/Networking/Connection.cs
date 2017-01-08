namespace Unosquare.Swan.Networking
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    /// Continuous Reading Mode: Subscribe to data reception events, it runs a background thread, don't use Red methods
    /// Manual Reading Mode: Data reception events are NEVER fired. No background threads are used. Use Read methods to receive data
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class Connection : IDisposable
    {
        #region Private Members

        // New Line definitions for reading. This applies to both, events and read methods
        private readonly string NewLineSequence;
        private readonly byte[] NewLineSequenceBytes;
        private readonly char[] NewLineSequenceChars;
        private readonly string[] NewLineSequenceLineSplitter;

        // Disconnect and Dispose
        private bool HasDisposed;
        private int DisconnectCalls;

        // Continuous Reading
        private Thread ContinuousReadingThread = null;
        private readonly TimeSpan ContinuousReadingInterval = TimeSpan.FromMilliseconds(5);
        private readonly byte[] ReceiveBuffer;
        private int ReceiveBufferPointer;

        // Reading and writing
        private Task<int> ReadTask = null;
        private readonly Queue<string> ReadLineBuffer = new Queue<string>();
        private readonly ManualResetEventSlim WriteDone = new ManualResetEventSlim(true);

        #endregion

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
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the network stream.
        /// </summary>
        private NetworkStream NetworkStream { get; set; }

        /// <summary>
        /// Gets or sets the SSL stream.
        /// </summary>
        private SslStream SecureStream { get; set; }

        /// <summary>
        /// Gets the active stream. Returns an SSL stream if the connection is secure, otherwise returns 
        /// the underlying NetworkStream
        /// </summary>
        public Stream ActiveStream => SecureStream ?? NetworkStream as Stream;

        /// <summary>
        /// Gets a value indicating whether the current connection stream is an SSL stream.
        /// </summary>
        public bool IsActiveStreamSecure => SecureStream != null;

        /// <summary>
        /// Gets the text encoding for send and receive operations.
        /// </summary>
        public Encoding TextEncoding { get; }

        /// <summary>
        /// Gets the remote end point of this TCP connection.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; private set; }

        /// <summary>
        /// Gets the local end point of this TCP connection.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        /// <summary>
        /// Gets the remote client of this TCP connection.
        /// </summary>
        public TcpClient RemoteClient { get; private set; }

        /// <summary>
        /// When in continuous reading mode, and if set to greater than 0,
        /// a Data reception event will be fired whenever the amount of bytes
        /// determined by this property has been received. Useful for fixed-length message protocols.
        /// </summary>
        public int ProtocolBlockSize { get; }

        /// <summary>
        /// Gets a value indicating whether this connection is in continuous reading mode.
        /// Remark: Whenever a disconnect event occurs, the background thread is terminated
        /// and this property will return false whenever the reading thread is not active.
        /// Therefore, even if continuous reading was not disabled in the constructor, this property
        /// might return false.
        /// </summary>
        public bool IsContinuousReadingEnabled => ContinuousReadingThread != null;

        /// <summary>
        /// Gets the start time at which the connection was started in UTC.
        /// </summary>
        public DateTime ConnectionStartTimeUtc { get; }

        /// <summary>
        /// Gets the start time at which the connection was started in local time.
        /// </summary>
        public DateTime ConnectionStartTime => ConnectionStartTimeUtc.ToLocalTime();

        /// <summary>
        /// Gets the duration of the connection.
        /// </summary>
        public TimeSpan ConnectionDuration => DateTime.UtcNow.Subtract(ConnectionStartTimeUtc);

        /// <summary>
        /// Gets the last time data was received at in UTC.
        /// </summary>
        public DateTime DataReceivedLastTimeUtc { get; private set; }

        /// <summary>
        /// Gets how long has elapsed since data was last received.
        /// </summary>
        public TimeSpan DataReceivedIdleDuration => DateTime.UtcNow.Subtract(DataReceivedLastTimeUtc);

        /// <summary>
        /// Gets the last time at which data was sent in UTC.
        /// </summary>
        public DateTime DataSentLastTimeUtc { get; private set; }

        /// <summary>
        /// Gets how long has elapsed since data was last sent
        /// </summary>
        public TimeSpan DataSentIdleDuration => DateTime.UtcNow.Subtract(DataSentLastTimeUtc);

        /// <summary>
        /// Gets a value indicating whether this connection is connected.
        /// Remarks: This property polls the socket internally and checks if it is available to read data from it.
        /// If disconnect has been called, then this property will return false.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (DisconnectCalls > 0)
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="textEncoding">The text encoding.</param>
        /// <param name="newLineSequence">The new line sequence used for read and write operations.</param>
        /// <param name="disableContinuousReading">if set to <c>true</c> [disable continuous reading].</param>
        /// <param name="blockSize">Size of the block. -- set to 0 or less to disable</param>
        public Connection(TcpClient client, Encoding textEncoding, string newLineSequence, bool disableContinuousReading,
            int blockSize)
        {
            // Setup basic properties
            Id = Guid.NewGuid();
            TextEncoding = textEncoding;

            // Setup new line sequence
            if (string.IsNullOrEmpty(newLineSequence))
                throw new ArgumentException("Argument cannot be null", nameof(newLineSequence));

            NewLineSequence = newLineSequence;
            NewLineSequenceBytes = TextEncoding.GetBytes(NewLineSequence);
            NewLineSequenceChars = NewLineSequence.ToCharArray();
            NewLineSequenceLineSplitter = new[] { NewLineSequence };

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
            ReceiveBuffer = new byte[RemoteClient.ReceiveBufferSize * 2];
            ProtocolBlockSize = blockSize;
            ReceiveBufferPointer = 0;

            // Setup continuous reading mode if enabled
            if (disableContinuousReading) return;

#if !NET452
            ThreadPool.QueueUserWorkItem(new WaitCallback(PerformContinuousReading), this);
#else
            int availableWorkerThreads;
            int availableCompletionPortThreads;

            int maxWorkerThreads;
            int maxCompletionPortThreads;

            ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);
            ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThreads);

            var activeThreadPoolTreads = maxWorkerThreads - availableWorkerThreads;

            if (activeThreadPoolTreads < Environment.ProcessorCount / 4)
            {
                ThreadPool.QueueUserWorkItem(PerformContinuousReading, this);
                //Log.Trace($"Queued new ThreadPool Thread. Active TP Threads: {activeThreadPoolTreads}");
            }
            else
            {
                new Thread(PerformContinuousReading) { IsBackground = true }.Start();
                //Log.Trace($"Created standard thread. Active TP Threads: {activeThreadPoolTreads}");
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

        #endregion

        #region Continuous Read Methods

        /// <summary>
        /// Splits a given byte array.
        /// </summary>
        /// <param name="sourceArray">The source array.</param>
        /// <param name="length">The length.</param>
        /// <param name="splitSequence">The split sequence.</param>
        /// <returns></returns>
        private static List<Tuple<byte[], bool>> SplitByteArray(byte[] sourceArray, int length, byte[] splitSequence)
        {
            var result = new List<Tuple<byte[], bool>>();
            var lastSectionIndex = -1;

            for (var i = splitSequence.Length - 1; i < length; i++)
            {
                var matchesSequence = true;
                var sequenceIndex = -1;

                for (var offset = -splitSequence.Length + 1; offset <= 0; offset++)
                {
                    sequenceIndex++;
                    if (sourceArray[i + offset] != splitSequence[sequenceIndex])
                    {
                        matchesSequence = false;
                        break;
                    }
                }

                if (matchesSequence == false)
                    continue;

                var arraySection = new byte[i - lastSectionIndex];
                Array.Copy(sourceArray, lastSectionIndex + 1, arraySection, 0, arraySection.Length);
                result.Add(new Tuple<byte[], bool>(arraySection, true));
                lastSectionIndex = i;
            }

            {
                var lastSequenceLength = length - (lastSectionIndex + 1);
                if (lastSequenceLength > 0)
                {
                    var arraySection = new byte[lastSequenceLength];
                    Array.Copy(sourceArray, lastSectionIndex + 1, arraySection, 0, arraySection.Length);
                    result.Add(new Tuple<byte[], bool>(arraySection, false));
                }
            }


            return result;
        }

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
                ReceiveBuffer[ReceiveBufferPointer] = receivedData[i];
                ReceiveBufferPointer++;

                // Block size reached
                if (ProtocolBlockSize > 0 && ReceiveBufferPointer >= ProtocolBlockSize)
                {
                    var eventBuffer = new byte[ReceiveBuffer.Length];
                    Array.Copy(ReceiveBuffer, eventBuffer, eventBuffer.Length);

                    DataReceived(this,
                        new ConnectionDataReceivedEventArgs(
                            eventBuffer, ConnectionDataReceivedTrigger.BlockSizeReached, moreAvailable));
                    ReceiveBufferPointer = 0;
                    continue;
                }

                // The receive buffer is full. Time to flush
                if (ReceiveBufferPointer >= ReceiveBuffer.Length)
                {
                    var eventBuffer = new byte[ReceiveBuffer.Length];
                    Array.Copy(ReceiveBuffer, eventBuffer, eventBuffer.Length);
                    DataReceived(this,
                        new ConnectionDataReceivedEventArgs(
                            eventBuffer, ConnectionDataReceivedTrigger.BufferFull, moreAvailable));
                    ReceiveBufferPointer = 0;
                }
            }


            // Extract the segments split by newline terminated bytes
            var sequences = SplitByteArray(ReceiveBuffer, ReceiveBufferPointer, NewLineSequenceBytes);

            // Something really wrong happened
            if (sequences.Count == 0)
                throw new Exception("Split function failed! This is terribly wrong!");

            // We only have one sequence and it is not newline-terminated
            // we don't have to do anything.
            if (sequences.Count == 1 && sequences[0].Item2 == false)
                return;

            //Log.Trace(" > > > Showing sequences: ");

            // Process the events for each sequence
            for (var i = 0; i < sequences.Count; i++)
            {
                var sequenceBytes = sequences[i].Item1;
                var isNewLineTerminated = sequences[i].Item2;
                var isLast = i == sequences.Count - 1;

                //Log.Trace($"    ~ {i:00} ~ TERM: {isNewLineTerminated,-6} LAST: {isLast,-6} LEN: {sequenceBytes.Length,-4} {TextEncoding.GetString(sequenceBytes).TrimEnd(NewLineSequenceChars)}");

                if (isNewLineTerminated)
                {
                    var eventArgs = new ConnectionDataReceivedEventArgs(sequenceBytes,
                        ConnectionDataReceivedTrigger.NewLineSequenceEncountered, isLast == false);
                    DataReceived(this, eventArgs);
                }

                // Depending on the last segment determine what to do with the receive buffer
                if (isLast)
                {

                    if (isNewLineTerminated)
                    {
                        // Simply reset the buffer pointer if the last segment was also terminated
                        ReceiveBufferPointer = 0;
                    }
                    else
                    {
                        // If we have not received the termination sequence, then just shift the receive buffer to the left
                        // and adjust the pointer
                        Array.Copy(sequenceBytes, ReceiveBuffer, sequenceBytes.Length);
                        ReceiveBufferPointer = sequenceBytes.Length;
                    }
                }
            }
        }

        /// <summary>
        /// This is the body of the thread when performing continuous reading
        /// </summary>
        /// <param name="threadContext">The thread context.</param>
        private void PerformContinuousReading(object threadContext)
        {
            ContinuousReadingThread = Thread.CurrentThread;

            var receiveBuffer = new byte[RemoteClient.ReceiveBufferSize * 2];

            while (IsConnected && DisconnectCalls <= 0)
            {
                var doThreadSleep = false;

                try
                {
                    if (ReadTask == null)
                        ReadTask = ActiveStream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

                    if (ReadTask.Wait(ContinuousReadingInterval))
                    {
                        var bytesReceivedCount = ReadTask.Result;
                        if (bytesReceivedCount > 0)
                        {
                            DataReceivedLastTimeUtc = DateTime.UtcNow;
                            var buffer = new byte[bytesReceivedCount];
                            Array.Copy(receiveBuffer, 0, buffer, 0, bytesReceivedCount);
                            RaiseReceiveBufferEvents(buffer);
                        }

                        ReadTask = null;
                    }
                    else
                    {
                        doThreadSleep = DisconnectCalls <= 0;
                    }

                }
                catch (Exception ex)
                {
                    ex.Log(nameof(Connection), "Continuous Read operation errored");
                }
                finally
                {
                    if (doThreadSleep)
                        Thread.Sleep(ContinuousReadingInterval);
                }
            }
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Reads data from the remote client asynchronously and with the given timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Read methods have been disabled because continuous reading is enabled.</exception>
        /// <exception cref="TimeoutException">Reading data from {ActiveStream} timed out in {timeout.TotalMilliseconds} m</exception>
        public async Task<byte[]> ReadDataAsync(TimeSpan timeout)
        {
            if (IsContinuousReadingEnabled)
                throw new InvalidOperationException(
                    "Read methods have been disabled because continuous reading is enabled.");

            var receiveBuffer = new byte[RemoteClient.ReceiveBufferSize * 2];
            var receiveBuilder = new List<byte>(receiveBuffer.Length);

            try
            {
                var startTime = DateTime.UtcNow;

                while (receiveBuilder.Count <= 0)
                {
                    if (DateTime.UtcNow.Subtract(startTime) >= timeout)
                        throw new TimeoutException(
                            $"Reading data from {ActiveStream} timed out in {timeout.TotalMilliseconds} ms");

                    if (ReadTask == null)
                        ReadTask = ActiveStream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

                    if (ReadTask.Wait(ContinuousReadingInterval))
                    {
                        var bytesReceivedCount = ReadTask.Result;
                        if (bytesReceivedCount > 0)
                        {
                            DataReceivedLastTimeUtc = DateTime.UtcNow;
                            var buffer = new byte[bytesReceivedCount];
                            Array.Copy(receiveBuffer, 0, buffer, 0, bytesReceivedCount);
                            receiveBuilder.AddRange(buffer);
                        }

                        ReadTask = null;
                    }
                    else
                    {
                        await Task.Delay(ContinuousReadingInterval);
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
        /// <returns></returns>
        public async Task<byte[]> ReadDataAsync()
        {
            return await ReadDataAsync(TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Asynchronously reads data as text with the given timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public async Task<string> ReadTextAsync(TimeSpan timeout)
        {
            var buffer = await ReadDataAsync(timeout);
            return buffer == null ? null : TextEncoding.GetString(buffer);
        }

        /// <summary>
        /// Asynchronously reads data as text with a 5000 millisecond timeout.
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadTextAsync()
        {
            return await ReadTextAsync(TimeSpan.FromSeconds(5));
        }


        /// <summary>
        /// Performs the same task as this method's overload but it defaults to a read timeout of 30 seconds.
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadLineAsync()
        {
            return await ReadLineAsync(TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Reads the next available line of text in queue. Return null when no text is read.
        /// This method differs from the rest of the read methods because it keeps an internal
        /// queue of lines that are read from the stream and only returns the one line next in the queue.
        /// It is only recommended to use this method when you are working with text-based protocols
        /// and the rest of the read methods are not called.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Read methods have been disabled because continuous reading is enabled.</exception>
        public async Task<string> ReadLineAsync(TimeSpan timeout)
        {
            if (IsContinuousReadingEnabled)
                throw new InvalidOperationException(
                    "Read methods have been disabled because continuous reading is enabled.");

            if (ReadLineBuffer.Count > 0)
                return ReadLineBuffer.Dequeue();

            var builder = new StringBuilder();
            while (true)
            {
                var text = await ReadTextAsync(timeout);
                if (text.Length == 0)
                    break;

                builder.Append(text);

                if (text.EndsWith(NewLineSequence))
                {
                    var lines = builder.ToString().TrimEnd(NewLineSequenceChars)
                        .Split(NewLineSequenceLineSplitter, StringSplitOptions.None);
                    foreach (var item in lines)
                        ReadLineBuffer.Enqueue(item);

                    break;
                }
            }

            if (ReadLineBuffer.Count > 0)
                return ReadLineBuffer.Dequeue();

            return null;
        }

        #endregion

        #region Write Methods

        /// <summary>
        /// Writes data asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="forceFlush">if set to <c>true</c> [force flush].</param>
        /// <returns></returns>
        public async Task WriteDataAsync(byte[] buffer, bool forceFlush)
        {
            try
            {
                WriteDone.Wait();
                WriteDone.Reset();
                await ActiveStream.WriteAsync(buffer, 0, buffer.Length);
                if (forceFlush)
                    await ActiveStream.FlushAsync();

                DataSentLastTimeUtc = DateTime.UtcNow;
            }
            catch
            {
                throw;
            }
            finally
            {
                WriteDone.Set();
            }
        }

        /// <summary>
        /// Writes text asynchronously.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public async Task WriteTextAsync(string text)
        {
            await WriteTextAsync(text, TextEncoding);
        }

        /// <summary>
        /// Writes text asynchronously.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public async Task WriteTextAsync(string text, Encoding encoding)
        {
            var buffer = encoding.GetBytes(text);
            await WriteDataAsync(buffer, true);
        }

        /// <summary>
        /// Writes a line of text asynchronously. 
        /// The new line sequence is added automatically at the end of the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public async Task WriteLineAsync(string line, Encoding encoding)
        {
            var buffer = encoding.GetBytes($"{line}{NewLineSequence}");
            await WriteDataAsync(buffer, true);
        }

        /// <summary>
        /// Writes a line of text asynchronously. 
        /// The new line sequence is added automatically at the end of the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        public async Task WriteLineAsync(string line)
        {
            await WriteLineAsync(line, TextEncoding);
        }

        #endregion

        #region Socket Methods

        /// <summary>
        /// Upgrades the active stream to an SSL stream if this connection object is hosted in the server.
        /// </summary>
        /// <param name="serverCertificate">The server certificate.</param>
        /// <returns></returns>
        public async Task<bool> UpgradeToSecureAsServerAsync(X509Certificate2 serverCertificate)
        {
            if (IsActiveStreamSecure)
                return true;

            WriteDone.Wait();

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
        /// <returns></returns>
        public async Task<bool> UpgradeToSecureAsClientAsync(string hostname,
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
        /// <returns></returns>
        public async Task<bool> UpgradeToSecureAsClientAsync()
        {
            return await UpgradeToSecureAsClientAsync(Network.HostName.ToLowerInvariant(),
                (a, b, c, d) => true);
        }

        /// <summary>
        /// Disconnects this connection.
        /// </summary>
        public void Disconnect()
        {
            if (DisconnectCalls > 0)
                return;

            DisconnectCalls++;
            WriteDone.Wait();

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
                ContinuousReadingThread = null;
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (HasDisposed)
                return;

            // Release managed resources
            Disconnect();
            ContinuousReadingThread = null;
            WriteDone.Dispose();

            HasDisposed = true;
        }

        #endregion
    }
}
