namespace Unosquare.Swan.Abstractions
{
    /// <summary>
    /// A simple interface for application workers
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// Should start the task immediately and asynchronously
        /// </summary>
        void Start();

        /// <summary>
        /// Should stop the task immediately and synchronously
        /// </summary>
        void Stop();
    }
}
