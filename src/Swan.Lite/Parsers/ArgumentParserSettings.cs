using System;

namespace Swan.Parsers
{
    /// <summary>
    /// Provides settings for <see cref="ArgumentParser"/>.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.).
    /// </summary>
    public class ArgumentParserSettings
    {
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
        /// <value>
        ///   <c>true</c> if [case sensitive]; otherwise, <c>false</c>.
        /// </value>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether perform case sensitive comparisons of <i>values</i>.
        /// Note that case insensitivity only applies to <i>values</i>, not the parameters.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [case insensitive enum values]; otherwise, <c>false</c>.
        /// </value>
        public bool CaseInsensitiveEnumValues { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the parser shall move on to the next argument and ignore the given argument if it
        /// encounter an unknown arguments.
        /// </summary>
        /// <value>
        ///   <c>true</c> to allow parsing the arguments with different class options that do not have all the arguments.
        /// </value>
        /// <remarks>
        /// This allows fragmented version class parsing, useful for project with add-on where add-ons also requires command line arguments but
        /// when these are unknown by the main program at build time.
        /// </remarks>
        public bool IgnoreUnknownArguments { get; set; } = true;

        internal StringComparison NameComparer => CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }
}