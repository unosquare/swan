using System;
using System.IO;

namespace Unosquare.Swan.Runtime
{
    /// <summary>
    /// Provides settings for <see cref="CmdArgsParser"/>.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.)
    /// </summary>
    public class ParserSettings : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserSettings"/> class.
        /// </summary>
        public ParserSettings()
        {
            CaseSensitive = true;
            CaseInsensitiveEnumValues = false;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ParserSettings"/> class.
        /// </summary>
        ~ParserSettings()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [write banner].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [write banner]; otherwise, <c>false</c>.
        /// </value>
        public bool WriteBanner { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether perform case sensitive comparisons.
        /// Note that case insensitivity only applies to <i>parameters</i>, not the values
        /// assigned to them (for example, enum parsing).
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether perform case sensitive comparisons of <i>values</i>.
        /// Note that case insensitivity only applies to <i>values</i>, not the parameters.
        /// </summary>
        public bool CaseInsensitiveEnumValues { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="System.IO.TextWriter"/> used for help method output.
        /// Setting this property to null, will disable help screen.
        /// </summary>
        public TextWriter HelpWriter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parser shall move on to the next argument and ignore the given argument if it
        /// encounter an unknown arguments
        /// </summary>
        /// <value>
        /// <c>true</c> to allow parsing the arguments with different class options that do not have all the arguments.
        /// </value>
        /// <remarks>
        /// This allows fragmented version class parsing, useful for project with add-on where add-ons also requires command line arguments but
        /// when these are unknown by the main program at build time.
        /// </remarks>
        public bool IgnoreUnknownArguments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable double dash '--' syntax,
        /// that forces parsing of all subsequent tokens as values.
        /// </summary>
        public bool EnableDashDash { get; set; }
        
        internal StringComparer NameComparer => CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Frees resources owned by the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                HelpWriter?.Dispose();
                HelpWriter = null;

                disposed = true;
            }
        }
    }
}