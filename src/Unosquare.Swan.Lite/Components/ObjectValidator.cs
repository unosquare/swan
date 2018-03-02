namespace Unosquare.Swan.Components
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Attributes;
#if NETSTANDARD1_3 || UWP
    using System.Reflection;
#endif

    /// <summary>
    /// Represents an object validator 
    /// </summary>
    public class ObjectValidator
    {
        private readonly Dictionary<Type, List<Tuple<Delegate, string>>> _predicates =
            new Dictionary<Type, List<Tuple<Delegate, string>>>();

        /// <summary>
        /// Checks if an object is valid based on the custom validator attributes
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="obj">The object</param>
        /// <returns>A bool indicating if it is a valid object</returns>
        public ObjectValidationResult Validate<T>(T obj)
        {
            if (Equals(obj, null))
                throw new ArgumentNullException(nameof(obj));

            var errorList = new ObjectValidationResult();
            ValidateObject(obj, false, errorList.Add);

            return errorList;
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">obj</exception>
        /// <exception cref="InvalidOperationException">There are no validators for this type</exception>
        public bool IsValid<T>(T obj) => ValidateObject(obj);

        /// <summary>
        /// Adds a validator to a specific class
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="predicate">The predicate that will be evaluated</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentNullException">
        /// predicate
        /// or
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

            var properties = Runtime.AttributeCache.Value.RetrieveFromType<T>(typeof(IValidator));

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

    public class ObjectValidationResult
    {
        public List<ValidationError> Errors { get; set; }

        public bool IsValid => !Errors.Any();

        public void Add(string propertyName, string errorMessage) =>
            Errors.Add(new ValidationError {ErrorMessage = errorMessage, PropertyName = errorMessage});

        public class ValidationError
        {
            public string PropertyName { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}