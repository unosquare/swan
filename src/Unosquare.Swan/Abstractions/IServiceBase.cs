namespace Unosquare.Swan.Abstractions
{
    /// <summary>
    /// Mimic a Windows ServiceBase class. Useful to keep compatibility with applications
    /// running as services in OS different to Windows.
    /// </summary>
    public interface IServiceBase
    {
        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">The arguments.</param>
        void OnStart(string[] args);

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        void OnStop();
    }
}
