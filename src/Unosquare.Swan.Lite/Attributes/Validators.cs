namespace Unosquare.Swan.Attributes
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A simple Validator interface
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Checks if a value is valid
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="value"> The value</param>
        /// <returns>True if it is valid.False if it is not</returns>
        bool IsValid<T>(T value);
    }

    /// <summary>
    /// Regex validator
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MatchAttribute : Attribute, IValidator
    {
        /// <summary>
        /// The string regex used to find a match
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchAttribute"/> class.
        /// </summary>
        /// <param name="rgx"> A regex string</param>
        public MatchAttribute(string rgx)
        {
            Expression = rgx ?? throw new ArgumentNullException(nameof(Expression));
        }

        /// <inheritdoc/>
        public bool IsValid<T>(T value)
        {
            if (value == null)
                return false;

            if (!(value is string))
                throw new InvalidOperationException("Property is not a string");

            return Regex.IsMatch(value.ToString(), Expression);
        }
    }

    /// <summary>
    /// Email validator
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EmailAttribute : MatchAttribute
    {
        private static readonly string _emailRegExp =
            @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAttribute"/> class.
        /// </summary>
        public EmailAttribute()
            : base(_emailRegExp)
        {
        }
    }

    /// <summary>
    /// A not null validator
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : Attribute, IValidator
    {
        /// <inheritdoc/>
        public bool IsValid<T>(T value)
        {
            if (typeof(T).IsValueType())
                return !default(T).Equals(value);

            return !Equals(null, value);
        }
    }

    /// <summary>
    /// A range constraint validator
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RangeAttribute : Attribute, IValidator
    {
        /// <summary>
        /// Maximum value for the range
        /// </summary>
        public object Maximum { get; }

        /// <summary>
        /// Minimum value for the range
        /// </summary>
        public object Minimum { get; }

        /// <summary>
        ///  Gets the type of the <see cref="Minimum"/> and <see cref="Maximum"/> values
        /// </summary>
        public Type OperandType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class.
        /// Constructor that takes integer minimum and maximum values
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        public RangeAttribute(int min, int max)
        {
            if (min >= max)
                throw new InvalidOperationException("Maximum value must be greater than minimum");

            Maximum = max;
            Minimum = min;
            OperandType = typeof(int);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class.
        /// Constructor that takes double minimum and maximum values
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        public RangeAttribute(double min, double max)
        {
            if (min >= max)
                throw new InvalidOperationException("Maximum value must be greater than minimum");

            Maximum = max;
            Minimum = min;
            OperandType = typeof(double);
        }

        /// <inheritdoc/>
        public bool IsValid<T>(T value)
        {
            if (Equals(value, null))
                throw new ArgumentNullException(nameof(value));

            var max = (IComparable) Maximum;
            var min = (IComparable) Minimum;
            try
            {
                var val = (IComparable) Convert.ChangeType(value, OperandType, CultureInfo.InvariantCulture);
                return min.CompareTo(val) <= 0 && max.CompareTo(val) >= 0;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}