namespace Unosquare.Swan.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A base implementation of an Application service containing a worker thread that performs background processing.
    /// </summary>
    public abstract class AppWorkerBase
    {
        #region Property Backing

        private Thread WorkerThread;
        private AppWorkerState WorkerState = AppWorkerState.Stopped;
        private readonly object SyncLock = new object();
        private volatile bool HasDisposed;

        /// <summary>
        /// Occurs when [state changed].
        /// </summary>
        public event EventHandler<AppWorkerStateChangedEventArgs> StateChanged;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AppWorkerBase"/> class.
        /// </summary>
        protected AppWorkerBase()
        {
            State = AppWorkerState.Stopped;
            CancellationPending = false;
            IsBusy = false;
        }

        #endregion

        #region Abstract and Virtual Methods

        /// <summary>
        /// Creates the worker thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">Worker Thread seems to be still running.</exception>
        private void CreateWorkerThread()
        {
            if (WorkerThread != null)
            {
                if (WorkerThread.IsAlive)
                    throw new InvalidOperationException("Worker Thread seems to be still running.");

                WorkerThread = null;
            }

            WorkerThread = new Thread(() =>
            {
                IsBusy = true;

                try
                {
                    WorkerThreadLoop();
                }
                catch (Exception ex)
                {
                    ex.Log(GetType());
                    OnWorkerThreadLoopException(ex);
                }
                finally
                {
                    OnWorkerThreadExit();

                    State = AppWorkerState.Stopped;
                    CancellationPending = false;
                    IsBusy = false;
                }
            })
            { IsBackground = true };

        }

        /// <summary>
        /// Called when an unhandled exception is thrown.
        /// </summary>
        /// <param name="ex">The ex.</param>
        protected virtual void OnWorkerThreadLoopException(Exception ex)
        {
            "Service exception detected.".Debug(GetType(), ex);
        }

        /// <summary>
        /// This method is called when the user loop has exited
        /// </summary>
        protected virtual void OnWorkerThreadExit()
        {
            "Service thread is stopping.".Debug(GetType());
        }

        /// <summary>
        /// Make the calling thread sleep in short intervals for the requested timespan.
        /// If Cancellation is requested, then it immediately returns.
        /// </summary>
        /// <param name="t">The t.</param>
        protected async void WaitForTimeout(TimeSpan t)
        {
            var timeoutDate = DateTime.UtcNow.Add(t);
            while (DateTime.UtcNow < timeoutDate)
            {
                if (CancellationPending) return;

                await Task.Delay(TimeSpan.FromMilliseconds(300));
            }
        }

        /// <summary>
        /// Implement this method as a loop that checks whether CancellationPending has been set to true
        /// If so, immediately exit the loop.
        /// </summary>
        protected abstract void WorkerThreadLoop();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the state of the application service.
        /// In other words, useful to know whether the service is running.
        /// </summary>
        public AppWorkerState State
        {
            get { return WorkerState; }
            private set
            {
                lock (SyncLock)
                {
                    if (value == WorkerState) return;

                    $"Service state changing from {State} to {value}".Debug(GetType());
                    var newState = value;
                    var oldState = WorkerState;
                    WorkerState = value;

                    StateChanged?.Invoke(this, new AppWorkerStateChangedEventArgs(oldState, newState));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user loop is pending cancellation.
        /// </summary>
        public bool CancellationPending { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the thread is busy
        /// </summary>
        public bool IsBusy { get; private set; }

        #endregion

        #region AppWorkerBase Methods

        /// <summary>
        /// Performs internal service initialization tasks required before starting the service.
        /// </summary>
        /// <exception cref="InvalidOperationException">Service cannot be initialized because it seems to be currently running</exception>
        public virtual void Initialize()
        {
            if (State != AppWorkerState.Stopped)
                throw new InvalidOperationException(
                    "Service cannot be initialized because it seems to be currently running");
        }

        /// <summary>
        /// Starts the application service. This call must not block the calling thread and must
        /// run on its own resources.
        /// </summary>
        /// <exception cref="InvalidOperationException">Service cannot be started because it seems to be currently running</exception>
        public virtual void Start()
        {
            if (State != AppWorkerState.Stopped)
                throw new InvalidOperationException("Service cannot be started because it seems to be currently running");

            CreateWorkerThread();
            WorkerThread.Start();
            State = AppWorkerState.Running;
        }

        /// <summary>
        /// Stops and disposes service resources.
        /// </summary>
        /// <exception cref="InvalidOperationException">Service cannot be stopped because it is not running.</exception>
        public virtual void Stop()
        {
            if (State != AppWorkerState.Running) return;

            "Service stop requested.".Debug(GetType());
            CancellationPending = true;
            WorkerThread.Join();
            IsBusy = false;
        }

        #endregion

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (HasDisposed) return;

            if (isDisposing)
            {
                "Service disposing.".Debug(GetType());

                if (WorkerThread != null)
                {
#if NET452
                    if (WorkerThread.IsAlive)
                        WorkerThread.Abort();
#endif

                    WorkerThread = null;
                }
            }

            HasDisposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}