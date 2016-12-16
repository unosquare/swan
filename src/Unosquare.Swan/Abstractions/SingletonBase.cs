namespace Unosquare.Swan.Abstractions
{
    using System;

    /// <summary>
    /// Represents a singleton pattern abstract class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonBase<T> : IDisposable
        where T : class
    {
        private bool IsDisposing; // To detect redundant calls

        /// <summary>
        /// A synchronization root that is commonly used for cross-thread operations.
        /// </summary>
        protected static readonly object SyncRoot = new object();

        /// <summary>
        /// The static, singleton instance reference.
        /// </summary>
        protected static readonly Lazy<T> LazyInstance = new Lazy<T>(
            valueFactory: () => { return Activator.CreateInstance(typeof(T), true) as T; }, 
            isThreadSafe: true);

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonBase{T}"/> class.
        /// </summary>
        protected SingletonBase()
        {
            // placeholder
        }

        /// <summary>
        /// Disposes the internal singleton instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the job
        /// </summary>
        /// <param name="disposeManaged"></param>
        protected virtual void Dispose(bool disposeManaged)
        {
            if (IsDisposing) return;

            // free managed resources
            if (LazyInstance != null)
            {
                try
                {
                    var disposableInstance = LazyInstance.Value as IDisposable;
                    if (disposableInstance != null)
                    {
                        disposableInstance.Dispose();
                    }
                }
                catch
                {
                    // swallow
                }
            }

            IsDisposing = true;
        }

        /// <summary>
        /// Gets the instance that this singleton represents.
        /// If the instance is null, it is constructed and assigned when this member is accessed.
        /// </summary>
        public static T Instance
        {
            get
            {
                return LazyInstance.Value;
            }
        }
    }
}
