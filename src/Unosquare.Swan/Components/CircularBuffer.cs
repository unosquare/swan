namespace Unosquare.Swan.Components
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A fixed-size buffer that acts as an infinite length one.
    /// This buffer is backed by unmanaged, very fast memory so ensure you call
    /// the dispose method when you are done using it.
    /// Only for Windows.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class CircularBuffer : IDisposable
    {
        /// <summary>
        /// The locking object to perform synchronization.
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The unmanaged buffer
        /// </summary>
        private IntPtr _buffer = IntPtr.Zero;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer"/> class.
        /// </summary>
        /// <param name="bufferLength">Length of the buffer.</param>
        public CircularBuffer(int bufferLength)
        {
#if !NET452
            if (Runtime.OS != Swan.OperatingSystem.Windows)
                throw new InvalidOperationException("CircularBuffer component is only available in Windows");
#endif

            Length = bufferLength;
            _buffer = Marshal.AllocHGlobal(Length);
        }
        
        #region Properties

        /// <summary>
        /// Gets the capacity of this buffer.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the current, 0-based read index
        /// </summary>
        /// <value>
        /// The index of the read.
        /// </value>
        public int ReadIndex { get; private set; }

        /// <summary>
        /// Gets the current, 0-based write index.
        /// </summary>
        /// <value>
        /// The index of the write.
        /// </value>
        public int WriteIndex { get; private set; }

        /// <summary>
        /// Gets an the object associated with the last write
        /// </summary>
        /// <value>
        /// The write tag.
        /// </value>
        public TimeSpan WriteTag { get; private set; } = TimeSpan.MinValue;

        /// <summary>
        /// Gets the available bytes to read.
        /// </summary>
        /// <value>
        /// The readable count.
        /// </value>
        public int ReadableCount { get; private set; }

        /// <summary>
        /// Gets the number of bytes that can be written.
        /// </summary>
        /// <value>
        /// The writable count.
        /// </value>
        public int WritableCount => Length - ReadableCount;

        /// <summary>
        /// Gets percentage of used bytes (readbale/available, from 0.0 to 1.0).
        /// </summary>
        /// <value>
        /// The capacity percent.
        /// </value>
        public double CapacityPercent => 1.0 * ReadableCount / Length;

        #endregion

        #region Methods

        /// <summary>
        /// Reads the specified number of bytes into the target array.
        /// </summary>
        /// <param name="requestedBytes">The requested bytes.</param>
        /// <param name="target">The target.</param>
        /// <param name="targetOffset">The target offset.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Exception that is thrown when a method call is invalid for the object's current state
        /// </exception>
        public void Read(int requestedBytes, byte[] target, int targetOffset)
        {
            lock (_syncLock)
            {
                if (requestedBytes > ReadableCount)
                {
                    throw new InvalidOperationException(
                        $"Unable to read {requestedBytes} bytes. Only {ReadableCount} bytes are available");
                }

                var readCount = 0;
                while (readCount < requestedBytes)
                {
                    var copyLength = Math.Min(Length - ReadIndex, requestedBytes - readCount);
                    var sourcePtr = _buffer + ReadIndex;
                    Marshal.Copy(sourcePtr, target, targetOffset + readCount, copyLength);

                    readCount += copyLength;
                    ReadIndex += copyLength;
                    ReadableCount -= copyLength;

                    if (ReadIndex >= Length)
                        ReadIndex = 0;
                }
            }
        }

        /// <summary>
        /// Writes data to the backing buffer using the specified pointer and length.
        /// and associating a write tag for this operation.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="length">The length.</param>
        /// <param name="writeTag">The write tag.</param>
        /// <exception cref="System.InvalidOperationException">Read</exception>
        public void Write(IntPtr source, int length, TimeSpan writeTag)
        {
            lock (_syncLock)
            {
                if (ReadableCount + length > Length)
                {
                    throw new InvalidOperationException(
                        $"Unable to write to circular buffer. Call the {nameof(Read)} method to make some additional room");
                }

                var writeCount = 0;
                while (writeCount < length)
                {
                    var copyLength = Math.Min(Length - WriteIndex, length - writeCount);
                    var sourcePtr = source + writeCount;
                    var targetPtr = _buffer + WriteIndex;
                    CopyMemory(targetPtr, sourcePtr, (uint) copyLength);

                    writeCount += copyLength;
                    WriteIndex += copyLength;
                    ReadableCount += copyLength;

                    if (WriteIndex >= Length)
                        WriteIndex = 0;
                }

                WriteTag = writeTag;
            }
        }

        /// <summary>
        /// Resets all states as if this buffer had just been created.
        /// </summary>
        public void Clear()
        {
            lock (_syncLock)
            {
                WriteIndex = 0;
                ReadIndex = 0;
                WriteTag = TimeSpan.MinValue;
                ReadableCount = 0;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_buffer == IntPtr.Zero) return;

            Marshal.FreeHGlobal(_buffer);
            _buffer = IntPtr.Zero;
            Length = 0;
        }

        /// <summary>
        /// Fast pointer memory block copy function
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        /// <param name="length">The length.</param>
        [DllImport("kernel32")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        #endregion
    }
}