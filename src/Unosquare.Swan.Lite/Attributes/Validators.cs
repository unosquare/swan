namespace Unosquare.Swan.Lite.Attributes
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Validator interface
    /// </summary>
    public interface IValidator
    {
        bool IsValid<T>(T value);
    }
    
    /// <summary>
    /// Regex validator
    /// </summary>
    public class MatchAttribute : Attribute, IValidator
    {
        /// <summary>
        /// the string regex used to find a match
        /// </summary>
        public string Expression { get; }

        public MatchAttribute(string rgx)
        {
            Expression = rgx?? throw new ArgumentNullException(nameof(Expression));
        }

        public bool IsValid<T>(T value) 
        {
            if (!(value is string))
            {
                throw new InvalidOperationException("Property is not a string");
            }
            
            return Regex.IsMatch(value.ToString(), Expression);
        }
    }

    /// <summary>
    /// Email validator
    /// </summary>
    public class EmailAttribute : MatchAttribute
    {
        private static readonly string _emailRegExp = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

        public EmailAttribute()
            : base(_emailRegExp)
        {
        }
    }

    /// <summary>
    /// A not null validator
    /// </summary>
    public class NotNullAttribute : Attribute, IValidator
    {
        public bool IsValid<T>(T value)
        {
            if (typeof(T).IsValueType())
                return !default(T).Equals(value);

            return !Equals(null, value);
        }
    }

    public class RangeAttribute : Attribute, IValidator
    {
        public object Maximum { get; }
        public object Minimum { get; }
        public Type OperandType { get; }
        public RangeAttribute(int min, int max)
        {
            this.Maximum = max;
            this.Minimum = min;
            this.OperandType = typeof(int);
        }

        public RangeAttribute(double min, double max)
        {
            this.Maximum = max;
            this.Minimum = min;
            this.OperandType = typeof(double);
        }

        public bool IsValid<T>(T value)
        {
            if (Equals(value, null))
                throw new ArgumentNullException(nameof(value));
            
            var max = (IComparable)Maximum;
            var min = (IComparable)Minimum;
            try
            {
                var val = (IComparable)Convert.ChangeType(value, OperandType, CultureInfo.InvariantCulture);
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
