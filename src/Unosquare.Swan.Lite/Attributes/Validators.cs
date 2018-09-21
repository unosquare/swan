namespace Unosquare.Swan.Attributes
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A simple Validator interface.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// The error message.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Checks if a value is valid.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value"> The value.</param>
        /// <returns>True if it is valid.False if it is not.</returns>
        bool IsValid<T>(T value);
    }

    /// <summary>
    /// Regex validator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MatchAttribute : Attribute, IValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchAttribute" /> class.
        /// </summary>
        /// <param name="rgx">A regex string.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentNullException">Expression.</exception>
        public MatchAttribute(string rgx, string errorMessage = null)
        {
            Expression = rgx ?? throw new ArgumentNullException(nameof(Expression));
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
            if (value == null)
                return false;

            if (!(value is string))
                throw new ArgumentException("Property is not a string");

            return Regex.IsMatch(value.ToString(), Expression);
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
        /// Initializes a new instance of the <see cref="EmailAttribute" /> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public EmailAttribute(string errorMessage = null)
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