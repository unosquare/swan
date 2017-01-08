using System;

namespace Unosquare.Swan
{
    /// <summary>
    /// An attribute used to help conversion structs back and forth into arrays of bytes via
    /// extension methods included in this library ToStruct and ToBytes.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class StructEndiannessAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StructEndiannessAttribute"/> class.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        public StructEndiannessAttribute(Endianness endianness)
        {
            Endianness = endianness;
        }

        /// <summary>
        /// Gets the endianness.
        /// </summary>
        public Endianness Endianness
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Models an option specification.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ArgumentOptionAttribute : Attribute
    {
        private string _setName;

        private ArgumentOptionAttribute(string shortName, string longName)
        {
            if (shortName == null) throw new ArgumentNullException(nameof(shortName));
            if (longName == null) throw new ArgumentNullException(nameof(longName));

            ShortName = shortName;
            LongName = longName;

            _setName = string.Empty;
            Separator = '\0';
        }

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

        /// <summary>
        /// Gets long name of this command line option. This name is usually a single English word.
        /// </summary>
        public string LongName { get; }

        /// <summary>
        /// Gets a short name of this command line option, made of one character.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets or sets the option's mutually exclusive set name.
        /// </summary>
        public string SetName
        {
            get { return _setName; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _setName = value;
            }
        }

        /// <summary>
        /// When applying attribute to <see cref="System.Collections.Generic.IEnumerable{T}"/> target properties,
        /// it allows you to split an argument and consume its content as a sequence.
        /// </summary>
        public char Separator { get; set; }

        /// <summary>
        /// Gets or sets mapped property default value.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a command line option is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a short description of this command line option. Usually a sentence summary.
        /// </summary>
        public string HelpText { get; set; }
    }
}
