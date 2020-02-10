using System;
using System.Collections.Generic;
using System.Threading;
using Swan.Configuration;
using Swan.Logging;

namespace Swan.Threading
{
    /// <summary>
    /// Represents an background worker abstraction with a life cycle and running at a independent thread.
    /// </summary>
    public abstract class RunnerBase : ConfiguredObject, IDisposable
    {
        private Thread? _worker;
        private CancellationTokenSource? _cancelTokenSource;
        private ManualResetEvent? _workFinished;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunnerBase"/> class.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        protected RunnerBase(bool isEnabled)
        {
            Name = GetType().Name;
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Gets the error messages.
        /// </summary>
        /// <value>
        /// The error messages.
        /// </value>
        public List<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public virtual void Start()
        {
            if (IsEnabled == false)
                return;

            "Start Requested".Debug(Name);
            _cancelTokenSource = new CancellationTokenSource();
            _workFinished = new ManualResetEvent(false);

            _worker = new Thread(() =>
            {
                _workFinished.Reset();
                IsRunning = true;
                try
                {
                    Setup();
                    DoBackgroundWork(_cancelTokenSource.Token);
                }
                catch (ThreadAbortException)
                {
                    $"{nameof(ThreadAbortException)} caught.".Warn(Name);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    $"{ex.GetType()}: {ex.Message}\r\n{ex.StackTrace}".Error(Name);
                }
                finally
                {
                    Cleanup();
                    _workFinished?.Set();
                    IsRunning = false;
                    "Stopped Completely".Debug(Name);
                }
            })
            {
                IsBackground = true,
                Name = $"{Name}Thread",
            };

            _worker.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public virtual void Stop()
        {
            if (IsEnabled == false || IsRunning == false)
                return;

            "Stop Requested".Debug(Name);
            _cancelTokenSource?.Cancel();
            var waitRetries = 5;
            while (waitRetries >= 1)
            {
                if (_workFinished?.WaitOne(250) ?? true)
                {
                    waitRetries = -1;
                    break;
                }

                waitRetries--;
            }

            if (waitRetries < 0)
            {
                "Workbench stopped gracefully".Debug(Name);
            }
            else
            {
                "Did not respond to stop request. Aborting thread and waiting . . .".Warn(Name);
                _worker?.Abort();

                if (_workFinished?.WaitOne(5000) == false)
                    "Waited and no response. Worker might have been left in an inconsistent state.".Error(Name);
                else
                    "Waited for worker and it finally responded (OK).".Debug(Name);
            }

            _workFinished?.Dispose();
            _workFinished = null;
        }
        
        /// <inheritdoc/>
        public void Dispose()
        {
            _cancelTokenSource?.Dispose();
            _workFinished?.Dispose();
        }

        /// <summary>
        /// Setups this instance.
        /// </summary>
        protected void Setup()
        {
            EnsureConfigurationNotLocked();
            OnSetup();
            LockConfiguration();
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        protected virtual void Cleanup()
        {
            // empty
        }

        /// <summary>
        /// Called when [setup].
        /// </summary>
        protected virtual void OnSetup()
        {
            // empty
        }

        /// <summary>
        /// Does the background work.
        /// </summary>
        /// <param name="cancellationToken">The ct.</param>
        protected abstract void DoBackgroundWork(CancellationToken cancellationToken);
    }
}
