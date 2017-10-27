namespace Unosquare.Swan.Attributes
{
    using System;

    /// <summary>
    /// Models an option specification.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ArgumentOptionAttribute 
        : Attribute
    {
        private string _setName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentOptionAttribute"/> class.
        /// The default long name will be inferred from target property.
        /// </summary>
        public ArgumentOptionAttribute()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentOptionAttribute"/> class.
        /// </summary>
        /// <param name="longName">The long name of the option.</param>
        public ArgumentOptionAttribute(string longName)
            : this(string.Empty, longName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentOptionAttribute"/> class.
        /// </summary>
        /// <param name="shortName">The short name of the option.</param>
        /// <param name="longName">The long name of the option or null if not used.</param>
        public ArgumentOptionAttribute(char shortName, string longName)
            : this(new string(shortName, 1), longName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentOptionAttribute"/> class.
        /// </summary>
        /// <param name="shortName">The short name of the option..</param>
        public ArgumentOptionAttribute(char shortName)
            : this(new string(shortName, 1), string.Empty)
        {
        }
        
        private ArgumentOptionAttribute(string shortName, string longName)
        {
            ShortName = shortName ?? throw new ArgumentNullException(nameof(shortName));
            LongName = longName ?? throw new ArgumentNullException(nameof(longName));

            _setName = string.Empty;
            Separator = '\0';
        }

        /// <summary>
        /// Gets long name of this command line option. This name is usually a single English word.
        /// </summary>
        /// <value>
        /// The long name.
        /// </value>
        public string LongName { get; }

        /// <summary>
        /// Gets a short name of this command line option, made of one character.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        public string ShortName { get; }

        /// <summary>
        /// Gets or sets the option's mutually exclusive set name.
        /// </summary>
        /// <value>
        /// The name of the set.
        /// </value>
        /// <exception cref="ArgumentNullException">value</exception>
        public string SetName
        {
            get => _setName;
            set => _setName = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// When applying attribute to <see cref="System.Collections.Generic.IEnumerable{T}"/> target properties,
        /// it allows you to split an argument and consume its content as a sequence.
        /// </summary>
        public char Separator { get; set; }

        /// <summary>
        /// Gets or sets mapped property default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a command line option is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a short description of this command line option. Usually a sentence summary.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string HelpText { get; set; }
    }
}