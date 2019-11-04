using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Swan.Reflection;

namespace Swan.Validators
{
    /// <summary>
    /// Represents an object validator. 
    /// </summary>
    /// <example>
    /// The following code describes how to perform a simple object validation.
    /// <code>
    /// using Swan.Validators;
    /// 
    /// class Example
    /// {
    ///     public static void Main()
    ///     {
    ///         // create an instance of ObjectValidator
    ///         var obj = new ObjectValidator();
    ///         
    ///         // Add a validation to the 'Simple' class with a custom error message
    ///         obj.AddValidator&lt;Simple&gt;(x => 
    ///             !string.IsNullOrEmpty(x.Name), "Name must not be empty");
    ///         
    ///         // check if object is valid
    ///         var res = obj.IsValid(new Simple { Name = "Name" });
    ///     }
    ///     
    ///     class Simple
    ///     {
    ///         public string Name { get; set; }
    ///     }
    /// }
    /// </code>
    /// 
    /// The following code shows of to validate an object with a custom validator and some attributes using the Runtime ObjectValidator singleton.
    /// <code>
    /// using Swan.Validators;
    /// 
    /// class Example
    /// {
    ///     public static void Main()
    ///     {
    ///         // create an instance of ObjectValidator
    ///         Runtime.ObjectValidator
    ///         .AddValidator&lt;Simple&gt;(x => 
    ///             !x.Name.Equals("Name"), "Name must not be 'Name'");
    ///             
    ///         // validate object
    ///         var res =  Runtime.ObjectValidator
    ///         .Validate(new Simple{ Name = "name", Number = 5, Email ="email@mail.com"})
    ///     }
    ///     
    ///     class Simple
    ///     {
    ///         [NotNull]
    ///         public string Name { get; set; }
    ///         
    ///         [Range(1, 10)]
    ///         public int Number { get; set; }
    ///         
    ///         [Email]
    ///         public string Email { get; set; }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ObjectValidator
    {
        private static readonly Lazy<ObjectValidator> LazyInstance = new Lazy<ObjectValidator>(() => new ObjectValidator());

        private readonly ConcurrentDictionary<Type, List<Tuple<Delegate, string>>> _predicates =
            new ConcurrentDictionary<Type, List<Tuple<Delegate, string>>>();
        
        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public static ObjectValidator Current => LazyInstance.Value;

        /// <summary>
        /// Validates an object given the specified validators and attributes.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="target">The object.</param>
        /// <returns cref="ObjectValidationResult">A validation result. </returns>
        public ObjectValidationResult Validate<T>(T target)
        {
            var errorList = new ObjectValidationResult();
            ValidateObject(target, false, errorList.Add);

            return errorList;
        }

        /// <summary>
        /// Validates an object given the specified validators and attributes.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="target">The object.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">obj.</exception>
        public bool IsValid<T>(T target) => ValidateObject(target);

        /// <summary>
        /// Adds a validator to a specific class.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="predicate">The predicate that will be evaluated.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentNullException">
        /// predicate
        /// or
        /// message.
        /// </exception>
        public void AddValidator<T>(Predicate<T> predicate, string message)
            where T : class
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(message);

            if (!_predicates.TryGetValue(typeof(T), out var existing))
            {
                existing = new List<Tuple<Delegate, string>>();
                _predicates[typeof(T)] = existing;
            }

            existing.Add(Tuple.Create((Delegate)predicate, message));
        }

        private bool ValidateObject<T>(T obj, bool returnOnError = true, Action<string, string>? action = null)
        {
            if (Equals(obj, null))
                throw new ArgumentNullException(nameof(obj));

            if (_predicates.ContainsKey(typeof(T)))
            {
                foreach (var (@delegate, value) in _predicates[typeof(T)])
                {
                    if ((bool)@delegate.DynamicInvoke(obj)) continue;

                    action?.Invoke(value, string.Empty);
                    if (returnOnError) return false;
                }
            }

            var properties = AttributeCache.DefaultCache.Value.RetrieveFromType<T, IValidator>();

            foreach (var prop in properties)
            {
                foreach (var attribute in prop.Value)
                {
                    var val = (IValidator)attribute;

                    if (val.IsValid(prop.Key.GetValue(obj, null))) continue;

                    action?.Invoke(val.ErrorMessage, prop.Key.Name);
                    if (returnOnError) return false;
                }
            }

            return true;
        }
    }
}
