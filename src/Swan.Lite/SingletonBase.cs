namespace Swan
{
    using Swan.Reflection;
    using System;

    /// <summary>
    /// Represents a singleton pattern abstract class.
    /// </summary>
    /// <typeparam name="T">The type of class.</typeparam>
    public abstract class SingletonBase<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// The static, singleton instance reference.
        /// </summary>
        protected static readonly Lazy<T> LazyInstance = new(
            valueFactory: () => TypeManager.CreateInstance<T>()
                ?? throw new MissingMethodException(typeof(T).Name, ".ctor"),
            isThreadSafe: true);

        private bool _isDisposing; // To detect redundant calls

        /// <summary>
        /// Gets the instance that this singleton represents.
        /// If the instance is null, it is constructed and assigned when this member is accessed.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static T Instance => LazyInstance.Value;

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// Call the GC.SuppressFinalize if you override this method and use
        /// a non-default class finalizer (destructor).
        /// </summary>
        /// <param name="disposeManaged"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposeManaged)
        {
            if (_isDisposing) return;

            _isDisposing = true;

            // free managed resources
            if (LazyInstance == null) return;

            try
            {
                var disposableInstance = LazyInstance.Value as IDisposable;
                disposableInstance?.Dispose();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // swallow
            }
        }
    }
}