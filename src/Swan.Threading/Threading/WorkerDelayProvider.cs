﻿namespace Swan.Threading;

using System.Diagnostics;

/// <summary>
/// Represents a class that implements delay logic for thread workers.
/// </summary>
public static class WorkerDelayProvider
{
    /// <summary>
    /// Gets the default delay provider.
    /// </summary>
    public static IWorkerDelayProvider? Default => TokenTimeout;

    /// <summary>
    /// Provides a delay implementation which simply waits on the task and cancels on
    /// the cancellation token.
    /// </summary>
    public static IWorkerDelayProvider Token => new TokenCancellableDelay();

    /// <summary>
    /// Provides a delay implementation which waits on the task and cancels on both,
    /// the cancellation token and a wanted delay timeout.
    /// </summary>
    public static IWorkerDelayProvider? TokenTimeout => new TokenTimeoutCancellableDelay();

    /// <summary>
    /// Provides a delay implementation which uses short sleep intervals of 5ms.
    /// </summary>
    public static IWorkerDelayProvider TokenSleep => new TokenSleepDelay();

    /// <summary>
    /// Provides a delay implementation which uses short delay intervals of 5ms and
    /// a wait on the delay task in the final loop.
    /// </summary>
    public static IWorkerDelayProvider SteppedToken => new SteppedTokenDelay();

    private class TokenCancellableDelay : IWorkerDelayProvider
    {
        public void ExecuteCycleDelay(int wantedDelay, Task delayTask, CancellationToken token)
        {
            switch (wantedDelay)
            {
                case 0 or < -1:
                    return;
                // for wanted delays of less than 30ms it is not worth
                // passing a timeout or a token as it only adds unnecessary
                // overhead.
                case <= 30:
                    try { delayTask.Wait(token); }
                    catch { /* ignore */ }
                    return;
                default:
                    // only wait on the cancellation token
                    // or until the task completes normally
                    try { delayTask.Wait(token); }
                    catch { /* ignore */ }

                    break;
            }
        }
    }

    private class TokenTimeoutCancellableDelay : IWorkerDelayProvider
    {
        public void ExecuteCycleDelay(int wantedDelay, Task delayTask, CancellationToken token)
        {
            switch (wantedDelay)
            {
                case 0 or < -1:
                    return;
                // for wanted delays of less than 30ms it is not worth
                // passing a timeout or a token as it only adds unnecessary
                // overhead.
                case <= 30:
                    try { delayTask.Wait(token); }
                    catch { /* ignore */ }
                    return;
                default:
                    try { delayTask.Wait(wantedDelay, token); }
                    catch { /* ignore */ }

                    break;
            }
        }
    }

    private class TokenSleepDelay : IWorkerDelayProvider
    {
        private readonly Stopwatch _elapsedWait = new();

        public void ExecuteCycleDelay(int wantedDelay, Task delayTask, CancellationToken token)
        {
            _elapsedWait.Restart();

            if (wantedDelay is 0 or < -1)
                return;

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(5);

                if (wantedDelay != Timeout.Infinite && _elapsedWait.ElapsedMilliseconds >= wantedDelay)
                    break;
            }
        }
    }

    private class SteppedTokenDelay : IWorkerDelayProvider
    {
        private const int StepMilliseconds = 15;
        private readonly Stopwatch _elapsedWait = new();

        public void ExecuteCycleDelay(int wantedDelay, Task delayTask, CancellationToken token)
        {
            _elapsedWait.Restart();

            if (wantedDelay is 0 or < -1)
                return;

            if (wantedDelay == Timeout.Infinite)
            {
                try { delayTask.Wait(wantedDelay, token); }
                catch { /* Ignore cancelled tasks */ }
                return;
            }

            LoopCycle(wantedDelay, delayTask, token);
        }

        private void LoopCycle(int wantedDelay, Task delayTask, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var remainingWaitTime = wantedDelay - Convert.ToInt32(_elapsedWait.ElapsedMilliseconds);

                // Exit for no remaining wait time
                if (remainingWaitTime <= 0)
                    break;

                if (remainingWaitTime >= StepMilliseconds)
                {
                    Task.Delay(StepMilliseconds, token).Wait(token);
                }
                else
                {
                    try
                    {
                        delayTask.Wait(remainingWaitTime);
                    }
                    catch
                    {
                        /* ignore cancellation of task exception */
                    }
                }

                if (_elapsedWait.ElapsedMilliseconds >= wantedDelay)
                    break;
            }
        }
    }
}
