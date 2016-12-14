namespace Unosquare.Swan
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
        public int ExitCode { get; protected set; }

        /// <summary>
        /// Gets the text of the standard output.
        /// </summary>
        public string StandardOutput { get; protected set; }

        /// <summary>
        /// Gets the text of the standard error.
        /// </summary>
        public string StandardError { get; protected set; }
    }
}
