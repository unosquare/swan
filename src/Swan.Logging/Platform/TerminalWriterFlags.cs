﻿namespace Swan.Platform;

using System;

/// <summary>
/// Defines a set of bitwise standard terminal writers.
/// </summary>
[Flags]
public enum TerminalWriterFlags
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
    /// Writes to all possible terminal writers
    /// </summary>
    All = StandardOutput | StandardError,
}
