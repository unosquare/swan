namespace Swan.Threading
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>
    /// Provides a base implementation for application workers.
    /// </summary>
    /// <seealso cref="IWorker" />
    public abstract class TimerWorkerBase : WorkerBase
    {
        private readonly object _syncLock = new object();
        private readonly Timer _timer;
        private bool _isTimerAlive = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerWorkerBase" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="period">The execution interval.</param>
        protected TimerWorkerBase(string name, TimeSpan period)
            : base(name, period)
        {
            // Instantiate the timer that will be used to schedule cycles
            _timer = new Timer(
                ExecuteTimerCallback,
                this,
                Timeout.Infinite,
                Timeout.Infinite);
        }

        /// <inheritdoc />
        public override Task<WorkerState> StartAsync()
        {
            lock (_syncLock)
            {
                if (WorkerState == WorkerState.Paused || WorkerState == WorkerState.Waiting)
                    return ResumeAsync();

                if (WorkerState != WorkerState.Created)
                    return Task.FromResult(WorkerState);

                if (IsStopRequested)
                    return Task.FromResult(WorkerState);

                var task = QueueStateChange(StateChangeRequest.Start);
                Interrupt();
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

                if (IsStopRequested)
                    return Task.FromResult(WorkerState);

                var task = QueueStateChange(StateChangeRequest.Pause);
                Interrupt();
                return task;
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

                if (IsStopRequested)
                    return Task.FromResult(WorkerState);

                var task = QueueStateChange(StateChangeRequest.Resume);
                Interrupt();
                return task;
            }
        }

        /// <inheritdoc />
        public override Task<WorkerState> StopAsync()
        {
            lock (_syncLock)
            {
                if (WorkerState == WorkerState.Stopped || WorkerState == WorkerState.Created)
                {
                    WorkerState = WorkerState.Stopped;
                    return Task.FromResult(WorkerState);
                }

                var task = QueueStateChange(StateChangeRequest.Stop);
                Interrupt();
                return task;
            }
        }

        /// <summary>
        /// Schedules a new cycle for execution. The delay is given in
        /// milliseconds. Passing a delay of 0 means a new cycle should be executed
        /// immediately.
        /// </summary>
        /// <param name="delay">The delay.</param>
        protected void ScheduleCycle(int delay)
        {
            lock (_syncLock)
            {
                if (!_isTimerAlive) return;
                _timer.Change(delay, Timeout.Infinite);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            lock (_syncLock)
            {
                if (!_isTimerAlive) return;
                _isTimerAlive = false;
                _timer.Dispose();
            }
        }

        /// <summary>
        /// Cancels the current token and schedules a new cycle immediately.
        /// </summary>
        private void Interrupt()
        {
            lock (_syncLock)
            {
                if (WorkerState == WorkerState.Stopped)
                    return;

                CycleCancellation.Cancel();
                ScheduleCycle(0);
            }
        }

        /// <summary>
        /// Executes the worker cycle control logic.
        /// This includes processing state change requests,
        /// the execution of use cycle code,
        /// and the scheduling of new cycles.
        /// </summary>
        private void ExecuteWorkerCycle()
        {
            CycleStopwatch.Restart();

            lock (_syncLock)
            {
                if (IsDisposing || IsDisposed)
                {
                    WorkerState = WorkerState.Stopped;

                    // Cancel any awaiters
                    try { StateChangedEvent.Set(); }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch { /* Ignore */ }
#pragma warning restore CA1031 // Do not catch general exception types

                    return;
                }

                // Prevent running another instance of the cycle
                if (CycleCompletedEvent.IsSet == false) return;

                // Lock the cycle and capture relevant state valid for this cycle
                CycleCompletedEvent.Reset();
            }

            var interruptToken = CycleCancellation.Token;
            var initialWorkerState = WorkerState;

            // Process the tasks that are awaiting
            if (ProcessStateChangeRequests())
                return;

            try
            {
                if (initialWorkerState != WorkerState.Waiting || interruptToken.IsCancellationRequested)
                    return;

                // Mark the state as Running
                WorkerState = WorkerState.Running;

                // Call the execution logic
                ExecuteCycleLogic(interruptToken);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                OnCycleException(ex);
            }
            finally
            {
                // Update the state
                WorkerState = initialWorkerState == WorkerState.Paused
                    ? WorkerState.Paused
                    : WorkerState.Waiting;

                lock (_syncLock)
                {
                    // Signal the cycle has been completed so new cycles can be executed
                    CycleCompletedEvent.Set();

                    // Schedule a new cycle
                    ScheduleCycle(!interruptToken.IsCancellationRequested
                        ? ComputeCycleDelay(initialWorkerState)
                        : 0);
                }
            }
        }

        /// <summary>
        /// Represents the callback that is executed when the <see cref="_timer"/> ticks.
        /// </summary>
        /// <param name="state">The state -- this contains the worker.</param>
        private void ExecuteTimerCallback(object state) => ExecuteWorkerCycle();

        /// <summary>
        /// Queues a transition in worker state for processing. Returns a task that can be awaited
        /// when the operation completes.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The awaitable task.</returns>
        private Task<WorkerState> QueueStateChange(StateChangeRequest request)
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

        /// <summary>
        /// Processes the state change queue by checking pending events and scheduling
        /// cycle execution accordingly. The <see cref="WorkerState"/> is also updated.
        /// </summary>
        /// <returns>Returns <c>true</c> if the execution should be terminated. <c>false</c> otherwise.</returns>
        private bool ProcessStateChangeRequests()
        {
            lock (_syncLock)
            {
                var currentState = WorkerState;
                var hasRequest = false;
                var schedule = 0;

                // Update the state according to request priority
                if (StateChangeRequests[StateChangeRequest.Stop] || IsDisposing || IsDisposed)
                {
                    hasRequest = true;
                    WorkerState = WorkerState.Stopped;
                    schedule = StateChangeRequests[StateChangeRequest.Stop] ? Timeout.Infinite : 0;
                }
                else if (StateChangeRequests[StateChangeRequest.Pause])
                {
                    hasRequest = true;
                    WorkerState = WorkerState.Paused;
                    schedule = Timeout.Infinite;
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
                    ClearStateChangeRequests(schedule, currentState, WorkerState);
                }

                return hasRequest;
            }
        }

        /// <summary>
        /// Signals all state change requests to set.
        /// </summary>
        /// <param name="schedule">The cycle schedule.</param>
        /// <param name="oldState">The previous worker state.</param>
        /// <param name="newState">The new worker state.</param>
        private void ClearStateChangeRequests(int schedule, WorkerState oldState, WorkerState newState)
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
                OnStateChangeProcessed(oldState, newState);
                ScheduleCycle(schedule);
            }
        }
    }
}
