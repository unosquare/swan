using System;
using System.Threading;

namespace Swan.Threading
{
    /// <summary>
    /// Provides a Manual Reset Event factory with a unified API.
    /// </summary>
    /// <example>
    /// The following example shows how to use the WaitEventFactory class.
    /// <code>
    /// using Swan.Threading;
    /// 
    /// public class Example
    /// {
    ///     // create a WaitEvent using the slim version
    ///     private static readonly IWaitEvent waitEvent = WaitEventFactory.CreateSlim(false);
    ///     
    ///     public static void Main()
    ///     {
    ///         Task.Factory.StartNew(() =>
    ///         {
    ///             DoWork(1);
    ///         });
    ///             
    ///         Task.Factory.StartNew(() =>
    ///         {
    ///             DoWork(2);
    ///         });
    ///             
    ///         // send first signal 
    ///         waitEvent.Complete();
    ///         waitEvent.Begin();
    ///             
    ///         Thread.Sleep(TimeSpan.FromSeconds(2));
    ///             
    ///         // send second signal
    ///         waitEvent.Complete();
    ///             
    ///         Terminal.Readline();
    ///     }
    ///         
    ///     public static void DoWork(int taskNumber)
    ///     {
    ///         $"Data retrieved:{taskNumber}".WriteLine();
    ///         waitEvent.Wait();
    ///              
    ///         Thread.Sleep(TimeSpan.FromSeconds(2));
    ///         $"All finished up {taskNumber}".WriteLine();
    ///     }
    ///  }
    /// </code>
    /// </example>
    public static class WaitEventFactory
    {
        #region Factory Methods

        /// <summary>
        /// Creates a Wait Event backed by a standard ManualResetEvent.
        /// </summary>
        /// <param name="isCompleted">if initially set to completed. Generally true.</param>
        /// <returns>The Wait Event.</returns>
        public static IWaitEvent Create(bool isCompleted) => new WaitEvent(isCompleted);

        /// <summary>
        /// Creates a Wait Event backed by a ManualResetEventSlim.
        /// </summary>
        /// <param name="isCompleted">if initially set to completed. Generally true.</param>
        /// <returns>The Wait Event.</returns>
        public static IWaitEvent CreateSlim(bool isCompleted) => new WaitEventSlim(isCompleted);

        /// <summary>
        /// Creates a Wait Event backed by a ManualResetEventSlim.
        /// </summary>
        /// <param name="isCompleted">if initially set to completed. Generally true.</param>
        /// <param name="useSlim">if set to <c>true</c> creates a slim version of the wait event.</param>
        /// <returns>The Wait Event.</returns>
        public static IWaitEvent Create(bool isCompleted, bool useSlim) => useSlim ? CreateSlim(isCompleted) : Create(isCompleted);

        #endregion

        #region Backing Classes

        /// <summary>
        /// Defines a WaitEvent backed by a ManualResetEvent.
        /// </summary>
        private class WaitEvent : IWaitEvent
        {
            private ManualResetEvent? _event;

            /// <summary>
            /// Initializes a new instance of the <see cref="WaitEvent"/> class.
            /// </summary>
            /// <param name="isCompleted">if set to <c>true</c> [is completed].</param>
            public WaitEvent(bool isCompleted)
            {
                _event = new ManualResetEvent(isCompleted);
            }

            /// <inheritdoc />
            public bool IsDisposed { get; private set; }

            /// <inheritdoc />
            public bool IsValid
            {
                get
                {
                    if (IsDisposed || _event == null) 
                        return false;

                    if (_event?.SafeWaitHandle?.IsClosed ?? true) 
                        return false;

                    return !(_event?.SafeWaitHandle?.IsInvalid ?? true);
                }
            }

            /// <inheritdoc />
            public bool IsCompleted => IsValid == false || (_event?.WaitOne(0) ?? true);

            /// <inheritdoc />
            public bool IsInProgress => !IsCompleted;

            /// <inheritdoc />
            public void Begin() => _event?.Reset();

            /// <inheritdoc />
            public void Complete() => _event?.Set();

            /// <inheritdoc />
            void IDisposable.Dispose()
            {
                if (IsDisposed) return;
                IsDisposed = true;

                _event?.Set();
                _event?.Dispose();
                _event = null;
            }

            /// <inheritdoc />
            public void Wait() => _event?.WaitOne();

            /// <inheritdoc />
            public bool Wait(TimeSpan timeout) => _event?.WaitOne(timeout) ?? true;
        }

        /// <summary>
        /// Defines a WaitEvent backed by a ManualResetEventSlim.
        /// </summary>
        private class WaitEventSlim : IWaitEvent
        {
            private ManualResetEventSlim? _event;

            /// <summary>
            /// Initializes a new instance of the <see cref="WaitEventSlim"/> class.
            /// </summary>
            /// <param name="isCompleted">if set to <c>true</c> [is completed].</param>
            public WaitEventSlim(bool isCompleted)
            {
                _event = new ManualResetEventSlim(isCompleted);
            }

            /// <inheritdoc />
            public bool IsDisposed { get; private set; }

            /// <inheritdoc />
            public bool IsValid =>
                !IsDisposed && _event?.WaitHandle?.SafeWaitHandle != null &&
                (!_event.WaitHandle.SafeWaitHandle.IsClosed && !_event.WaitHandle.SafeWaitHandle.IsInvalid);

            /// <inheritdoc />
            public bool IsCompleted => IsValid == false || _event?.IsSet == true;

            /// <inheritdoc />
            public bool IsInProgress => !IsCompleted;

            /// <inheritdoc />
            public void Begin() => _event?.Reset();

            /// <inheritdoc />
            public void Complete() => _event?.Set();

            /// <inheritdoc />
            void IDisposable.Dispose()
            {
                if (IsDisposed) return;
                IsDisposed = true;

                _event?.Set();
                _event?.Dispose();
                _event = null;
            }

            /// <inheritdoc />
            public void Wait() => _event?.Wait();

            /// <inheritdoc />
            public bool Wait(TimeSpan timeout) => _event?.Wait(timeout) ?? true;
        }

        #endregion
    }
}
