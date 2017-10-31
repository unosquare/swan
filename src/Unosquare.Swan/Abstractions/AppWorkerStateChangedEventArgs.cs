namespace Unosquare.Swan.Abstractions
{
    using System;

    /// <summary>
    /// Represents event arguments whenever the state of an application service changes
    /// </summary>
    public class AppWorkerStateChangedEventArgs : EventArgs
    {
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

        /// <summary>
        /// Gets the state to which the application service changed.
        /// </summary>
        /// <value>
        /// The new state.
        /// </value>
        public AppWorkerState NewState { get; }

        /// <summary>
        /// Gets the old state.
        /// </summary>
        /// <value>
        /// The old state.
        /// </value>
        public AppWorkerState OldState { get; }
    }
}
