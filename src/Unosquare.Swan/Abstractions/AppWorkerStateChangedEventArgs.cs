namespace Unosquare.Swan.Abstractions
{
    using System;

    /// <summary>
    /// Enumerates the different Application Worker States
    /// </summary>
    public enum AppWorkerState
    {
        /// <summary>
        /// The stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// The running
        /// </summary>
        Running,
    }

    /// <summary>
    /// Represents event arguments whenever the state of an application service changes
    /// </summary>
    public class AppWorkerStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state to which the application service changed.
        /// </summary>
        public AppWorkerState NewState { get; private set; }

        /// <summary>
        /// Gets the old state.
        /// </summary>
        public AppWorkerState OldState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppWorkerStateChangedEventArgs" /> class.
        /// </summary>
        /// <param name="oldState">The old state.</param>
        /// <param name="newState">The new state.</param>
        public AppWorkerStateChangedEventArgs(AppWorkerState oldState, AppWorkerState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    /// <summary>
    /// An event handler that is called whenever the state of an application service is changed
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="AppWorkerStateChangedEventArgs"/> instance containing the event data.</param>
    public delegate void AppWorkerStateChangedEventHandler(object sender, AppWorkerStateChangedEventArgs e);
}
