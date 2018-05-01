#if !NET452
namespace Unosquare.Swan.Abstractions
{
    /// <summary>
    /// Mimic a Windows ServiceBase class. Useful to keep compatibility with applications
    /// running as services in OS different to Windows.
    /// </summary>
    public abstract class ServiceBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the service can be stopped once it has started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can stop; otherwise, <c>false</c>.
        /// </value>
        public bool CanStop { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the service should be notified when the system is shutting down.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can shutdown; otherwise, <c>false</c>.
        /// </value>
        public bool CanShutdown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the service can be paused and resumed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can pause and continue; otherwise, <c>false</c>.
        /// </value>
        public bool CanPauseAndContinue { get; set; }

        /// <summary>
        /// Gets or sets the exit code.
        /// </summary>
        /// <value>
        /// The exit code.
        /// </value>
        public int ExitCode { get; set; }

        /// <summary>
        /// Indicates whether to report Start, Stop, Pause, and Continue commands in the event log.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic log]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoLog { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public string ServiceName { get; set; }

        /// <summary>
        /// Stops the executing service.
        /// </summary>
        public void Stop()
        {
            if (!CanStop) return;

            CanStop = false;
            OnStop();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected virtual void OnStart(string[] args)
        {
            // do nothing
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected virtual void OnStop()
        {
            // do nothing
        }
    }
}
#endif