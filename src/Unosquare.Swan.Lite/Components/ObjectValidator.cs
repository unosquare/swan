namespace Unosquare.Swan.Components
{
    using System;
    using System.Linq;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Abstractions;

    /// <summary>
    /// Represents an object validator. 
    /// </summary>
    /// <example>
    /// The following code describes how to perform a simple object validation.
    /// <code>
    /// using Unosquare.Swan.Components;
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
    /// using Unosquare.Swan.Components;
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
        private readonly ConcurrentDictionary<Type, List<Tuple<Delegate, string>>> _predicates =
            new ConcurrentDictionary<Type, List<Tuple<Delegate, string>>>();

        /// <summary>
        /// Validates an object given the specified validators and attributes.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns cref="ObjectValidationResult">A validation result. </returns>
        public ObjectValidationResult Validate<T>(T obj)
        {
            var errorList = new ObjectValidationResult();
            ValidateObject(obj, false, errorList.Add);

            return errorList;
        }

        /// <summary>
        /// Validates an object given the specified validators and attributes.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">obj.</exception>
        public bool IsValid<T>(T obj) => ValidateObject(obj);

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

            existing.Add(Tuple.Create((Delegate) predicate, message));
        }

        private bool ValidateObject<T>(T obj, bool returnOnError = true, Action<string, string> action = null)
        {   
            if (Equals(obj, null))
                throw new ArgumentNullException(nameof(obj));
           
            if (_predicates.ContainsKey(typeof(T)))
            {
                foreach (var validation in _predicates[typeof(T)])
                {
                    if ((bool) validation.Item1.DynamicInvoke(obj)) continue;

                    action?.Invoke(string.Empty, validation.Item2);
                    if (returnOnError) return false;
                }
            }

            var properties = Runtime.AttributeCache.RetrieveFromType<T>(typeof(IValidator));

            foreach (var prop in properties)
            {
                foreach (var attribute in prop.Value)
                {
                    var val = (IValidator) attribute;

                    if (val.IsValid(prop.Key.GetValue(obj, null))) continue;

                    action?.Invoke(prop.Key.Name, val.ErrorMessage);
                    if (returnOnError) return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Defines a validation result containing all validation errors and their properties.
    /// </summary>
    public class ObjectValidationResult
    {
        /// <summary>
        /// A list of errors.
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        /// <summary>
        /// <c>true</c> if there are no errors; otherwise, <c>false</c>.
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Adds an error with a specified property name.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="errorMessage">The error message.</param>
        public void Add(string propertyName, string errorMessage) =>
            Errors.Add(new ValidationError {ErrorMessage = errorMessage, PropertyName = propertyName});

        /// <summary>
        /// Defines a validation error.
        /// </summary>
        public class ValidationError
        {
            /// <summary>
            /// The property name.
            /// </summary>
            public string PropertyName { get; set; }

            /// <summary>
            /// The message error.
            /// </summary>
            public string ErrorMessage { get; set; }
        }
    }
}