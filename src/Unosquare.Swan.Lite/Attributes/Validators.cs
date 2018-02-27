namespace Unosquare.Swan.Lite.Attributes
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Validator interface
    /// </summary>
    public interface IValidator
    {
        bool Validate<T>(T value);
    }
    
    /// <summary>
    /// Regex validator
    /// </summary>
    public class MatchAttribute : Attribute, IValidator
    {
        public string Expression { get; }

        public MatchAttribute(string rgx)
        {
            Expression = rgx?? throw new ArgumentNullException(nameof(Expression));
        }

        public bool Validate<T>(T value)
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

    public class NotNullAttribute : Attribute, IValidator
    {
        public bool Validate<T>(T value)
        {
            if (typeof(T).IsValueType())
                return !default(T).Equals(value);

            return !Equals(null, value);
        }
    }
}
