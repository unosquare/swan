﻿namespace Swan.Threading;

/// <summary>
/// Provides a base implementation for application workers
/// that perform continuous, long-running tasks. This class
/// provides the ability to perform fine-grained control on these tasks.
/// </summary>
/// <seealso cref="IWorker" />
public abstract class ThreadWorkerBase : WorkerBase
{
    private readonly object _syncLock = new();
    private readonly Thread _thread;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadWorkerBase"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="priority">The thread priority.</param>
    /// <param name="period">The interval of cycle execution.</param>
    /// <param name="delayProvider">The cycle delay provide implementation.</param>
    protected ThreadWorkerBase(string name, ThreadPriority priority, TimeSpan period, IWorkerDelayProvider? delayProvider)
        : base(name, period)
    {
        DelayProvider = delayProvider;
        _thread = new(RunWorkerLoop)
        {
            IsBackground = true,
            Priority = priority,
            Name = name,
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadWorkerBase"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="period">The execution interval.</param>
    protected ThreadWorkerBase(string name, TimeSpan period)
        : this(name, ThreadPriority.Normal, period, WorkerDelayProvider.Default)
    {
        // placeholder
    }

    /// <summary>
    /// Provides an implementation on a cycle delay provider.
    /// </summary>
    protected IWorkerDelayProvider? DelayProvider { get; }

    /// <inheritdoc />
    public override Task<WorkerState> StartAsync()
    {
        lock (_syncLock)
        {
            if (WorkerState is WorkerState.Paused or WorkerState.Waiting)
                return ResumeAsync();

            if (WorkerState != WorkerState.Created)
                return Task.FromResult(WorkerState);

            if (IsStopRequested)
                return Task.FromResult(WorkerState);

            var task = QueueStateChange(StateChangeRequest.Start);
            _thread.Start();
            return task;
        }
    }

    /// <inheritdoc />
    public override Task<WorkerState> PauseAsync()
    {
        lock (_syncLock)
        {
            if (WorkerState != WorkerState.Running && WorkerState != WorkerState.Waiting)
                return Task.FromResult(WorkerState);

            return IsStopRequested ? Task.FromResult(WorkerState) : QueueStateChange(StateChangeRequest.Pause);
        }
    }

    /// <inheritdoc />
    public override Task<WorkerState> ResumeAsync()
    {
        lock (_syncLock)
        {
            if (WorkerState == WorkerState.Created)
                return StartAsync();

            if (WorkerState != WorkerState.Paused && WorkerState != WorkerState.Waiting)
                return Task.FromResult(WorkerState);

            return IsStopRequested ? Task.FromResult(WorkerState) : QueueStateChange(StateChangeRequest.Resume);
        }
    }

    /// <inheritdoc />
    public override Task<WorkerState> StopAsync()
    {
        lock (_syncLock)
        {
            if (WorkerState is WorkerState.Stopped or WorkerState.Created)
            {
                WorkerState = WorkerState.Stopped;
                return Task.FromResult(WorkerState);
            }

            return QueueStateChange(StateChangeRequest.Stop);
        }
    }

    /// <summary>
    /// Suspends execution queues a new new cycle for execution. The delay is given in
    /// milliseconds. When overridden in a derived class the wait handle will be set
    /// whenever an interrupt is received.
    /// </summary>
    /// <param name="wantedDelay">The remaining delay to wait for in the cycle.</param>
    /// <param name="delayTask">Contains a reference to a task with the scheduled period delay.</param>
    /// <param name="token">The cancellation token to cancel waiting.</param>
    protected virtual void ExecuteCycleDelay(int wantedDelay, Task delayTask, CancellationToken token) =>
        DelayProvider?.ExecuteCycleDelay(wantedDelay, delayTask, token);

    /// <inheritdoc />
    protected override void OnDisposing()
    {
        lock (_syncLock)
        {
            if ((_thread.ThreadState & ThreadState.Unstarted) != ThreadState.Unstarted)
                _thread.Join();
        }
    }

    /// <summary>
    /// Implements worker control, execution and delay logic in a loop.
    /// </summary>
    private void RunWorkerLoop()
    {
        while (WorkerState != WorkerState.Stopped && !IsDisposing && !IsDisposed)
        {
            CycleStopwatch.Restart();
            var interruptToken = CycleCancellation.Token;
            var period = GetPeriod();
            var delayTask = Task.Delay(period, interruptToken);
            var initialWorkerState = WorkerState;

            // Lock the cycle and capture relevant state valid for this cycle
            CycleCompletedEvent.Reset();

            // Process the tasks that are awaiting
            if (ProcessStateChangeRequests())
                continue;

            try
            {
                if (initialWorkerState == WorkerState.Waiting &&
                    !interruptToken.IsCancellationRequested)
                {
                    // Mark the state as Running
                    WorkerState = WorkerState.Running;

                    // Call the execution logic
                    ExecuteCycleLogic(interruptToken);
                }
            }
            catch (Exception ex)
            {
                OnCycleException(ex);
            }
            finally
            {
                CleanLoop(initialWorkerState, delayTask, interruptToken);
            }
        }

        ClearStateChangeRequests();
        WorkerState = WorkerState.Stopped;
    }

    private int GetPeriod() => Period.TotalMilliseconds >= int.MaxValue ? -1 : Convert.ToInt32(Math.Floor(Period.TotalMilliseconds));

    private void CleanLoop(WorkerState initialWorkerState, Task delayTask, CancellationToken interruptToken)
    {
        // Update the state
        WorkerState = initialWorkerState == WorkerState.Paused
            ? WorkerState.Paused
            : WorkerState.Waiting;

        // Signal the cycle has been completed so new cycles can be executed
        CycleCompletedEvent.Set();

        if (interruptToken.IsCancellationRequested)
            return;

        var cycleDelay = ComputeCycleDelay(initialWorkerState);
        if (cycleDelay == Timeout.Infinite)
            delayTask = Task.Delay(Timeout.Infinite, interruptToken);

        ExecuteCycleDelay(
            cycleDelay,
            delayTask,
            CycleCancellation.Token);
    }

    /// <summary>
    /// Processes the state change request by checking pending events and scheduling
    /// cycle execution accordingly. The <see cref="WorkerState"/> is also updated.
    /// </summary>
    /// <returns>Returns <c>true</c> if the execution should be terminated. <c>false</c> otherwise.</returns>
    private bool ProcessStateChangeRequests()
    {
        lock (_syncLock)
        {
            var hasRequest = false;
            var currentState = WorkerState;

            // Update the state in the given priority
            if (StateChangeRequests[StateChangeRequest.Stop] || IsDisposing || IsDisposed)
            {
                hasRequest = true;
                WorkerState = WorkerState.Stopped;
            }
            else if (StateChangeRequests[StateChangeRequest.Pause])
            {
                hasRequest = true;
                WorkerState = WorkerState.Paused;
            }
            else if (StateChangeRequests[StateChangeRequest.Start] || StateChangeRequests[StateChangeRequest.Resume])
            {
                hasRequest = true;
                WorkerState = WorkerState.Waiting;
            }

            // Signals all state changes to continue
            // as a command has been handled.
            if (hasRequest)
            {
                ClearStateChangeRequests();
                OnStateChangeProcessed(currentState, WorkerState);
            }

            return hasRequest;
        }
    }

    /// <summary>
    /// Signals all state change requests to set.
    /// </summary>
    private void ClearStateChangeRequests()
    {
        lock (_syncLock)
        {
            // Mark all events as completed
            StateChangeRequests[StateChangeRequest.Start] = false;
            StateChangeRequests[StateChangeRequest.Pause] = false;
            StateChangeRequests[StateChangeRequest.Resume] = false;
            StateChangeRequests[StateChangeRequest.Stop] = false;

            StateChangedEvent.Set();
            CycleCompletedEvent.Set();
        }
    }
}
