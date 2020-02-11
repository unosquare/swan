﻿namespace Swan
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods to help create external processes, and efficiently capture the
    /// standard error and standard output streams.
    /// </summary>
    public static class ProcessRunner
    {
        /// <summary>
        /// Defines a delegate to handle binary data reception from the standard 
        /// output or standard error streams from a process.
        /// </summary>
        /// <param name="processData">The process data.</param>
        /// <param name="process">The process.</param>
        public delegate void ProcessDataReceivedCallback(byte[] processData, Process process);

        /// <summary>
        /// Runs the process asynchronously and if the exit code is 0,
        /// returns all of the standard output text. If the exit code is something other than 0
        /// it returns the contents of standard error.
        /// This method is meant to be used for programs that output a relatively small amount of text.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The type of the result produced by this Task.</returns>
        /// <example>
        /// The following code explains how to run an external process using the 
        /// <see cref="GetProcessOutputAsync(string, string, string, CancellationToken)"/> method.
        /// <code>
        /// class Example
        /// {
        ///     using System.Threading.Tasks;
        ///     using Swan;
        ///     
        ///     static async Task Main()
        ///     {
        ///         // execute a process and save its output
        ///          var data = await ProcessRunner.
        ///          GetProcessOutputAsync("dotnet", "--help");
        ///     
        ///         // print the output
        ///         data.WriteLine();
        ///     }
        /// }
        /// </code>
        /// </example>
        public static async Task<string> GetProcessOutputAsync(
            string filename,
            string arguments = "",
            string? workingDirectory = null,
            CancellationToken cancellationToken = default)
        {
            var result = await GetProcessResultAsync(filename, arguments, workingDirectory, cancellationToken: cancellationToken).ConfigureAwait(false);
            return result.ExitCode == 0 ? result.StandardOutput : result.StandardError;
        }

        /// <summary>
        /// Runs the process asynchronously and if the exit code is 0,
        /// returns all of the standard output text. If the exit code is something other than 0
        /// it returns the contents of standard error.
        /// This method is meant to be used for programs that output a relatively small amount 
        /// of text using a different encoder.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The type of the result produced by this Task.
        /// </returns>
        public static async Task<string> GetProcessEncodedOutputAsync(
            string filename, 
            string arguments = "",
            Encoding? encoding = null, 
            CancellationToken cancellationToken = default)
        {
            var result = await GetProcessResultAsync(filename, arguments, null, encoding, cancellationToken).ConfigureAwait(false);
            return result.ExitCode == 0 ? result.StandardOutput : result.StandardError;
        }

        /// <summary>
        /// Executes a process asynchronously and returns the text of the standard output and standard error streams
        /// along with the exit code. This method is meant to be used for programs that output a relatively small
        /// amount of text.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Text of the standard output and standard error streams along with the exit code as a <see cref="ProcessResult" /> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">filename.</exception>
        public static Task<ProcessResult> GetProcessResultAsync(
            string filename, 
            string arguments = "",
            CancellationToken cancellationToken = default) =>
            GetProcessResultAsync(filename, arguments, null, Definitions.CurrentAnsiEncoding, cancellationToken);

        /// <summary>
        /// Executes a process asynchronously and returns the text of the standard output and standard error streams
        /// along with the exit code. This method is meant to be used for programs that output a relatively small
        /// amount of text.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Text of the standard output and standard error streams along with the exit code as a <see cref="ProcessResult" /> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">filename.</exception>
        /// <example>
        /// The following code describes how to run an external process using the <see cref="GetProcessResultAsync(string, string, string, Encoding, CancellationToken)" /> method.
        /// <code>
        /// class Example
        /// {
        ///     using System.Threading.Tasks;
        ///     using Swan;
        /// 
        ///     static async Task Main()
        ///     {
        ///         // Execute a process asynchronously
        ///         var data = await ProcessRunner.GetProcessResultAsync("dotnet", "--help");
        /// 
        ///         // print out the exit code
        ///         $"{data.ExitCode}".WriteLine();
        ///
        ///         // print out the output
        ///         data.StandardOutput.WriteLine();
        ///         // and the error if exists
        ///         data.StandardError.Error();
        ///     }
        /// }
        /// </code></example>
        public static async Task<ProcessResult> GetProcessResultAsync(
            string filename, 
            string arguments,
            string? workingDirectory, 
            Encoding? encoding = null, 
            CancellationToken cancellationToken = default)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            if (encoding == null)
                encoding = Definitions.CurrentAnsiEncoding;

            var standardOutputBuilder = new StringBuilder();
            var standardErrorBuilder = new StringBuilder();

            var processReturn = await RunProcessAsync(
                filename,
                arguments,
                workingDirectory,
                (data, proc) => standardOutputBuilder.Append(encoding.GetString(data)),
                (data, proc) => standardErrorBuilder.Append(encoding.GetString(data)),
                encoding,
                true,
                cancellationToken)
                .ConfigureAwait(false);

            return new ProcessResult(processReturn, standardOutputBuilder.ToString(), standardErrorBuilder.ToString());
        }

        /// <summary>
        /// Runs an external process asynchronously, providing callbacks to
        /// capture binary data from the standard error and standard output streams.
        /// The callbacks contain a reference to the process so you can respond to output or
        /// error streams by writing to the process' input stream.
        /// The exit code (return value) will be -1 for forceful termination of the process.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="onOutputData">The on output data.</param>
        /// <param name="onErrorData">The on error data.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="syncEvents">if set to <c>true</c> the next data callback will wait until the current one completes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Value type will be -1 for forceful termination of the process.
        /// </returns>
        public static Task<int> RunProcessAsync(
            string filename,
            string arguments,
            string? workingDirectory,
            ProcessDataReceivedCallback? onOutputData,
            ProcessDataReceivedCallback? onErrorData,
            Encoding encoding,
            bool syncEvents = true,
            CancellationToken cancellationToken = default)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            return Task.Run(() =>
            {
                // Setup the process and its corresponding start info
                var process = new Process
                {
                    EnableRaisingEvents = false,
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = arguments,
                        CreateNoWindow = true,
                        FileName = filename,
                        RedirectStandardError = true,
                        StandardErrorEncoding = encoding,
                        RedirectStandardOutput = true,
                        StandardOutputEncoding = encoding,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    },
                };

                if (!string.IsNullOrWhiteSpace(workingDirectory))
                    process.StartInfo.WorkingDirectory = workingDirectory;

                // Launch the process and discard any buffered data for standard error and standard output
                process.Start();
                process.StandardError.DiscardBufferedData();
                process.StandardOutput.DiscardBufferedData();

                // Launch the asynchronous stream reading tasks
                var readTasks = new Task[2];
                readTasks[0] = CopyStreamAsync(
                    process, 
                    process.StandardOutput.BaseStream, 
                    onOutputData, 
                    syncEvents,
                    cancellationToken);
                readTasks[1] = CopyStreamAsync(
                    process, 
                    process.StandardError.BaseStream, 
                    onErrorData, 
                    syncEvents, 
                    cancellationToken);

                try
                {
                    // Wait for all tasks to complete
                    Task.WaitAll(readTasks, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
                finally
                {
                    // Wait for the process to exit
                    while (cancellationToken.IsCancellationRequested == false)
                    {
                        if (process.HasExited || process.WaitForExit(5))
                            break;
                    }

                    // Forcefully kill the process if it do not exit
                    try
                    {
                        if (process.HasExited == false)
                            process.Kill();
                    }
                    catch
                    {
                        // swallow
                    }
                }

                try
                {
                    // Retrieve and return the exit code.
                    // -1 signals error
                    return process.HasExited ? process.ExitCode : -1;
                }
                catch
                {
                    return -1;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Runs an external process asynchronously, providing callbacks to
        /// capture binary data from the standard error and standard output streams.
        /// The callbacks contain a reference to the process so you can respond to output or
        /// error streams by writing to the process' input stream.
        /// The exit code (return value) will be -1 for forceful termination of the process.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="onOutputData">The on output data.</param>
        /// <param name="onErrorData">The on error data.</param>
        /// <param name="syncEvents">if set to <c>true</c> the next data callback will wait until the current one completes.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Value type will be -1 for forceful termination of the process.</returns>
        /// <example>
        /// The following example illustrates how to run an external process using the 
        /// <see cref="RunProcessAsync(string, string, ProcessDataReceivedCallback, ProcessDataReceivedCallback, bool, CancellationToken)"/>
        /// method.
        /// <code>
        /// class Example
        /// {
        ///     using System.Diagnostics;
        ///     using System.Text;
        ///     using System.Threading.Tasks;
        ///     using Swan;
        ///     
        ///     static async Task Main()
        ///     {
        ///         // Execute a process asynchronously 
        ///         var data = await ProcessRunner
        ///         .RunProcessAsync("dotnet", "--help", Print, Print);
        ///     
        ///         // flush all messages
        ///         Terminal.Flush();
        ///     }
        ///     
        ///     // a callback to print both output or errors
        ///     static void Print(byte[] data, Process proc) =>
        ///         Encoding.GetEncoding(0).GetString(data).WriteLine();
        /// }
        /// </code>
        /// </example>
        public static Task<int> RunProcessAsync(
            string filename, 
            string arguments,
            ProcessDataReceivedCallback onOutputData, 
            ProcessDataReceivedCallback onErrorData, 
            bool syncEvents = true,
            CancellationToken cancellationToken = default)
            => RunProcessAsync(
                filename, 
                arguments, 
                null, 
                onOutputData, 
                onErrorData, 
                Definitions.CurrentAnsiEncoding,
                syncEvents, 
                cancellationToken);

        /// <summary>
        /// Copies the stream asynchronously.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="baseStream">The source stream.</param>
        /// <param name="onDataCallback">The on data callback.</param>
        /// <param name="syncEvents">if set to <c>true</c> [synchronize events].</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Total copies stream.</returns>
        private static Task<ulong> CopyStreamAsync(
            Process process,
            Stream baseStream,
            ProcessDataReceivedCallback? onDataCallback,
            bool syncEvents,
            CancellationToken ct) =>
            Task.Run(async () =>
            {
                // define some state variables
                var swapBuffer = new byte[2048]; // the buffer to copy data from one stream to the next
                ulong totalCount = 0; // the total amount of bytes read
                var hasExited = false;

                while (ct.IsCancellationRequested == false)
                {
                    try
                    {
                        // Check if process is no longer valid
                        // if this condition holds, simply read the last bits of data available.
                        int readCount; // the bytes read in any given event
                        if (process.HasExited || process.WaitForExit(1))
                        {
                            while (true)
                            {
                                try
                                {
                                    readCount = await baseStream.ReadAsync(swapBuffer, 0, swapBuffer.Length, ct);

                                    if (readCount > 0)
                                    {
                                        totalCount += (ulong) readCount;
                                        onDataCallback?.Invoke(swapBuffer.Skip(0).Take(readCount).ToArray(), process);
                                    }
                                    else
                                    {
                                        hasExited = true;
                                        break;
                                    }
                                }
                                catch
                                {
                                    hasExited = true;
                                    break;
                                }
                            }
                        }

                        if (hasExited) break;

                        // Try reading from the stream. < 0 means no read occurred.
                        readCount = await baseStream.ReadAsync(swapBuffer, 0, swapBuffer.Length, ct).ConfigureAwait(false);

                        // When no read is done, we need to let is rest for a bit
                        if (readCount <= 0)
                        {
                            await Task.Delay(1, ct).ConfigureAwait(false); // do not hog CPU cycles doing nothing.
                            continue;
                        }

                        totalCount += (ulong) readCount;
                        if (onDataCallback == null) continue;

                        // Create the buffer to pass to the callback
                        var eventBuffer = swapBuffer.Skip(0).Take(readCount).ToArray();

                        // Create the data processing callback invocation
                        var eventTask = Task.Run(() => onDataCallback.Invoke(eventBuffer, process), ct);

                        // wait for the event to process before the next read occurs
                        if (syncEvents) eventTask.Wait(ct);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        break;
                    }
                }

                return totalCount;
            }, ct);
    }
}
