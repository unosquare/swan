#if !UWP
namespace Unosquare.Swan.Components
{
    /// <summary>
    /// Represents the text of the standard output and standard error
    /// of a process, including its exit code.
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessResult" /> class.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        /// <param name="standardOutput">The standard output.</param>
        /// <param name="standardError">The standard error.</param>
        public ProcessResult(int exitCode, string standardOutput, string standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        /// <value>
        /// The exit code.
        /// </value>
        public int ExitCode { get; }

        /// <summary>
        /// Gets the text of the standard output.
        /// </summary>
        /// <value>
        /// The standard output.
        /// </value>
        public string StandardOutput { get; }

        /// <summary>
        /// Gets the text of the standard error.
        /// </summary>
        /// <value>
        /// The standard error.
        /// </value>
        public string StandardError { get; }
    }
}
#endif