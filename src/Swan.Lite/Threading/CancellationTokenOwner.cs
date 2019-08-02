using System;
using System.Threading;

namespace Swan.Threading
{
    /// <summary>
    /// Acts as a <see cref="CancellationTokenSource"/> but with reusable tokens.
    /// </summary>
    public sealed class CancellationTokenOwner : IDisposable
    {
        private readonly object _syncLock = new object();
        private bool _isDisposed;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Gets the token of the current.
        /// </summary>
        public CancellationToken Token
        {
            get
            {
                lock (_syncLock)
                {
                    return _isDisposed
                        ? CancellationToken.None
                        : _tokenSource.Token;
                }
            }
        }

        /// <summary>
        /// Cancels the last referenced token and creates a new token source.
        /// </summary>
        public void Cancel()
        {
            lock (_syncLock)
            {
                if (_isDisposed) return;
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = new CancellationTokenSource();
            }
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            lock (_syncLock)
            {
                if (_isDisposed) return;

                if (disposing)
                {
                    _tokenSource.Cancel();
                    _tokenSource.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}
