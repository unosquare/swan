using System;

namespace Swan
{
    /// <summary>
    /// Defines a set of bitwise standard terminal writers.
    /// </summary>
    [Flags]
    public enum TerminalWriters
    {
        /// <summary>
        /// Prevents output
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Writes to the Console.Out
        /// </summary>
        StandardOutput = 1,
        
        /// <summary>
        /// Writes to the Console.Error
        /// </summary>
        StandardError = 2,
        
        /// <summary>
        /// Writes to the System.Diagnostics.Debug
        /// </summary>
        Diagnostics = 4,
        
        /// <summary>
        /// Writes to all possible terminal writers
        /// </summary>
        All = StandardOutput | Diagnostics | StandardError,
        
        /// <summary>
        /// The error and debug writers
        /// </summary>
        ErrorAndDebug = StandardError | Diagnostics,
        
        /// <summary>
        /// The output and debug writers
        /// </summary>
        OutputAndDebug = StandardOutput | Diagnostics,
    }
}
