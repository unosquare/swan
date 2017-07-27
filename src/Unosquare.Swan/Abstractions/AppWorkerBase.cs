namespace Unosquare.Swan.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A base implementation of an Application service containing a worker task that performs background processing.
    /// </summary>
    public abstract class AppWorkerBase
    {
        #region Property Backing

        private AppWorkerState _workerState = AppWorkerState.Stopped;
        private readonly object _syncLock = new object();
        private CancellationTokenSource _tokenSource;

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
            IsBusy = false;
        }

        #endregion

        #region Abstract and Virtual Methods

        /// <summary>
        /// Creates the worker task.
        /// </summary>
        /// <exception cref="InvalidOperationException">Worker Thread seems to be still running.</exception>
        private void CreateWorker()
        {
            _tokenSource = new CancellationTokenSource();
            _tokenSource.Token.Register(() =>
            {
                IsBusy = false;
                OnWorkerThreadExit();
            });

            Task.Factory.StartNew(() =>
            {
                IsBusy = true;

                try
                {
                    WorkerThreadLoop();
                }
                catch (AggregateException)
                {
                    // Ignored
                }
                catch (Exception ex)
                {
                    ex.Log(GetType().Name);
                    OnWorkerThreadLoopException(ex);

                    if (_tokenSource.IsCancellationRequested == false)
                        _tokenSource.Cancel();
                }
            }, _tokenSource.Token);
        }

        /// <summary>
        /// Called when an unhandled exception is thrown.
        /// </summary>
        /// <param name="ex">The ex.</param>
        protected virtual void OnWorkerThreadLoopException(Exception ex)
            => "Service exception detected.".Debug(GetType().Name, ex);

        /// <summary>
        /// This method is called when the user loop has exited
        /// </summary>
        protected virtual void OnWorkerThreadExit() => "Service thread is stopping.".Debug(GetType().Name);

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
            get { return _workerState; }
            private set
            {
                lock (_syncLock)
                {
                    if (value == _workerState) return;

                    $"Service state changing from {State} to {value}".Debug(GetType().Name);
                    var newState = value;
                    var oldState = _workerState;
                    _workerState = value;

                    StateChanged?.Invoke(this, new AppWorkerStateChangedEventArgs(oldState, newState));
                }
            }
        }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken => _tokenSource?.Token ?? default(CancellationToken);

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

            CreateWorker();
            State = AppWorkerState.Running;
        }

        /// <summary>
        /// Stops and disposes service resources.
        /// </summary>
        /// <exception cref="InvalidOperationException">Service cannot be stopped because it is not running.</exception>
        public virtual void Stop()
        {
            if (State != AppWorkerState.Running) return;

            _tokenSource?.Cancel();
            "Service stop requested.".Debug(GetType().Name);
            State = AppWorkerState.Stopped;
        }

        #endregion
    }
}