﻿namespace Swan.Threading;

using System.Diagnostics;

/// <summary>
/// Provides base infrastructure for Timer and Thread workers.
/// </summary>
/// <seealso cref="IWorker" />
public abstract class WorkerBase : IWorker, IDisposable
{
    // Since these are API property backers, we use interlocked to read from them
    // to avoid deadlocked reads
    private readonly object _syncLock = new();

    private readonly AtomicBoolean _isDisposed = new();
    private readonly AtomicBoolean _isDisposing = new();
    private readonly AtomicEnum<WorkerState> _workerState = new(WorkerState.Created);
    private readonly AtomicTimeSpan _timeSpan;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerBase"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="period">The execution interval.</param>
    protected WorkerBase(string name, TimeSpan period)
    {
        Name = name;
        _timeSpan = new(period);

        StateChangeRequests = new(5)
        {
            [StateChangeRequest.Start] = false,
            [StateChangeRequest.Pause] = false,
            [StateChangeRequest.Resume] = false,
            [StateChangeRequest.Stop] = false,
        };
    }

    /// <summary>
    /// Enumerates all the different state change requests.
    /// </summary>
    protected enum StateChangeRequest
    {
        /// <summary>
        /// No state change request.
        /// </summary>
        None,

        /// <summary>
        /// Start state change request
        /// </summary>
        Start,

        /// <summary>
        /// Pause state change request
        /// </summary>
        Pause,

        /// <summary>
        /// Resume state change request
        /// </summary>
        Resume,

        /// <summary>
        /// Stop state change request
        /// </summary>
        Stop,
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public TimeSpan Period
    {
        get => _timeSpan.Value;
        set => _timeSpan.Value = value;
    }

    /// <inheritdoc />
    public WorkerState WorkerState
    {
        get => _workerState.Value;
        protected set => _workerState.Value = value;
    }

    /// <inheritdoc />
    public bool IsDisposed
    {
        get => _isDisposed.Value;
        protected set => _isDisposed.Value = value;
    }

    /// <inheritdoc />
    public bool IsDisposing
    {
        get => _isDisposing.Value;
        protected set => _isDisposing.Value = value;
    }

    /// <summary>
    /// Gets the default period of 15 milliseconds which is the default precision for timers.
    /// </summary>
    protected static TimeSpan DefaultPeriod { get; } = TimeSpan.FromMilliseconds(15);

    /// <summary>
    /// Gets a value indicating whether stop has been requested.
    /// This is useful to prevent more requests from being issued.
    /// </summary>
    protected bool IsStopRequested => StateChangeRequests[StateChangeRequest.Stop];

    /// <summary>
    /// Gets the cycle stopwatch.
    /// </summary>
    protected Stopwatch CycleStopwatch { get; } = new();

    /// <summary>
    /// Gets the state change requests.
    /// </summary>
    protected Dictionary<StateChangeRequest, bool> StateChangeRequests { get; }

    /// <summary>
    /// Gets the cycle completed event.
    /// </summary>
    protected ManualResetEventSlim CycleCompletedEvent { get; } = new(true);

    /// <summary>
    /// Gets the state changed event.
    /// </summary>
    protected ManualResetEventSlim StateChangedEvent { get; } = new(true);

    /// <summary>
    /// Gets the cycle logic cancellation owner.
    /// </summary>
    protected CancellationTokenOwner CycleCancellation { get; } = new();

    /// <summary>
    /// Gets or sets the state change task.
    /// </summary>
    protected Task<WorkerState>? StateChangeTask { get; set; }

    /// <inheritdoc />
    public abstract Task<WorkerState> StartAsync();

    /// <inheritdoc />
    public abstract Task<WorkerState> PauseAsync();

    /// <inheritdoc />
    public abstract Task<WorkerState> ResumeAsync();

    /// <inheritdoc />
    public abstract Task<WorkerState> StopAsync();

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        lock (_syncLock)
        {
            if (IsDisposed || IsDisposing) return;
            IsDisposing = true;
        }

        // This also ensures the state change queue gets cleared
        StopAsync().Wait();
        StateChangedEvent.Set();
        CycleCompletedEvent.Set();

        OnDisposing();

        CycleStopwatch.Stop();
        StateChangedEvent.Dispose();
        CycleCompletedEvent.Dispose();
        CycleCancellation.Dispose();

        IsDisposed = true;
        IsDisposing = false;
    }

    /// <summary>
    /// Handles the cycle logic exceptions.
    /// </summary>
    /// <param name="ex">The exception that was thrown.</param>
    protected abstract void OnCycleException(Exception ex);

    /// <summary>
    /// Represents the user defined logic to be executed on a single worker cycle.
    /// Check the cancellation token continuously if you need responsive interrupts.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected abstract void ExecuteCycleLogic(CancellationToken cancellationToken);

    /// <summary>
    /// This method is called automatically when <see cref="Dispose()"/> is called.
    /// Makes sure you release all resources within this call.
    /// </summary>
    protected abstract void OnDisposing();

    /// <summary>
    /// Called when a state change request is processed.
    /// </summary>
    /// <param name="previousState">The state before the change.</param>
    /// <param name="newState">The new state.</param>
    protected virtual void OnStateChangeProcessed(WorkerState previousState, WorkerState newState)
    {
        // placeholder
    }

    /// <summary>
    /// Computes the cycle delay.
    /// </summary>
    /// <param name="initialWorkerState">Initial state of the worker.</param>
    /// <returns>The number of milliseconds to delay for.</returns>
    protected int ComputeCycleDelay(WorkerState initialWorkerState)
    {
        var elapsedMillis = CycleStopwatch.ElapsedMilliseconds;
        var period = Period;
        var periodMillis = period.TotalMilliseconds;
        var delayMillis = periodMillis - elapsedMillis;

        if (initialWorkerState == WorkerState.Paused || period == TimeSpan.MaxValue || delayMillis >= int.MaxValue)
            return Timeout.Infinite;

        return elapsedMillis >= periodMillis ? 0 : Convert.ToInt32(Math.Floor(delayMillis));
    }
    
    /// <summary>
    /// Queues a transition in worker state for processing. Returns a task that can be awaited
    /// when the operation completes.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The awaitable task.</returns>
    protected Task<WorkerState> QueueStateChange(StateChangeRequest request)
    {
        lock (_syncLock)
        {
            if (StateChangeTask != null)
                return StateChangeTask;

            var waitingTask = new Task<WorkerState>(() =>
            {
                StateChangedEvent.Wait();
                lock (_syncLock)
                {
                    StateChangeTask = null;
                    return WorkerState;
                }
            });

            StateChangeTask = waitingTask;
            StateChangedEvent.Reset();
            StateChangeRequests[request] = true;
            waitingTask.Start();
            CycleCancellation.Cancel();

            return waitingTask;
        }
    }
}
