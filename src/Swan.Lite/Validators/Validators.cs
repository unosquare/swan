using System;
using System.Text.RegularExpressions;

namespace Swan.Validators
{
    /// <summary>
    /// Regex validator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MatchAttribute : Attribute, IValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchAttribute" /> class.
        /// </summary>
        /// <param name="regex">A regex string.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentNullException">Expression.</exception>
        public MatchAttribute(string regex, string? errorMessage = null)
        {
            Expression = regex ?? throw new ArgumentNullException(nameof(regex));
            ErrorMessage = errorMessage ?? "String does not match the specified regular expression";
        }

        /// <summary>
        /// The string regex used to find a match.
        /// </summary>
        public string Expression { get; }

        /// <inheritdoc/>
        public string ErrorMessage { get; internal set; }

        /// <inheritdoc/>
        public bool IsValid<T>(T value)
        {
            if (Equals(value, default(T)))
                return false;

            return !(value is string)
                ? throw new ArgumentException("Property is not a string")
                : Regex.IsMatch(value.ToString(), Expression);
        }
    }

    /// <summary>
    /// Email validator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EmailAttribute : MatchAttribute
    {
        private const string EmailRegExp =
            @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAttribute"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public EmailAttribute(string? errorMessage = null)
            : base(EmailRegExp, errorMessage ?? "String is not an email")
        {
        }
    }

    /// <summary>
    /// A not null validator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : Attribute, IValidator
    {
        /// <inheritdoc/>
        public string ErrorMessage => "Value is null";

        /// <inheritdoc/>
        public bool IsValid<T>(T value) => !Equals(default(T), value);
    }

    /// <summary>
    /// A range constraint validator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RangeAttribute : Attribute, IValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class.
        /// Constructor that takes integer minimum and maximum values.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public RangeAttribute(int min, int max)
        {
            if (min >= max)
                throw new InvalidOperationException("Maximum value must be greater than minimum");

            Maximum = max;
            Minimum = min;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class.
        /// Constructor that takes double minimum and maximum values.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public RangeAttribute(double min, double max)
        {
            if (min >= max)
                throw new InvalidOperationException("Maximum value must be greater than minimum");

            Maximum = max;
            Minimum = min;
        }

        /// <inheritdoc/>
        public string ErrorMessage => "Value is not within the specified range";

        /// <summary>
        /// Maximum value for the range.
        /// </summary>
        public IComparable Maximum { get; }

        /// <summary>
        /// Minimum value for the range.
        /// </summary>
        public IComparable Minimum { get; }

        /// <inheritdoc/>
        public bool IsValid<T>(T value)
            => value is IComparable comparable
            ? comparable.CompareTo(Minimum) >= 0 && comparable.CompareTo(Maximum) <= 0
            : throw new ArgumentException(nameof(value));
    }
}
