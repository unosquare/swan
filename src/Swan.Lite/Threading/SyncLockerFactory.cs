using System;
using System.Threading;

namespace Swan.Threading
{
    /// <summary>
    /// Provides factory methods to create synchronized reader-writer locks
    /// that support a generalized locking and releasing api and syntax.
    /// </summary>
    public static class SyncLockerFactory
    {
        #region Enums and Interfaces

        /// <summary>
        /// Enumerates the locking operations.
        /// </summary>
        private enum LockHolderType
        {
            Read,
            Write,
        }

        /// <summary>
        /// Defines methods for releasing locks.
        /// </summary>
        private interface ISyncReleasable
        {
            /// <summary>
            /// Releases the writer lock.
            /// </summary>
            void ReleaseWriterLock();

            /// <summary>
            /// Releases the reader lock.
            /// </summary>
            void ReleaseReaderLock();
        }

        #endregion

        #region Factory Methods
        
        /// <summary>
        /// Creates a reader-writer lock backed by a standard ReaderWriterLock.
        /// </summary>
        /// <returns>The synchronized locker.</returns>
        public static ISyncLocker Create() => new SyncLocker();

        /// <summary>
        /// Creates a reader-writer lock backed by a ReaderWriterLockSlim.
        /// </summary>
        /// <returns>The synchronized locker.</returns>
        public static ISyncLocker CreateSlim() => new SyncLockerSlim();

        /// <summary>
        /// Creates a reader-writer lock.
        /// </summary>
        /// <param name="useSlim">if set to <c>true</c> it uses the Slim version of a reader-writer lock.</param>
        /// <returns>The Sync Locker.</returns>
        public static ISyncLocker Create(bool useSlim) => useSlim ? CreateSlim() : Create();

        #endregion

        #region Private Classes

        /// <summary>
        /// The lock releaser. Calling the dispose method releases the lock entered by the parent SyncLocker.
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        private sealed class SyncLockReleaser : IDisposable
        {
            private readonly ISyncReleasable _parent;
            private readonly LockHolderType _operation;

            private bool _isDisposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="SyncLockReleaser"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="operation">The operation.</param>
            public SyncLockReleaser(ISyncReleasable parent, LockHolderType operation)
            {
                _parent = parent;
                _operation = operation;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_isDisposed) return;
                _isDisposed = true;

                if (_operation == LockHolderType.Read)
                    _parent.ReleaseReaderLock();
                else
                    _parent.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// The Sync Locker backed by a ReaderWriterLock.
        /// </summary>
        /// <seealso cref="ISyncLocker" />
        /// <seealso cref="ISyncReleasable" />
        private sealed class SyncLocker : ISyncLocker, ISyncReleasable
        {
            private bool _isDisposed;
            private ReaderWriterLock? _locker = new ReaderWriterLock();

            /// <inheritdoc />
            public IDisposable AcquireReaderLock()
            {
                _locker?.AcquireReaderLock(Timeout.Infinite);
                return new SyncLockReleaser(this, LockHolderType.Read);
            }

            /// <inheritdoc />
            public IDisposable AcquireWriterLock()
            {
                _locker?.AcquireWriterLock(Timeout.Infinite);
                return new SyncLockReleaser(this, LockHolderType.Write);
            }

            /// <inheritdoc />
            public void ReleaseWriterLock() => _locker?.ReleaseWriterLock();

            /// <inheritdoc />
            public void ReleaseReaderLock() => _locker?.ReleaseReaderLock();

            /// <inheritdoc />
            public void Dispose()
            {
                if (_isDisposed) return;
                _isDisposed = true;
                _locker?.ReleaseLock();
                _locker = null;
            }
        }

        /// <summary>
        /// The Sync Locker backed by ReaderWriterLockSlim.
        /// </summary>
        /// <seealso cref="ISyncLocker" />
        /// <seealso cref="ISyncReleasable" />
        private sealed class SyncLockerSlim : ISyncLocker, ISyncReleasable
        {
            private bool _isDisposed;

            private ReaderWriterLockSlim _locker
                = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            /// <inheritdoc />
            public IDisposable AcquireReaderLock()
            {
                _locker?.EnterReadLock();
                return new SyncLockReleaser(this, LockHolderType.Read);
            }

            /// <inheritdoc />
            public IDisposable AcquireWriterLock()
            {
                _locker?.EnterWriteLock();
                return new SyncLockReleaser(this, LockHolderType.Write);
            }

            /// <inheritdoc />
            public void ReleaseWriterLock() => _locker?.ExitWriteLock();

            /// <inheritdoc />
            public void ReleaseReaderLock() => _locker?.ExitReadLock();

            /// <inheritdoc />
            public void Dispose()
            {
                if (_isDisposed) return;
                _isDisposed = true;
                _locker?.Dispose();
                _locker = null;
            }
        }

        #endregion
    }
}
