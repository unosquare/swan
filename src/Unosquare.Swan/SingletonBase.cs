namespace Unosquare.Swan
{
    using System;

    /// <summary>
    /// Represents a singleton pattern abstract class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonBase<T> : IDisposable
        where T : class
    {
        protected static readonly object SyncRoot = new object();

        /// <summary>
        /// The static, singleton instance reference.
        /// </summary>
        protected static T Instance;

        protected SingletonBase()
        {
            // placeholder
        }

        /// <summary>
        /// Disposes the job
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Disposes the job
        /// </summary>
        /// <param name="disposeManaged"></param>
        protected virtual void Dispose(bool disposeManaged)
        {
            if (_disposedValue) return;

            // free managed resources
            if (Instance != null)
            {
                Instance = null;
            }

            _disposedValue = true;
        }

        /// <summary>
        /// Gets the instance that this singleton represents.
        /// If the instance is null, it is constructed ans assigned when this member is accessed.
        /// </summary>
        public static T Current
        {
            get
            {
                lock (SyncRoot)
                {
                    return Instance ?? (Instance = Activator.CreateInstance(typeof(T), true) as T);
                }
            }
        }
    }
}
