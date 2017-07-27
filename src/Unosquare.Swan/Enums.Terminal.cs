namespace Unosquare.Swan
{
    using System;

    /// <summary>
    /// Defines a set of bitwise standard terminal writers
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
        OutputAndDebug = StandardOutput | Diagnostics
    }

    /// <summary>
    /// Defines the bitwise flags to determine
    /// which types of messages get printed on the current console
    /// </summary>
    [Flags]
    public enum LogMessageType
    {
        /// <summary>
        /// The none message type
        /// </summary>
        None = 0,

        /// <summary>
        /// The information message type
        /// </summary>
        Info = 1,

        /// <summary>
        /// The debug message type
        /// </summary>
        Debug = 2,

        /// <summary>
        /// The trace message type
        /// </summary>
        Trace = 4,

        /// <summary>
        /// The error message type
        /// </summary>
        Error = 8,

        /// <summary>
        /// The warning message type
        /// </summary>
        Warning = 16,
    }
}
