namespace Unosquare.Swan
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods to help create external processes.
    /// </summary>
    static public class ProcessHelper
    {
        /// <summary>
        /// Defines a delegate to handle binary data reception from a process
        /// </summary>
        /// <param name="processData">The process data.</param>
        /// <param name="process">The process.</param>
        public delegate void ProcessDataReceivedCallback(byte[] processData, Process process);

        /// <summary>
        /// Copies the stream asynchronously.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="baseStream">The source stream.</param>
        /// <param name="onDataCallback">The on data callback.</param>
        /// <param name="syncEvents">if set to <c>true</c> [synchronize events].</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        private static async Task<int> CopyStreamAsync(Process process, Stream baseStream, ProcessDataReceivedCallback onDataCallback, bool syncEvents, CancellationToken ct)
        {
            return await Task.Factory.StartNew(async () =>
            {
                var swapBuffer = new byte[2048];
                var readCount = -1;
                var totalCount = 0;

                while (ct.IsCancellationRequested == false)
                {
                    try
                    {
                        if (process.HasExited || process.WaitForExit(1) == true)
                            break;

                        readCount = await baseStream.ReadAsync(swapBuffer, 0, swapBuffer.Length, ct);
                        if (readCount <= 0) continue;
                        if (onDataCallback != null)
                        {
                            var eventBuffer = swapBuffer.Skip(0).Take(readCount).ToArray();
                            var eventTask = Task.Factory.StartNew(() => { onDataCallback.Invoke(eventBuffer, process); });
                            if (syncEvents) eventTask.Wait(ct);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }

                return totalCount;
            }).Unwrap();

        }

        /// <summary>
        /// Runs an external process asynchronously.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="onOutputData">The on output data.</param>
        /// <param name="onErrorData">The on error data.</param>
        /// <param name="syncEvents">if set to <c>true</c> the next data callback will wait until the current one completes.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public static async Task<int> RunProcessAsync(string filename, string arguments, ProcessDataReceivedCallback onOutputData, ProcessDataReceivedCallback onErrorData, bool syncEvents, CancellationToken ct)
        {
            var task = Task.Factory.StartNew(() =>
            {
                var process = new Process
                {
                    EnableRaisingEvents = false,
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = arguments,
                        CreateNoWindow = true,
                        FileName = filename,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                process.Start();
                process.StandardError.DiscardBufferedData();
                process.StandardOutput.DiscardBufferedData();

                var readTasks = new Task[2];
                readTasks[0] = CopyStreamAsync(process, process.StandardOutput.BaseStream, onOutputData, syncEvents, ct);
                readTasks[1] = CopyStreamAsync(process, process.StandardError.BaseStream, onErrorData, syncEvents, ct);

                try
                {
                    Task.WaitAll(readTasks, ct);
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    while (ct.IsCancellationRequested == false)
                    {
                        if (process.HasExited)
                            break;

                        if (process.WaitForExit(5))
                            break;
                    }

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
                    if (process.HasExited)
                        return process.ExitCode;
                    else
                        return -1;
                }
                catch
                {
                    return -1;
                }
            });

            return await task;
        }
    }
}
