using System;

namespace Swan.Parsers
{
    /// <summary>
    /// Models a verb option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class VerbOptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerbOptionAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">name.</exception>
        public VerbOptionAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the name of the verb option.
        /// </summary>
        /// <value>
        /// Name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a short description of this command line verb. Usually a sentence summary.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string HelpText { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"  {Name}\t\t{HelpText}";
    }
}