namespace Swan.Gizmos;

/// <summary>
/// Provides a simple, thread-safe mechanism to limit the amount of calls
/// made to a subsequent statement. 
/// </summary>
public class SlidingWindowRateLimiter
{
    private readonly object SyncLock = new();
    private readonly SortedList<DateTime, int> CallTimes;

    /// <summary>
    /// Creates an instance of the <see cref="SlidingWindowRateLimiter"/>
    /// class with a sliding window of 60 seconds and a call limit of 60.
    /// </summary>
    public SlidingWindowRateLimiter()
        : this(TimeSpan.FromSeconds(60), 60)
    {
        // placheolder
    }

    /// <summary>
    /// Creates an instance of the <see cref="SlidingWindowRateLimiter"/> class.
    /// </summary>
    /// <param name="timeWindow">The time window. Must be positive.</param>
    /// <param name="limit">The call limit. Must be a positive number.</param>
    public SlidingWindowRateLimiter(TimeSpan timeWindow, int limit)
    {
        if (timeWindow.TotalMilliseconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeWindow));

        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        TimeWindow = timeWindow;
        Limit = limit;
        CallTimes = new(Limit + 1);
    }

    /// <summary>
    /// Gets the time window.
    /// </summary>
    public TimeSpan TimeWindow { get; }

    /// <summary>
    /// Gets the configured call limit.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Waits for a slot in the window to be available and
    /// returns when there is one.
    /// </summary>
    /// <returns>An awaitable task.</returns>
    public async Task WaitAsync(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            lock (SyncLock)
            {
                var windowStartTime = DateTime.UtcNow.Subtract(TimeWindow);
                var sortedCallTimes = CallTimes.Keys.ToArray();

                // get rid of times before window start time
                foreach (var callTime in sortedCallTimes)
                {
                    // once we've reached the start time
                    // stop removing items
                    if (callTime >= windowStartTime)
                        break;

                    CallTimes.Remove(callTime);
                }

                if (CallTimes.Count < Limit)
                {
                    CallTimes.Add(DateTime.UtcNow, default);
                    return;
                }
            }

            await Task.Delay(1, ct).ConfigureAwait(false);
        }
    }
}
