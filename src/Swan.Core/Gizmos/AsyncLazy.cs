namespace Swan.Gizmos;

/// <summary>
/// An asynchronous version of the <see cref="Lazy{T, TMetadata}"/> construct
/// with embedded thread safety.
/// </summary>
/// <typeparam name="T">The type of object the factory method produces.</typeparam>
public class AsyncLazy<T>
    where T : class
{
    private readonly SemaphoreSlim FactorySemaphore = new(1, 1);
    private readonly Func<Task<T>> TaskFactory;
    private long creationFlag;
    private T? FactoryValue;

    /// <summary>
    /// Creates a new instance of the <see cref="AsyncLazy{T}"/> class.
    /// </summary>
    /// <param name="factory">The factory method that produces the typed object.</param>
    public AsyncLazy(Func<Task<T>> factory) => TaskFactory = factory;

    /// <summary>
    /// Gets a value indicating whether the value has been created.
    /// </summary>
    public bool IsValueCreated => Interlocked.Read(ref creationFlag) != 0;

    /// <summary>
    /// Asynchronously uses the provided factory method to produce a value
    /// for the first time. If the value is already created, it simply returns the previously
    /// created value.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    public async Task<T> GetValueAsync(CancellationToken ct = default)
    {
        if (IsValueCreated)
            return FactoryValue!;

        try
        {
            await FactorySemaphore.WaitAsync(ct).ConfigureAwait(false);

            if (FactoryValue is null && !IsValueCreated)
            {
                FactoryValue = await TaskFactory.Invoke().ConfigureAwait(false);
                Interlocked.Increment(ref creationFlag);
            }

            return FactoryValue!;
        }
        finally
        {
            FactorySemaphore.Release();
        }
    }
}

